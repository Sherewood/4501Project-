using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;

/* destination reached event definition */
[System.Serializable]
public class DestinationReachedEvent : UnityEvent { }

/* 
  This enum will be used for switching between spline-based/physically-based movement modes 
 
  Will also be used for providing a safe default when the spline's attempt to use unity pathfinding doesn't work properly
 */
public enum MovementMode
{
    //used for spline-based movement
    //might be removed after prototype because I don't like how it's turned out, but for now all movement
    //from point A to point B for individual units will try to use this.
    MODE_SPLINE,
    //used for physics-based movement (flocking, steering, arrival, etc.)
    //should be activated for units that use the flocking behaviour
    MODE_PHYSICAL,
    //fall back to if other modes don't work to prevent unit not moving/error
    //use warning log to indicate fallback to default movement if applicable...
    MODE_DEFAULT,
}

/* from animation microdemo, modified to serve this component */
public class CRSpline {

    // Points, tangents, and segment lengths that form the piecewise spline
    private Vector3[] point;
    private Vector3[] tangent; // tangent[i] is the curve tangent at point[i]
    private float[] length; // length[i] is the length of curve segments from point 0 up to point 'i'

    //configure spline using a given NavMeshPath's corner points as control points
    //call this before using the spline...
    public bool InitSpline(NavMeshPath splinePath)
    {
        int numControlPoints = splinePath.corners.Length;

        //path needs at least 2 endpoints
        if(numControlPoints <= 1)
        {
            Debug.LogError("Error: Invalid path computed - only one or less endpoints");
            return false;
        }

        point = new Vector3[numControlPoints];
        tangent = new Vector3[numControlPoints];
        length = new float[numControlPoints];

        //get first and last corner, and use the direction between them to set up the tangents
        point[0] = splinePath.corners[0];
        point[numControlPoints-1] = splinePath.corners[numControlPoints - 1];

        Vector3 destDirection = Vector3.Normalize(point[numControlPoints - 1] - point[0]);

        //set first tangent to direction from start to end, and last tangent the same
        //originally inverted last tangent, but that led to the unit turning away from its destination at the end of spline movement
        tangent[0] = destDirection;
        tangent[numControlPoints - 1] = destDirection;

        //first length is zero
        length[0] = 0;

        //set up intermediate control points
        for (int i = 1; i < splinePath.corners.Length-1; i++)
        {
            point[i] = splinePath.corners[i];
            // Tangent computed according to Catmull-Rom formula
            tangent[i] = (splinePath.corners[i + 1] - splinePath.corners[i - 1]) / 2.0f;
            // Length is the length of line segments so far
            length[i] = length[i - 1] + Vector3.Distance(point[i],point[i - 1]);
        }

        // The last length is the total length of the curve
        length[numControlPoints - 1] = length[numControlPoints-2] + Vector3.Distance(point[numControlPoints - 1], point[numControlPoints - 2]);

        return true;
    }

    // Interpolate this Catmull-Rom Spline according to parameter t = [0, 1]
    public Vector3 CRSplineInterp(float t)
    {
        // Clamp parameter to the valid range
        if (t < 0.0f) { t = 0.0f; }
        if (t > 1.0f) { t = 1.0f; }

        float tlen = GetCompletedLength(t);

        // Find out which curve segment we are in
        int curve = 0;
        for (int i = 0; (i < length.Length) && (tlen > length[i]); i++)
        {
            curve = i;
        }

        // If we are at the last point, return the point directly
        if (curve == (length.Length - 1))
        {
            return point[point.Length - 1];
        }

        // Get interpolation parameter inside the selected curve segment
        float s = (tlen - length[curve]) / (length[curve + 1] - length[curve]);

        // Compute Catmull-Rom spline
        float s2 = Mathf.Pow(s, 2);
        float s3 = Mathf.Pow(s, 3);
        Vector3 pos = (2 * s3 - 3 * s2 + 1) * point[curve] +
                      (s3 - 2 * s2 + s) * tangent[curve] +
                      (-2 * s3 + 3 * s2) * point[curve + 1] +
                      (s3 - s2) * tangent[curve + 1];
        return pos;
    }

    /* extra helpers */

    //given parameter t = [0,1], return the (approximate) length of the completed section of the curve
    public float GetCompletedLength(float t)
    {
        // Translate t in [0, 1] to a parameter between [0, total length of curve segments]
        // tlen is the parameter in an approximate arc-length parameterized version of the curve
        // It's approximate because we are using the length of line segments and not of the curve itself
        float tlen = t * length[length.Length - 1];

        return tlen;
    }

    //return the (approximate) full path length of the curve
    public float GetFullPathLength()
    {
        return length[length.Length - 1];
    }

    //returns the last tangent of the curve
    public Vector3 GetLastTangent()
    {
        return tangent[tangent.Length - 1];
    }
}


/* Unit Component */
//Purpose: Control movement (and rotation) of the unit

/* Note: Movement method will have to be updated for final deliverable */

public class Movement : MonoBehaviour
{
    /* callbacks */
    public DestinationReachedEvent DestinationReachedEvent;

    /* movement type */

    //the type of movement to use to update the unit's position
    private MovementMode _movementMode;

    /* spline \/ */

    //todo: add boolean to toggle spline movement on/off? (for working with flocking?)

    private CRSpline _curSpline;

    //timing parameter (range [0,1])
    private float s;

    //spline parameter should be set initially above 0 for proper initial delta when determining the new position for rigidbody MovePosition
    private const float INITIAL_SPLINE_PARAM = 0.01f;

    //rate of change of s
    private float sChangeRate;

    private const float BASE_CHANGE_RATE = 1.0f;

    /* spline ^ */

    //destination (lower priority - via the object itself)
    private Vector3 _destination;
    //destination (high priority - via player command)
    private Vector3 _orderedDestination;

    //destination which changes over time (due to the unit associated with the Transform object moving)
    private Transform _dynamicDestination;

    //true if dynamic destination is ordered
    private bool _isDynamicDestOrdered;

    //true when unit is moving towards destination
    private bool _moving;

    private Quaternion _targetRotation;

    //added offset from the final destination
    //ex: if offset from destination is 1, unit will finish movement order when 1 further unit away from the destination than otherwise
    //totally butchered the sentence ik lmao
    //currently only supported for ordered movement to a static location.
    private float _offsetFromDestination;

    
    //point to return to when ordered to head back
    private Vector3 _returnPoint;

    private Rigidbody _rigidBody;

    private UnitState _unitState;

    /* Configuration */

    public float Speed;
    public float TurnRate;

    // Start is called before the first frame update
    void Start()
    {
        _destination = new Vector3();
        _orderedDestination = new Vector3();
        _moving = false;
        _returnPoint = new Vector3();

        _dynamicDestination = null;
        _isDynamicDestOrdered = false;

        _rigidBody = GetComponent<Rigidbody>();

        _unitState = GetComponent<UnitState>();

        //start up coroutines
        StartCoroutine(InPlaceRotationHandler());

        StartCoroutine(UpdateDynamicDestination());

        _targetRotation = Quaternion.identity;

        //initialize spline-related parameters
        _curSpline = new CRSpline();
        s = 0.0f;
        sChangeRate = 0.0f;
    
    }

    //destroy coroutines at end
    void OnDestroy()
    {
        StopCoroutine(InPlaceRotationHandler());
        StopCoroutine(UpdateDynamicDestination());
    }

    //moved to fixedupdate for better physics
    void FixedUpdate()
    {
        /* recommendation: Send speed to animation script somewhere around here
         * will need to track last known position to determine actual speed of the unit 
         * (not the constant speed param)
         * animation script will then update the corresponding state variable
         */

        if (_moving)
        {
            //get target
            Vector3 target = GetDestination();

            //if nothing to move towards, stop.
            if (target == Vector3.zero)
            {
                _moving = false;
                return;
            }

            //handle movement based on the current movement mode.
            switch (_movementMode) {
                case MovementMode.MODE_SPLINE:
                    SplineMovementUpdate();
                    break;
                case MovementMode.MODE_PHYSICAL:
                    break;
                case MovementMode.MODE_DEFAULT:
                    DefaultMovementUpdate();
                    break;
            }

            if (Vector3.Distance(transform.position, target) < 0.26f + _offsetFromDestination)
            {
                Debug.Log("Destination reached...");
                //terminate movement if destination reached.
                StopMovement(true);
                //report to interested parties that destination has been reached
                DestinationReachedEvent.Invoke();
            }
        }
        //rotation in place for when unit is not in motion (aiming)
        else
        {
            if (_targetRotation != Quaternion.identity)
            {
                _rigidBody.MoveRotation(Quaternion.RotateTowards(transform.rotation, _targetRotation, TurnRate * Time.deltaTime));
            }
        }
    }

    private void SplineMovementUpdate()
    {
        //handling spline movement using animation microdemo code
        //update spline in FixedUpdate because physics is wack otherwise

        //todo: dynamic destination handling: Check if target has moved too far, and recalculate spline if so

        //update time parameter (todo: tune this timing properly...)
        s += Time.deltaTime * sChangeRate;

        //todo: add ease in/out for s

        // Evaluate spline to get the position
        Vector3 newPos = _curSpline.CRSplineInterp(s);

        //todo: extra handling to recalculate the spline if unit ends up too far away from the expected spline position?
        //will prevent "rubber-banding" where unit speed skyrockets once broken free to try and catch up.

        /* not sure if using the rigidbody MovePosition method will allow it to still count as spline movement,
         but the collision detection + rigidbody physics was not working properly otherwise.... */

        //using unnormalized travelDir is suprisingly accurate
        Vector3 travelDir = newPos - this.transform.position;

        _rigidBody.MovePosition(this.transform.position + (newPos - this.transform.position) * Time.deltaTime);
        // Get orientation from tangent along the curve
        Vector3 curve_tan = _curSpline.CRSplineInterp(s + 0.01f) - _curSpline.CRSplineInterp(s);
        curve_tan.Normalize();
        // Check if we are close to the last point along the path
        if (s >= 0.99f)
        {
            // The last point does not have a well-defined tangent, so use the one of the curve
            // might want to revisit, causes unit to turn around at the end of its movement
            curve_tan = _curSpline.GetLastTangent();
        }
        // Create orientation from the tangent
        Quaternion orient = new Quaternion();
        orient.SetLookRotation(curve_tan, Vector3.up);

        // Set unit's orientation
        _targetRotation = orient;
        _rigidBody.MoveRotation(Quaternion.RotateTowards(transform.rotation, _targetRotation, TurnRate * Time.deltaTime));
    }

    //todo: add flocking code here
    //forces added based on whether unit is leader or not.
    private void PhysicsBasedMovmentUpdate()
    {
        //remove when added, obviously
        Debug.LogWarning("Physics based movement is not available!");
    }

    //should not be used intentionally, meant as a fallback
    private void DefaultMovementUpdate()
    {
        //get the target destination
        Vector3 target = GetDestination();
        if(target == Vector3.zero)
        {
            return;
        }
        Vector3 moveDirection = Vector3.Normalize(target - transform.position);

        //move at constant rate
        //could still add proper acceleration here later, but since this is only a fallback method don't see the point in spending time on it.
        _rigidBody.MovePosition(transform.position += moveDirection * Speed * Time.deltaTime);

        //only rotate based on x,z direction
        //works a lot better on flat surfaces, tbd on slanted regions
        moveDirection.y = 0;
        //determine the target rotation
        _targetRotation.SetFromToRotation(new Vector3(0, 0, 1), moveDirection);
        _rigidBody.MoveRotation(Quaternion.RotateTowards(transform.rotation, _targetRotation, TurnRate * Time.deltaTime));
    }


    //helper for determining destination
    private Vector3 GetDestination()
    {
        Vector3 target = new Vector3();
        if (_orderedDestination != Vector3.zero)
        {
            target = _orderedDestination;
        }
        else if(_destination != Vector3.zero)
        {
            target = _destination;
        }

        return target;
    }

    /* destination setters */

    //set ordered destination - should usually be from player command
    public bool SetOrderedDestination(Vector3 orderedDestination, MovementMode movementMode = MovementMode.MODE_DEFAULT, float offsetFromDestination = 0.0f)
    {
        _orderedDestination = orderedDestination;
        _offsetFromDestination = offsetFromDestination;
        _moving = true;

        if (ShouldChangeToMoveState())
        {
            _unitState.SetState(UState.STATE_MOVING);
        }

        return StartMovement(movementMode, true);
    }

    //set destination - not specifically ordered so can be overridden by ordered destination
    public bool SetDestination(Vector3 destination, MovementMode movementMode = MovementMode.MODE_DEFAULT)
    {
        _destination = destination;
        _moving = true;

        if (ShouldChangeToMoveState())
        {
            _unitState.SetState(UState.STATE_MOVING);
        }

        return StartMovement(movementMode, false);
    }

    /* set destination methods for dynamic destinations */

    //note: if rotateOnly is true, only rotating will be done, otherwise both movement and rotation is done
    public bool SetDynamicOrderedDestination(Transform dynamicDestination, bool rotateOnly, MovementMode movementMode = MovementMode.MODE_DEFAULT)
    {
        _dynamicDestination = dynamicDestination;
        _orderedDestination = dynamicDestination.position;

        _isDynamicDestOrdered = true;

        //only enter movement mode if rotateOnly is false.
        if (!rotateOnly)
        {
            _moving = true;

            if (ShouldChangeToMoveState())
            {
                _unitState.SetState(UState.STATE_MOVING);
            }

            if (!StartMovement(movementMode, true))
            {
                return false;
            };
        }

        return true;
    }

    public bool SetDynamicDestination(Transform dynamicDestination, bool rotateOnly, MovementMode movementMode = MovementMode.MODE_DEFAULT)
    {
        _dynamicDestination = dynamicDestination;
        _destination = dynamicDestination.position;

        _isDynamicDestOrdered = false;

        if (!rotateOnly)
        {
            _moving = true;

            if (ShouldChangeToMoveState())
            {
                _unitState.SetState(UState.STATE_MOVING);
            }

            if(!StartMovement(movementMode, false))
            {
                return false;
            };
        }

        return true;
    }

    /* worker-specific stuff */

    //set moving to harvest
    public bool SetOrderedHarvestDestination(Vector3 orderedDestination, MovementMode movementMode = MovementMode.MODE_DEFAULT)
    {
        if(!SetOrderedDestination(orderedDestination, movementMode))
        {
            return false;
        };

        //just straight up forcing the state to 'moving to harvest' could be problematic
        //but only workers will support this component so it won't interfere with any attacking states.
        _unitState.SetState(UState.STATE_MOVING_TO_HARVEST);

        return true;
    }

    //set moving to construct
    public bool SetOrderedConstructionDestination(Vector3 orderedDestination, MovementMode movementMode = MovementMode.MODE_DEFAULT)
    {
        //get the forward offset from the construction component
        Construction constructComp = GetComponent<Construction>();

        //move to the construction site, but stop short according to the offset
        SetOrderedDestination(orderedDestination, movementMode, constructComp.GetConstructionSiteOffset());

        //just straight up forcing the state to 'moving to construct' could be problematic
        //but only workers will support this component so it won't interfere with any attacking states.
        _unitState.SetState(UState.STATE_MOVING_TO_CONSTRUCT);

        return true;
    }



    /* Return point */

    //set point to return to
    public void SetReturnPoint(Vector3 returnPoint)
    {
        _returnPoint = returnPoint;
    }

    //order return to return point
    public bool OrderReturn(float offsetFromDestination = 0.0f, MovementMode movementMode = MovementMode.MODE_DEFAULT)
    {
        _orderedDestination = _returnPoint;
        _offsetFromDestination = offsetFromDestination;
        _moving = true;

        if (ShouldChangeToMoveState())
        {
            _unitState.SetState(UState.STATE_MOVING);
        }

        return StartMovement(movementMode, true);
    }

    //general method to initiate unit movement
    private bool StartMovement(MovementMode movementMode, bool isOrdered)
    {
        //do not start new movement command if movement is not ordered, and ordered movment is in progress
        if (!isOrdered && _orderedDestination != Vector3.zero)
        {
            return false;
        }

        Vector3 dest = isOrdered ? _orderedDestination : _destination;

        //update movement mode to match request
        _movementMode = movementMode;

        //trigger handling for starting movement, based on the specified movement mode
        switch (_movementMode)
        {
            case MovementMode.MODE_SPLINE:
                StartSplineMovement(dest);
                break;
            case MovementMode.MODE_PHYSICAL:
                Debug.LogError("Request to trigger physics-based movement, but that is not ready yet.");
                return false;
            case MovementMode.MODE_DEFAULT:
                Debug.LogError("Request to trigger default movement, but that is not ready yet.");
                return false;
        }

        return true;
    }

    /* Movement Starting Methods */

    //triggers spline-based movement
    private void StartSplineMovement(Vector3 dest)
    {

        //calculate a path in order to get control points for the spline.

        NavMeshPath splinePath = new NavMeshPath();

        //if trying to path to the destination directly fails, need to determine an alternative destination
        //will occur when the target point is not on the navmesh.
        if (!NavMesh.CalculatePath(transform.position, dest, NavMesh.AllAreas, splinePath))
        {
            Debug.Log("Attempted to path to obstructed region, need to try and compute alternative destination");

            if(!FindUnobstructedPath(dest, out splinePath))
            {
                //fallback to default movement if can't make spline movement work...
                Debug.LogWarning("Could not find valid alternative destination, using default movement as fallback.");
                _movementMode = MovementMode.MODE_DEFAULT;
                return;
            }
        };

        //Instantiate the spline
        if (!_curSpline.InitSpline(splinePath)){
            Debug.LogError("Spline initialization failed, cannot begin moving the unit.");

            return;
        }

        //set time parameter to 0
        s = INITIAL_SPLINE_PARAM;

        //calculate rate of change (of s) based on length of spline and speed
        //length of spline taken into account so unit will progress through the spline at the expected physical speed
        sChangeRate = BASE_CHANGE_RATE * (Speed / _curSpline.GetFullPathLength());
    }

    //if the target position is the position of a NavMeshObstacle object, pathfinding will fail, so need to find a position sufficiently offseted
    //such that it is on the NavMesh.
    private bool FindUnobstructedPath(Vector3 dest, out NavMeshPath path)
    {
        path = new NavMeshPath();

        //first, try finding one in direction of our unit
        Vector3 directionToUnit = Vector3.Normalize(transform.position - dest);

        Vector3 offset = new Vector3();

        //if not targetting an object, then there is no size to base off of.
        if(_dynamicDestination == null)
        {
            //if offset from destination was requested, use that offset now to set the destination,
            //then don't include it in 'distance from destination' calculations later.
            if (_offsetFromDestination != 0.0f)
            {
                offset = directionToUnit * _offsetFromDestination;
                _offsetFromDestination = 0.0f;
            }
            else
            {
                offset = directionToUnit * 2.0f; //default value - provide some cushion...
            }
        }
        else
        {
            //Get angle from direction vector to target's forward and side vectors
            Quaternion fromForwardRot = Quaternion.FromToRotation(directionToUnit, _dynamicDestination.forward);

            Quaternion fromSideRot = Quaternion.FromToRotation(directionToUnit, _dynamicDestination.right);

            Vector3 forwardOffset = transform.localScale.z * _dynamicDestination.forward;
            Vector3 sideOffset = transform.localScale.x * _dynamicDestination.right;
            //offset is a combination of the forward and side offsets based on the angle between the target's forward angle and the direction vector
            //probably missing some obvious quaternion magic but whatever

            float forwardYAxisAngle = fromForwardRot.eulerAngles.y;

            //case 1 - angle between 315 and 45 degrees -> apply full forward offset, and side offset in range [315,45] -> [-45,45]/45
            if(forwardYAxisAngle <= 45.0f || forwardYAxisAngle >= 315.0f)
            {
                float localAngle = (forwardYAxisAngle >= 315.0f) ? forwardYAxisAngle - 360.0f : forwardYAxisAngle;

                offset = forwardOffset + sideOffset * (localAngle / 45.0f);
            }
            //case 2 - angle between 45 and 135 degrees -> apply full side offset, and forward offset in range [45,135] -> [-45,45]/-45
            else if(forwardYAxisAngle > 45.0f && forwardYAxisAngle <= 135.0f)
            {
                float localAngle = forwardYAxisAngle - 90.0f;

                offset = forwardOffset * (localAngle / -45.0f) + sideOffset;
            }
            //case 3 - angle between 135 and 225 degrees -> apply full negative forward offset, and side offset in range [135,225] -> [-45,45]/-45
            else if (forwardYAxisAngle > 135.0f && forwardYAxisAngle <= 225.0f)
            {
                float localAngle = forwardYAxisAngle - 180.0f;

                offset = -forwardOffset + sideOffset * (localAngle / -45.0f);
            }
            //case 4 - angle between 225 and 315 degrees -> apply full negative side offest, and forward offset in range [225,315] -> [-45,45]/-45
            else if (forwardYAxisAngle > 225.0f && forwardYAxisAngle <= 315.0f)
            {
                float localAngle = forwardYAxisAngle - 270.0f;

                offset = forwardOffset * (localAngle / -45.0f) - sideOffset;
            }
        }

        //apply offset and try to recalculate path
        dest += offset;

        if(NavMesh.CalculatePath(transform.position, dest, NavMesh.AllAreas, path))
        {
            return true;
        }

        //todo: Try alternative destination in all directions surrounding destination point (will implement if needed)
        //this is suboptimal so might be best to go a different route entirely for unit movement instead of having to deal with stuff like this...

        return false;
    }




    //cease all movement, or just unordered movement if stopOrderedMovement = false
    public void StopMovement(bool stopOrderedMovement)
    {
        if (stopOrderedMovement)
        {
            _orderedDestination = Vector3.zero;
            _isDynamicDestOrdered = false;
        }
        _destination = Vector3.zero;
        //only stop moving to dynamic destination if supposed to stop ordered movement, and/or the dynamic destination was not ordered.
        if (stopOrderedMovement || !_isDynamicDestOrdered)
        {
            _dynamicDestination = null;
        }
        //only stop the movement in general if supposed to stop ordered movement, and/or there is no ordered movement
        if (stopOrderedMovement || _orderedDestination == Vector3.zero)
        {
            _moving = false;
        }

        if (_unitState.GetState() == UState.STATE_MOVING)
        {
            _unitState.SetState(UState.STATE_IDLE);
        }
    }

    //returns true if the movement component is currently carrying out ordered movement
    public bool IsOrderedMovementInProgress()
    {
        return (_orderedDestination != Vector3.zero);
    }

    //return true if based on the current state, the state should transition to "MOVING"
    private bool ShouldChangeToMoveState()
    {
        UState curState = _unitState.GetState();

        bool idleOrHarvesting = (curState == UState.STATE_IDLE || curState == UState.STATE_HARVESTING);

        //attack, guarding, and fortified states should be overidden only if the player orders a change to move state
        bool overridableStatesIfOrdered = IsOrderedMovementInProgress() && (curState == UState.STATE_ATTACKING || curState == UState.STATE_GUARDING || curState == UState.STATE_FORTIFIED);

        return idleOrHarvesting || overridableStatesIfOrdered;
    }

    //handle rotation when the unit is to be stationary
    IEnumerator InPlaceRotationHandler()
    {
        while (true)
        {
            //rotation in place is disabled while movement ongoing for now
            if (_moving)
            {
                yield return null;
            }

            Vector3 target = GetDestination();

            //if nothing to rotate towards, skip
            if (target == Vector3.zero)
            {
                _targetRotation = Quaternion.identity;
                yield return null;
                continue;
            }

            Vector3 direction = Vector3.Normalize(target - transform.position);
            //only rotate based on x,z direction
            //works a lot better on flat surfaces, tbd on slanted regions
            direction.y = 0;
            //determine the target rotation
            _targetRotation.SetFromToRotation(new Vector3(0, 0, 1), direction);

            yield return null;
        }
    }

    //handle updating destination using dynamic destination
    IEnumerator UpdateDynamicDestination()
    {
        while (true)
        {
            if(_dynamicDestination == null)
            {
                yield return null;
                continue;
            }

            //update the respective destination vector to match the dynamic destination
            if (_isDynamicDestOrdered)
            {
                _orderedDestination = _dynamicDestination.position;
            }
            else
            {
                _destination = _dynamicDestination.position;
            }


            yield return null;
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;
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
    //callback to AI Control
    public AIEvent AICallback;

    /* movement type */

    //the type of movement to use to update the unit's position
    private MovementMode _movementMode;

    /* spline \/ */

    //todo: add boolean to toggle spline movement on/off? (for working with flocking?)

    private CRSpline _curSpline;

    //timing parameter (range [0,1])
    private float s;

    //for tweaking initial s value of spline
    //will prob remove later if 0.0f is best (which it prob is with kinematic movement)
    private const float INITIAL_SPLINE_PARAM = 0.0f;

    //rate of change of s
    private float sChangeRate;

    private const float BASE_CHANGE_RATE = 1.0f;

    //represents the percentage of the motion completed in scenarios where the spline has been
    //recalculated before movement is done
    //Example: Finish 50% of the original spline, then recalculate a new spline where its path length ends
    //up equal to that of the remaining section of the original spline -> pco = 0.5 as 50% of the actual path was completed
    private float _pathCompletionOffset = 0.0f;

    //tracking total hermite spline length, needs to be updated properly to set PCO properly
    private float _totalPathLength = 0.0f;

    /* spline ^ */

    //destination (lower priority - via the object itself)
    private Vector3 _destination;

    //random destination for flocking
    private Vector3 _wanderDestination;

    //other variables needed for flocking
    private float _wanderRadius;
    private bool _isWandering;

    public GameObject _flockLeader;

    //destination which changes over time (due to the unit associated with the Transform object moving)
    private Transform _dynamicDestination;
    //true if target destination has changed due to the dynamic destination moving
    private bool _dynamicDestUpdateMade;

    //true when unit is moving towards destination
    public bool _moving;

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

    // animateor

    private animation_Controller _animator;

    // Start is called before the first frame update
    void Start()
    {
        _destination = new Vector3();

        _wanderDestination = new Vector3();
        _wanderRadius = 8.0f;
        _isWandering = false;

        _moving = false;
        _returnPoint = new Vector3();
        _movementMode = MovementMode.MODE_DEFAULT;
        _flockLeader = null;

        _dynamicDestination = null;

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
        //animator
        _animator = this.GetComponent<animation_Controller>();
    
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
            if (_animator != null) _animator.SetAnim("WALK");
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
                    PhysicsBasedMovementUpdate();
                    break;
                case MovementMode.MODE_DEFAULT:
                    DefaultMovementUpdate();
                    break;
            }

            //temporary: include spline timing parameter for reaching destination
            if (Vector3.Distance(transform.position, target) < 0.26f + _offsetFromDestination || s > 1.0f)
            {
                Debug.Log("Destination reached..." + transform.position);
                //terminate movement if destination reached.
                StopMovement();
                //report to interested parties that destination has been reached
                AICallback.Invoke("reachedDestination");
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

    //todo: incorporate path-completion offset
    //will have to apply ease as an offset on s, instead of a replacement of s for that to work.
    private float Ease(float s)
    {
        float t = _pathCompletionOffset + s * (1.0f - _pathCompletionOffset);

        // Clamp parameter to the valid range
        if (t < 0.0f) { t = 0.0f; }
        if (t > 1.0f) { t = 1.0f; }

        //formula from animation microdemo
        float delta = (Mathf.Sin(t * Mathf.PI - Mathf.PI / 2.0f) + 1.0f) / 2.0f - t;

        return s + delta;
    }

    private void SplineMovementUpdate()
    {
        //handling spline movement using animation microdemo code

        //update time parameter (tuned using sChangeRate calculated in StartSplineMovement)
        s += Time.deltaTime * sChangeRate;

        //todo: add ease in/out for s
        float t = Ease(s);

        // Evaluate spline to get the position
        Vector3 newPos = _curSpline.CRSplineInterp(t);

        // just set it to the new position
        //some issues with colliding with units that have significantly more mass, kinematic rigidbody will just pass through.
        transform.position = newPos;
        // Get orientation from tangent along the curve
        Vector3 curve_tan = _curSpline.CRSplineInterp(t + 0.01f) - _curSpline.CRSplineInterp(t);
        curve_tan.Normalize();
        // Check if we are close to the last point along the path
        if (t >= 0.99f)
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
        transform.rotation = Quaternion.RotateTowards(transform.rotation, _targetRotation, TurnRate * Time.deltaTime);

        //dynamic destination handling: Check if target has moved too far, and recalculate spline if so
        //don't bother if t is too high as unit has basically arrived anyways...
        if (_dynamicDestUpdateMade && t < 0.99f)
        {
            HandleDynamicSplineChange(t);
        }
    }

    //todo: add flocking code here
    //forces added based on whether unit is leader or not.
    private void PhysicsBasedMovementUpdate()
    {
        //needs to be physics-based
        if (_rigidBody.isKinematic) _rigidBody.isKinematic = false;
        //get the target destination
        Vector3 target = GetDestination();
        if (target == Vector3.zero)
        {
            return;
        }

        //leader
        if (_flockLeader == null)
        {
            //find the direction to move in and add the force
            Vector3 moveVector = Vector3.zero;
            if(!_isWandering) moveVector = target - transform.position;
            else moveVector = _wanderDestination - transform.position;
            _rigidBody.AddForce(moveVector.normalized * Speed * Time.deltaTime * 5, ForceMode.VelocityChange);
            //adjust the velocity
            //_rigidBody.velocity = _rigidBody.velocity.normalized * Speed;
            //rotate towards the direction of movement
            Quaternion fixRotation = new Quaternion();
            fixRotation.eulerAngles = new Vector3(0, transform.rotation.eulerAngles.y, 0);
            transform.rotation = fixRotation;
            //pick a new spot to wander to when the previous one or the initial destination is reached
            if ((Vector3.Distance(target, transform.position) < 2.0f && !_isWandering) || (Vector3.Distance(_wanderDestination, transform.position) < 2.0f && _isWandering))
            {
                _isWandering = true;
                _wanderDestination = target + new Vector3(Random.Range(-_wanderRadius, _wanderRadius), 0, Random.Range(-_wanderRadius, _wanderRadius));
            }

            //if the unit walks away from the wandering area or a new destination is picked, then it won't wander anymore
            if (Vector3.Distance(target, transform.position) > 10.0f) _isWandering = false;
        }
        //follower
        else
        {
            //initial declaration for all the necessary forces
            Vector3 separationVector = Vector3.zero;
            Vector3 cohesionVector = Vector3.zero;
            Vector3 alignmentVector = transform.forward;
            Vector3 leaderVector = _flockLeader.transform.position - transform.position - _flockLeader.transform.forward * 0.5f;
            //getting all the units in the flock a given unit belongs too (kinda bugged rn)
            UnitController x = GetComponent<UnitController>();
            List<GameObject> flockMembers = GameObject.Find("InternalController").GetComponent<UnitController>().GetFlock(_flockLeader);

            //separation
            int numSeparationNeighbours = 0;
            //sum the positions of the other units in the flock
            foreach (GameObject currentUnit in flockMembers)
            {
                if (currentUnit != this.gameObject && Vector3.Distance(currentUnit.transform.position, transform.position) <= 3)
                {
                    numSeparationNeighbours++;
                    separationVector -= currentUnit.transform.position - transform.position;
                }
            }
            //if there were units, take the mean position of them relative to the unit this script is attached to and normalize it
            /*
            if (numSeparationNeighbours != 0)
            {
                separationVector = separationVector / numSeparationNeighbours;
                cohesionVector += transform.position;
                separationVector = Vector3.Normalize(separationVector);
            }
            */

            //cohesion
            //basically the same as separation but in reverse with a different radius
            int numCohesionNeighbours = 0;
            foreach (GameObject currentUnit in flockMembers)
            {
                if (currentUnit != this.gameObject && Vector3.Distance(currentUnit.transform.position, transform.position) <= 6)
                {
                    numCohesionNeighbours++;
                    cohesionVector += currentUnit.transform.position;
                }
            }
            if (numCohesionNeighbours != 0)
            {
                cohesionVector = cohesionVector / numCohesionNeighbours;
                cohesionVector -= transform.position;
            }

            //alignment
            //same as the other 2 except using transform.forward instead of position
            int numAlignmentNeighbours = 0;
            foreach (GameObject currentUnit in flockMembers)
            {
                if (currentUnit != this.gameObject && Vector3.Distance(currentUnit.transform.position, transform.position) <= 12)
                {
                    numAlignmentNeighbours++;
                    alignmentVector += currentUnit.transform.forward;
                }
            }
            if (numAlignmentNeighbours != 0)
            {
                alignmentVector = alignmentVector / numAlignmentNeighbours;
                //alignmentVector = Vector3.Normalize(alignmentVector);
            }
            alignmentVector.y = 0;

            //adjusting weights of each vector
            separationVector = separationVector * 3.5f;
            cohesionVector = cohesionVector * 0.8f;
            alignmentVector = alignmentVector * 3.0f;
            leaderVector = leaderVector * 2.0f;

            //add the forces and rotate the unit to where it's going, trying to align with the others as well
            Vector3 moveVector = separationVector + cohesionVector + alignmentVector + leaderVector;
            moveVector.y = 0;
            //moveVector = moveVector.normalized * Speed;
            _rigidBody.AddForce(moveVector * Time.deltaTime, ForceMode.VelocityChange);
            //_rigidBody.velocity = _rigidBody.velocity.normalized * Speed;

            Quaternion fixRotation = new Quaternion();
            fixRotation.eulerAngles = new Vector3(0, transform.rotation.eulerAngles.y, 0);
            transform.rotation = fixRotation;

        }
        
        
        /*
        _rigidBody.velocity = Vector3.Normalize(_rigidBody.velocity) * Speed;
        Debug.LogError(_rigidBody.velocity);
        Debug.LogError(target - transform.position);
        Debug.LogError(" ");

        // Create orientation from the tangent
        Quaternion orient = new Quaternion();
        orient.SetLookRotation(alignment, Vector3.up);

        // Set unit's orientation
        _targetRotation = orient;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, _targetRotation, TurnRate * Time.deltaTime);
        */
    }
    //should not be used intentionally, meant as a fallback
    private void DefaultMovementUpdate()
    {
        //needs to be physics-based
        if(_rigidBody.isKinematic) _rigidBody.isKinematic = false;
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

    //handle recalculating a new spline while performing spline movement
    //goal is to keep motion smooth and keep ease-in/out accurate
    private void HandleDynamicSplineChange(float t)
    {
        //get the previously recorded completed length based on PCO and total length
        float PCOSplineLength = _pathCompletionOffset * _totalPathLength;

        //get length of the old spline, and the length of the completed part of the old spline
        float oldSplineLength = _curSpline.GetFullPathLength();
        float oldSplineCompletedLength = _curSpline.GetCompletedLength(t);

        StartSplineMovement(GetDestination());

        //now, get the length of the new spline
        float newSplineLength = _curSpline.GetFullPathLength();

        //calculate the new total path length
        _totalPathLength = _totalPathLength - (oldSplineLength - oldSplineCompletedLength) + newSplineLength;

        //and calculate the new PCO based on the total length of completed path divided by the new total path length
        _pathCompletionOffset = (PCOSplineLength + oldSplineCompletedLength) / _totalPathLength;

        //clamp to [0,1]
        if(_pathCompletionOffset < 0)
        {
            Debug.LogWarning("Spline movement - Somehow calculated path completion offset of: " + _pathCompletionOffset);
            _pathCompletionOffset = 0;
        }
        else if(_pathCompletionOffset > 1.0f)
        {
            Debug.LogWarning("Spline movement - Somehow calculated path completion offset of: " + _pathCompletionOffset);
            _pathCompletionOffset = 1.0f;
        }
    }


    //helper for determining destination
    private Vector3 GetDestination()
    {
        Vector3 target = new Vector3();
        if(_destination != Vector3.zero)
        {
            target = _destination;
        }

        return target;
    }

    /* move to methods */

    //move to - not specifically ordered so can be overridden by ordered destination
    public bool MoveToDestination(Vector3 destination, MovementMode movementMode = MovementMode.MODE_DEFAULT, float offsetFromDestination = 0.0f)
    {
        _destination = destination;
        _offsetFromDestination = offsetFromDestination;
        _moving = true;

        return StartMovement(movementMode);
    }

    /* move to methods for dynamic destinations */

    public bool MoveToDynamicDestination(Transform dynamicDestination, bool rotateOnly, MovementMode movementMode = MovementMode.MODE_DEFAULT)
    {
        _dynamicDestination = dynamicDestination;
        _destination = dynamicDestination.position;

        if (!rotateOnly)
        {
            _moving = true;

            if(!StartMovement(movementMode))
            {
                return false;
            };
        }

        return true;
    }

    /* Return point */

    //set point to return to
    public void SetReturnPoint(Vector3 returnPoint)
    {
        _returnPoint = returnPoint;
    }

    //order return to return point
    public bool MoveToReturnPoint(float offsetFromDestination = 0.0f, MovementMode movementMode = MovementMode.MODE_DEFAULT)
    {
        _destination = _returnPoint;
        _offsetFromDestination = offsetFromDestination;
        _moving = true;

        return StartMovement(movementMode);
    }

    //general method to initiate unit movement
    private bool StartMovement(MovementMode movementMode)
    {
        Vector3 dest = GetDestination();

        //update movement mode to match request
        MovementMode oldMoveMode = _movementMode;
        _movementMode = movementMode;

        //trigger handling for starting movement, based on the specified movement mode
        switch (_movementMode)
        {
            case MovementMode.MODE_SPLINE:
                //if already performing spline movement, try using dynamic recalculation?
                if (_moving && oldMoveMode == MovementMode.MODE_SPLINE)
                {
                    HandleDynamicSplineChange(s);
                }
                else
                {
                    StartSplineMovement(dest);
                }
                break;
            case MovementMode.MODE_PHYSICAL:
                StartPhysicalMovement(dest);
                break;
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
        //set rigidbody to kinematic so movement can be controlled by setting the transform
        _rigidBody.isKinematic = true;

        //calculate a path in order to get control points for the spline.

        NavMeshPath splinePath = new NavMeshPath();

        //if trying to path to the destination directly fails, need to determine an alternative destination
        //will occur when the target point is not on the navmesh.
        if (!NavMesh.CalculatePath(transform.position, dest, NavMesh.AllAreas, splinePath))
        {
            //special case: if unit has navMeshObstacle attached, then try plotting a path 
            //with the first control point on the path placed in front of the unit, then manually adjusted afterward
            if (GetComponent<NavMeshObstacle>() != null)
            {
                Debug.Log("Path planning likely failing due to unit having NavMeshObstacle attached, need to try and compute alternative destination.");

                if(!FindUnobstructedPathUnitHasObstacle(dest, out splinePath))
                {
                    //fallback to default movement if can't make spline movement work...
                    Debug.LogWarning("Could not find valid alternative destination, using default movement as fallback.");
                    _movementMode = MovementMode.MODE_DEFAULT;
                    return;
                }
            }
            else
            {
                Debug.Log("Attempted to path to obstructed region, need to try and compute alternative destination");

                if (!FindUnobstructedPath(transform.position, dest, out splinePath))
                {
                    //fallback to default movement if can't make spline movement work...
                    Debug.LogWarning("Could not find valid alternative destination, using default movement as fallback.");
                    _movementMode = MovementMode.MODE_DEFAULT;
                    return;
                }
            }
            Debug.Log("Alternative path calculation successful.");
        }

        //Instantiate the spline
        if (!_curSpline.InitSpline(splinePath)){
            Debug.LogError("Spline initialization failed, cannot begin moving the unit.");

            return;
        }

        //set time parameter to 0
        s = INITIAL_SPLINE_PARAM;

        //if total path length not set, do it here
        if(_totalPathLength == 0)
        {
            _totalPathLength = _curSpline.GetFullPathLength();
        }

        //calculate rate of change (of s) based on length of spline and speed
        //length of spline taken into account so unit will progress through the spline at the expected physical speed
        sChangeRate = BASE_CHANGE_RATE * (Speed / _curSpline.GetFullPathLength());
    }

    //triggers spline-based movement
    private void StartPhysicalMovement(Vector3 dest)
    {
        _rigidBody.isKinematic = false;
    }

    //finding unobstructed path when unit has a NavMeshObstacle attached - will allow for units with NavMeshObstacle to path plan
    //this will be removed in final product assuming we don't go with hermite spline movement, because we won't need
    //to place NavMeshObstacle on heavier units to mitigate the lack of collision detection with kinematic movement
    private bool FindUnobstructedPathUnitHasObstacle(Vector3 dest, out NavMeshPath path)
    {
        NavMeshObstacle obstacle = GetComponent<NavMeshObstacle>();

        path = new NavMeshPath();

        //offset the start position to be out of range of the NavMeshObstacle
        Vector3 newStartPos = transform.position + transform.forward * (obstacle.size.z+0.5f);

        //if still fails, try adding in the usual unobstructed path handling
        if (!NavMesh.CalculatePath(newStartPos, dest, NavMesh.AllAreas, path))
        {
            Debug.Log("Attempted to path to obstructed region, need to try and compute alternative destination");

            if (!FindUnobstructedPath(newStartPos, dest, out path))
            {
                return false;
            }
        }

        //reset first control point to our position
        path.corners[0] = transform.position;

        return true;
    }

    //if the target position is the position of a NavMeshObstacle object, pathfinding will fail, so need to find a position sufficiently offseted
    //such that it is on the NavMesh.
    private bool FindUnobstructedPath(Vector3 start, Vector3 dest, out NavMeshPath path)
    {
        path = new NavMeshPath();

        //first, try finding one in direction of our unit
        Vector3 directionToUnit = Vector3.Normalize(start - dest);

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

        if(NavMesh.CalculatePath(start, dest, NavMesh.AllAreas, path))
        {
            return true;
        }

        //todo: Try alternative destination in all directions surrounding destination point (will implement if needed)
        //this is suboptimal so might be best to go a different route entirely for unit movement instead of having to deal with stuff like this...

        return false;
    }

    //cease all movement, or just unordered movement if stopOrderedMovement = false
    public void StopMovement()
    {
        //temp fix for weird physics issues when switching off kinematics: set rotation on x,z axis to 0
        //remove after spline movement is gone
        Quaternion curRotation = transform.rotation;
        curRotation.eulerAngles = new Vector3(0, transform.rotation.eulerAngles.y, 0);
        transform.rotation = curRotation;

        //disable kinematic rigidbody if it was enabled
        _rigidBody.isKinematic = false;

        //disable general stuff for movement state
        _destination = Vector3.zero;
        _dynamicDestination = null;
        _moving = false;

        //disable other movement-related stuff
        _pathCompletionOffset = 0;
        _totalPathLength = 0;
        _movementMode = MovementMode.MODE_DEFAULT;

        SetToIdle();
    }
        //set idle animation
        SetToIdle();

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
            //skip if no dynamic destination
            if(_dynamicDestination == null)
            {
                yield return null;
                continue;
            }

            //get the current destination

            //determine if the object has moved a certain distance, if so, update the destination to match the latest position
            Vector3 target = GetDestination();

            //could quickly optimize using manhattan distance if this is too inefficient
            float distToTarget = Vector3.Distance(target, _dynamicDestination.position);

            if (distToTarget > 1.0f)
            {
                //update the respective destination vector to match the dynamic destination
                _dynamicDestUpdateMade = true;

                _destination = _dynamicDestination.position;
            }
            else
            {
                _dynamicDestUpdateMade = false;
            }

            yield return null;
        }
    }

    //for animations
    public void SetToIdle()
    {
        if (_animator != null)
        {
            if (_animator.GetComponent<Animator>().GetBool("FIRE") == false || _animator.GetComponent<Animator>().GetBool("ATTACK") == false)
            {
                _animator.SetAnim("IDLE");
            }
        }
    }
}

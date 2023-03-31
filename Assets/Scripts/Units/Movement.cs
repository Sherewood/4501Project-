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
    //used for pathfinding from point A to B
    MODE_PATHFINDING,
    //used for physics-based movement (flocking, steering, arrival, etc.)
    //should be activated for units that use the flocking behaviour
    MODE_PHYSICAL,
    //fall back to if other modes don't work to prevent unit not moving/error
    //use warning log to indicate fallback to default movement if applicable...
    MODE_DEFAULT,
}

/* Unit Component */
//Purpose: Control movement (and rotation) of the unit

/* Note: Movement method will have to be updated for final deliverable */

public class Movement : MonoBehaviour
{
    //callback to AI Control
    public AIEvent AICallback;

    //callback to unit controller
    private DestinationReachedEvent _destReachedEvent;

    /* movement type */

    //the type of movement to use to update the unit's position
    private MovementMode _movementMode;

    //destination (lower priority - via the object itself)
    private Vector3 _destination;

    //flock leader - used for leader force in flocking
    private GameObject _flockLeader;
    //the flock that this unit is a member of
    private List<GameObject> _flock;
    //for wandering
    private System.Random _random;

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

    [Tooltip("When true, velocity will be forced to 0 when rotating in place. Leave disabled if you want your units to be pushed by other units.")]
    public bool FreezePositionWhenRotatingInPlace;

    // animateor

    private animation_Controller _animator;

    // navmeshagent for pathfinding mode
    private NavMeshAgent _navMeshAgent;

    //terrain used for measuring
    private Terrain _terrain;

    //everything initialized here can cause race condition crashes if initialized in Start()
    void Awake()
    {
        _destReachedEvent = new DestinationReachedEvent();

        _random = new System.Random();

        _terrain = FindObjectOfType<Terrain>();

        //moved all this due to race condition where initializing in Start will override shit
        _moving = false;
        _destination = new Vector3();
        _returnPoint = new Vector3();
        _movementMode = MovementMode.MODE_DEFAULT;
        _flockLeader = null;
        _flock = null;
        _dynamicDestination = null;

        ConfigNavMeshAgent();

        _rigidBody = GetComponent<Rigidbody>();

        _unitState = GetComponent<UnitState>();
    }

    void Start()
    {

        StabilizePosition();

        //start up coroutines
        StartCoroutine(InPlaceRotationHandler());

        StartCoroutine(UpdateDynamicDestination());

        _targetRotation = Quaternion.identity;

        //animator
        _animator = this.GetComponent<animation_Controller>();
    }

    //set up callback for destination reached handling
    public void ConfigureDestinationReachedCallback(UnityAction<GameObject> destinationReachedCallback)
    {
        _destReachedEvent.AddListener(destinationReachedCallback);
    }

    //lock the unit to the terrain
    private void StabilizePosition()
    {
        _rigidBody.position = new Vector3(_rigidBody.position.x, _terrain.SampleHeight(_rigidBody.position), _rigidBody.position.z); 
    }

    //configure NavMeshAgent using the speed specified for the unit
    private void ConfigNavMeshAgent()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        if (_navMeshAgent != null)
        {
            _navMeshAgent.speed = Speed;
            _navMeshAgent.acceleration = Speed / 2;
            _navMeshAgent.angularSpeed = TurnRate / 1.5f;
            _navMeshAgent.stoppingDistance = 0.0f;
            _navMeshAgent.enabled = false;
        }
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
                case MovementMode.MODE_PATHFINDING:
                    PathfindingMovementUpdate();
                    break;
                case MovementMode.MODE_PHYSICAL:
                    PhysicsBasedMovementUpdate();
                    break;
                case MovementMode.MODE_DEFAULT:
                    DefaultMovementUpdate();
                    break;
            }

            //temporary: include spline timing parameter for reaching destination
            if (Vector3.Distance(transform.position, target) < 0.26f + _offsetFromDestination)
            {
                Debug.Log("Destination reached..." + transform.position);
                //terminate movement if destination reached.
                StopMovement();
                //report to interested parties that destination has been reached
                AICallback.Invoke("reachedDestination");
                _destReachedEvent.Invoke(gameObject);
            }
        }
        //rotation in place for when unit is not in motion (aiming)
        else
        {
            //in some cases the unit will move while its supposed to be just rotating in place...
            //I have no idea why so I'm just going to force it to cooperate
            if(_rigidBody.velocity != Vector3.zero && FreezePositionWhenRotatingInPlace)
            {
                _rigidBody.velocity = Vector3.zero;
            }
            //Debug.Log(gameObject.name + " debug - rigidbody velocity while rotating in place: " + _rigidBody.velocity);
            if (_targetRotation != Quaternion.identity)
            {
                _rigidBody.MoveRotation(Quaternion.RotateTowards(transform.rotation, _targetRotation, TurnRate * Time.deltaTime));
            }
        }

        //keep unit attached to terrain
        StabilizePosition();
    }

    //for pathfinding
    public void PathfindingMovementUpdate()
    {
        //need to update path destination if target location changes
        if (_dynamicDestUpdateMade)
        {
            _navMeshAgent.SetDestination(GetDestination());
        }

        /*
        //trying a light separation force
        Collider[] nearbyUnits = Physics.OverlapSphere(transform.position, 2.0f);

        Vector3 separationVector = new Vector3();

        //todo: move this part back into targeting
        foreach(Collider candidate in nearbyUnits)
        {
            GameObject unit = candidate.gameObject;

            if(unit.GetComponent<UnitInfo>() == null)
            {
                continue;
            }

            float dist = Vector3.Distance(unit.transform.position, transform.position);
            Vector3 dir = unit.transform.position - transform.position;

            separationVector -= dir.normalized * (2.0f - dist);
        }

        _rigidBody.AddForce(separationVector * 0.5f * Time.deltaTime * Speed, ForceMode.VelocityChange);
        */

        if (CheckIfShouldSwitchMovementMode(true))
        {
            SwitchToDefaultMovement();
        }
    }

    public void SetFlockLeader(GameObject flockLeader)
    {
        _flockLeader = flockLeader;
    }

    public void SetFlock(List<GameObject> flockMembers)
    {
        _flock = flockMembers;
    }

    //todo: add flocking code here
    //forces added based on whether unit is leader or not.
    private void PhysicsBasedMovementUpdate()
    {
        //needs to be physics-based
        if (_rigidBody.isKinematic) _rigidBody.isKinematic = false;
        //get the target destination
        /*
        Vector3 target = GetDestination();
        if (target == Vector3.zero)
        {
            return;
        }
        */

        //if no flock leader, then stop movement immediately (assume the flock leader is no longer flock leader for whatever reason
        //todo: might instead look at some sort of waypoint system in absence of a flock leader?
        //would have to be something like - stop within certain distance of the waypoint, or within certain distance of a flock member that has stopped
        //but logic for that will be very complicated...
        if (_flockLeader == null)
        {
            StopMovement();
            return;
        }
        //follower
        //initial declaration for all the necessary forces
        Vector3 separationVector = Vector3.zero;
        Vector3 cohesionVector = Vector3.zero;
        Vector3 alignmentVector = transform.forward;
        Vector3 leaderVector = _flockLeader.transform.position - transform.position - _flockLeader.transform.forward * 0.5f;
        if(leaderVector.magnitude > Speed * 2)
        {
            leaderVector = leaderVector.normalized * Speed * 2;
        }
        //getting all the units in the flock a given unit belongs too (kinda bugged rn)
        UnitController x = GetComponent<UnitController>();
        //get flock from unit controller if wasn't manually set...
        if (_flock == null)
        {
            _flock = FindObjectOfType<UnitController>().GetFlock(_flockLeader);
        }
        //separation
        int numSeparationNeighbours = 0;
        //sum the positions of the other units in the flock
        foreach (GameObject currentUnit in _flock)
        {
            if (currentUnit == null) continue;

            float dist = Vector3.Distance(currentUnit.transform.position, transform.position);
            Vector3 dir = currentUnit.transform.position - transform.position;
            if (currentUnit != this.gameObject && dist <= 3)
            {
                numSeparationNeighbours++;
                //invert the force so units that are closer cause larger separation
                separationVector -= dir.normalized * (3-dist);
            }
        }

        //cohesion
        //basically the same as separation but in reverse with a different radius
        int numCohesionNeighbours = 0;
        foreach (GameObject currentUnit in _flock)
        {
            if (currentUnit == null) continue;

            if (currentUnit != this.gameObject && Vector3.Distance(currentUnit.transform.position, transform.position) <= 4)
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
        foreach (GameObject currentUnit in _flock)
        {
            if (currentUnit == null) continue;

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
        separationVector = separationVector * 1.5f;
        cohesionVector = cohesionVector * 0.2f;
        alignmentVector = alignmentVector * 0.8f;
        leaderVector = leaderVector * 2.0f;

        //add the forces and rotate the unit to where it's going, trying to align with the others as well
        Vector3 moveVector = separationVector + cohesionVector + alignmentVector + leaderVector;

        moveVector.y = 0;

        _rigidBody.AddForce(moveVector * Time.deltaTime * Speed, ForceMode.VelocityChange);
        
        // Create orientation from the alignment force
        Quaternion orient = new Quaternion();
        orient.SetLookRotation(alignmentVector, Vector3.up);

        // Set unit's orientation
        _targetRotation = orient;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, _targetRotation, TurnRate * Time.deltaTime);
        
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

        if (CheckIfShouldSwitchMovementMode(false))
        {
            SwitchToPathfindingMovement();
        }
    }

    //hack for switching between pathfinding and default movement when necessary (for dealing with moving towards NavMeshObstacles)
    public bool CheckIfShouldSwitchMovementMode(bool inPathfindingMode)
    {
        //if dealing with dynamic destination, use short-range RaycastAll to determine whether to switch into default mode
        //in order to limit pathfinding issues with navmeshobstacles
        //meant primarily for melee attackers, so don't use regular raycast because it might hit another melee attacker instead on the way
        if (_dynamicDestination != null && _dynamicDestination.gameObject.GetComponent<NavMeshObstacle>() != null)
        {
            Vector3 dir = Vector3.Normalize(_dynamicDestination.position - transform.position);

            //use larger range once in default movement mode to prevent rubberbanding
            float range = inPathfindingMode ? 5.0f : 5.5f;

            RaycastHit[] hitTargets = Physics.RaycastAll(transform.position, dir, range);

            //check to see if our target is close enough
            //may or may not want additional logic to determine if NavMeshObstacles are in the way
            foreach (RaycastHit hitTarget in hitTargets)
            {
                if (hitTarget.transform == _dynamicDestination)
                {
                    return inPathfindingMode;
                }
            }
        }
        return !inPathfindingMode;
    }

    //methods for switching between default and pathfinding movement

    //assumption: previous movement mode is pathfinding mode
    public void SwitchToDefaultMovement()
    {
        _navMeshAgent.enabled = false;
        _movementMode = MovementMode.MODE_DEFAULT;
    }

    //assumption: previous movement mode is default mode
    public void SwitchToPathfindingMovement()
    {
        _navMeshAgent.enabled = true;
        _navMeshAgent.SetDestination(GetDestination());
        _movementMode = MovementMode.MODE_PATHFINDING;
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

    //move to
    public bool MoveToDestination(Vector3 destination, MovementMode movementMode = MovementMode.MODE_DEFAULT, float offsetFromDestination = 0.0f)
    {
        _destination = destination;
        _offsetFromDestination = offsetFromDestination;
        _moving = true;

        return StartMovement(movementMode);
    }

    // get the unit to wander to a point within a given radius, either from their current position, or from their return point
    public bool WanderToPointWithinRadius(float radius, int wanderMinDist, int wanderMaxDist, bool useReturnPoint, int arc = 360)
    {
        int attemptLimit = 25;
        int attempts = 0;

        //use either the return point or the unit's current position as the center
        Vector3 center = useReturnPoint ? _returnPoint : transform.position;

        while (attempts < attemptLimit)
        {
            //randomly pick an angle within [-arc/2,arc/2] to represent the angle between the unit's current forward direction, and the desired direction vector
            int angle = _random.Next(arc) - arc / 2;
            Quaternion dirQuat = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 dir = dirQuat * transform.forward;

            //randomly pick a magnitude to represent the distance travelled between [wanderMin, wanderMax]
            int dist = _random.Next(wanderMinDist, wanderMaxDist);

            //calculate the destination point, and determine if it is within the given radius of the unit position or return point
            Vector3 destination = transform.position + dir * dist;

            //if it is, then move to that destination, otherwise try again, if too many attempts fail in a row drop an error
            if(Vector3.Distance(destination, center) <= radius)
            {
                //clamp to terrain first
                destination.y = _terrain.SampleHeight(destination);

                return MoveToDestination(destination, MovementMode.MODE_PATHFINDING);
            }

            //otherwise, try again
            attempts++;
        }

        Debug.LogError("Failed to find a wandering destination, radius: " + radius + " min distance: " + wanderMinDist + " max distance: " + wanderMaxDist + "arc: " + arc + ", Consider adjusting?");

        return false;
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

    public Vector3 GetReturnPoint()
    {
        return _returnPoint;
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
            case MovementMode.MODE_PATHFINDING:
                StartPathfindingMovement(dest);
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

    public void StartPathfindingMovement(Vector3 dest)
    {
        if(_navMeshAgent == null)
        {
            Debug.LogError("Error: Tried to do pathfinding on unit without NavMeshAgent");
            return;
        }

        _navMeshAgent.enabled = true;

        _navMeshAgent.SetDestination(dest);
        _navMeshAgent.stoppingDistance = _offsetFromDestination;
    }

    //triggers spline-based movement
    private void StartPhysicalMovement(Vector3 dest)
    {
        _rigidBody.isKinematic = false;
    }

    //keeping these unobstructed path methods for now in case they are needed...

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
        /*
        //temp fix for weird physics issues: set rotation on x,z axis to 0
        Quaternion curRotation = transform.rotation;
        curRotation.eulerAngles = new Vector3(0, transform.rotation.eulerAngles.y, 0);
        transform.rotation = curRotation;
        */
        //disable general stuff for movement state
        _destination = Vector3.zero;
        _dynamicDestination = null;
        _moving = false;

        //disable flocking stuff
        _flock = null;
        _flockLeader = null;

        //disable navmeshagent if enabled
        //trying to deal with weird behaviour by resetting rigidbody velocity. Might lead to other weirdness if
        //unit was being acted on by an external force.
        if(_navMeshAgent != null && _navMeshAgent.enabled)
        {
            _navMeshAgent.enabled = false;
            _rigidBody.velocity = new Vector3(0,0,0);
        }

        _movementMode = MovementMode.MODE_DEFAULT;

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

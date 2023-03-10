using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/* destination reached event definition */
[System.Serializable]
public class DestinationReachedEvent : UnityEvent { }

/* Unit Component */
//Purpose: Control movement (and rotation) of the unit

/* Note: Movement method will have to be updated for advanced prototype (And final deliverable!) */

//this whole component should be nuked from orbit tbh, what in gods name have I done, TIL having a proper state machine is important

public class Movement : MonoBehaviour
{
    /* callbacks */
    public DestinationReachedEvent DestinationReachedEvent;

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
        StartCoroutine(RotationHandler());

        StartCoroutine(UpdateDynamicDestination());

        _targetRotation = Quaternion.identity;
    
    }

    void OnDestroy()
    {
        StopCoroutine(RotationHandler());
        StopCoroutine(UpdateDynamicDestination());
    }

    //moved to fixedupdate for better physics
    void FixedUpdate()
    {
        if (_moving)
        {
            //get target
            Vector3 target = GetDestination();

            //if nothing to move towards, stop.
            if (target == Vector3.zero)
            {
                return;
            }

            //for getting direction, eliminate the y component before normalizing
            Vector3 direction = target - transform.position;

            direction.y = 0.0f;

            direction = Vector3.Normalize(direction);

            Debug.Log(direction);

            //lack of ease out leads to jittery physics from sudden stop? ease-in/out should help later....
            _rigidBody.MovePosition(transform.position += direction * Speed * Time.deltaTime);

            if (Vector3.Distance(transform.position, target) < 0.26f + _offsetFromDestination)
            {
                Debug.Log("Destination reached...");
                //terminate movement if destination reached.
                StopMovement(true);
                //report to interested parties that destination has been reached
                DestinationReachedEvent.Invoke();
            }
        }

        if (_targetRotation != Quaternion.identity)
        {
            _rigidBody.MoveRotation(Quaternion.RotateTowards(transform.rotation, _targetRotation, TurnRate * Time.deltaTime));
        }
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

    //set ordered destination - should usually be from player command
    public void SetOrderedDestination(Vector3 orderedDestination, float offsetFromDestination = 0.0f)
    {
        _orderedDestination = orderedDestination;
        _offsetFromDestination = offsetFromDestination;
        _moving = true;

        if (ShouldChangeToMoveState())
        {
            _unitState.SetState(UState.STATE_MOVING);
        }
    }

    //set destination - not specifically ordered so can be overridden by ordered destination
    public void SetDestination(Vector3 destination)
    {
        _destination = destination;
        _moving = true;

        if (ShouldChangeToMoveState())
        {
            _unitState.SetState(UState.STATE_MOVING);
        }
    }

    /* set destination methods for dynamic destinations */

    //note: if rotateOnly is true, only rotating will be done, otherwise both movement and rotation is done
    public void SetDynamicOrderedDestination(Transform dynamicDestination, bool rotateOnly)
    {
        _dynamicDestination = dynamicDestination;

        _isDynamicDestOrdered = true;

        //only enter movement mode if rotateOnly is false.
        if (!rotateOnly)
        {
            _moving = true;

            if (ShouldChangeToMoveState())
            {
                _unitState.SetState(UState.STATE_MOVING);
            }
        }
    }

    public void SetDynamicDestination(Transform dynamicDestination, bool rotateOnly)
    {
        _dynamicDestination = dynamicDestination;

        _isDynamicDestOrdered = false;

        if (!rotateOnly)
        {
            _moving = true;

            if (ShouldChangeToMoveState())
            {
                _unitState.SetState(UState.STATE_MOVING);
            }
        }
    }

    //set moving to harvest
    public void SetOrderedHarvestDestination(Vector3 orderedDestination)
    {
        SetOrderedDestination(orderedDestination);
        
        //just straight up forcing the state to 'moving to harvest' could be problematic
        //but only workers will support this component so it won't interfere with any attacking states.
        _unitState.SetState(UState.STATE_MOVING_TO_HARVEST);
    }

    //set moving to construct
    public void SetOrderedConstructionDestination(Vector3 orderedDestination)
    {
        //get the forward offset from the construction component
        Construction constructComp = GetComponent<Construction>();

        //move to the construction site, but stop short according to the offset
        SetOrderedDestination(orderedDestination, constructComp.GetConstructionSiteOffset());

        //just straight up forcing the state to 'moving to construct' could be problematic
        //but only workers will support this component so it won't interfere with any attacking states.
        _unitState.SetState(UState.STATE_MOVING_TO_CONSTRUCT);
    }

    

    //set point to return to
    public void SetReturnPoint(Vector3 returnPoint)
    {
        _returnPoint = returnPoint;
    }

    //order return to return point
    public void OrderReturn(float offsetFromDestination = 0.0f)
    {
        _orderedDestination = _returnPoint;
        _offsetFromDestination = offsetFromDestination;
        _moving = true;

        if (ShouldChangeToMoveState())
        {
            _unitState.SetState(UState.STATE_MOVING);
        }
    }

    //cease all movement, or just unordered movement if stopOrderedMovement = false
    public void StopMovement(bool stopOrderedMovement)
    {
        Debug.Log("StopOrderedMovement: " + stopOrderedMovement);
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

    /* Coroutines */

    //handle rotation separately from movement, so it can be done without being in a moving state if needed
    //to likely be replaced long term
    IEnumerator RotationHandler()
    {
        while (true)
        {
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

            //rotate the rigidbody
            //disabled because of bugginess, doing it in FixedUpdate instead aswell
            //_rigidBody.MoveRotation(Quaternion.RotateTowards(transform.rotation, rotation, TurnRate * Time.deltaTime));

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

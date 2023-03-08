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

    //destination (lower priority)
    private Vector3 _destination;
    //destination (high priority)
    private Vector3 _orderedDestination;

    //destination which changes over time (due to the unit associated with the Transform object moving)
    private Transform _dynamicDestination;

    //true if dynamic destination is ordered
    private bool _isDynamicDestOrdered;

    //true when unit is moving towards destination
    private bool _moving;

    private Quaternion _targetRotation;

    
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

            Vector3 direction = Vector3.Normalize(target - transform.position);

            //determine the target rotation
            Quaternion rotation = new Quaternion();
            rotation.SetFromToRotation(new Vector3(0,0,1),direction);

            //lack of ease out leads to jittery physics from sudden stop? ease-in/out should help later....
            _rigidBody.MovePosition(transform.position += direction * Speed * Time.deltaTime);

            //check if at destination
            if (Vector3.Distance(transform.position, target) < 0.1f)
            {
                //terminate movement if destination reached.
                StopMovement();
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
    public void SetOrderedDestination(Vector3 orderedDestination)
    {
        _orderedDestination = orderedDestination;
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
        SetOrderedDestination(orderedDestination);

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
    public void OrderReturn()
    {
        _orderedDestination = _returnPoint;
        _moving = true;

        if (ShouldChangeToMoveState())
        {
            _unitState.SetState(UState.STATE_MOVING);
        }
    }

    //cease all movement
    public void StopMovement()
    {
        //todo: more handling needed for reaching destination
        _orderedDestination = new Vector3();
        _destination = new Vector3();
        _dynamicDestination = null;
        _moving = false;
        _isDynamicDestOrdered = false;

        if (_unitState.GetState() == UState.STATE_MOVING)
        {
            _unitState.SetState(UState.STATE_IDLE);
        }
    }

    //return true if based on the current state, the state should transition to "MOVING"
    private bool ShouldChangeToMoveState()
    {
        UState curState = _unitState.GetState();

        return (curState == UState.STATE_IDLE || curState == UState.STATE_HARVESTING);
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

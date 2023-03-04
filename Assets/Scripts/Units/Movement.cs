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



public class Movement : MonoBehaviour
{
    /* callbacks */
    public DestinationReachedEvent DestinationReachedEvent;

    private Vector3 _destination;
    private Vector3 _orderedDestination;
    private bool _moving;
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

        _rigidBody = GetComponent<Rigidbody>();

        _unitState = GetComponent<UnitState>();
    }

    void Update()
    {
        if (_moving)
        {
            //get direction and target position, and move towards the position
            Vector3 direction = new Vector3();
            Vector3 target = new Vector3();
            //ordered destination overrides automatically determined destination
            if (_orderedDestination != null)
            {
                direction = Vector3.Normalize(_orderedDestination - transform.position);
                target = _orderedDestination;
            }
            else
            {
                direction = Vector3.Normalize(_destination - transform.position);
                target = _destination;
            }

            Quaternion rotation = new Quaternion();
            rotation.SetFromToRotation(new Vector3(0,0,1),direction);

            _rigidBody.MovePosition(transform.position += direction * Speed * Time.deltaTime);
            _rigidBody.MoveRotation(Quaternion.RotateTowards(transform.rotation,rotation,TurnRate*Time.deltaTime));

            //check if at destination
            if (Vector3.Distance(transform.position, target) < 0.01f)
            {
                //terminate movement if destination reached.
                StopMovement();
                //report to interested parties that destination has been reached
                DestinationReachedEvent.Invoke();
            }
        } 
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

    //set moving to harvest
    public void SetOrderedHarvestDestination(Vector3 orderedDestination)
    {
        SetOrderedDestination(orderedDestination);
        
        //just straight up forcing the state to 'moving to harvest' could be problematic
        //but only workers will support this component so it won't interfere with any attacking states.
        _unitState.SetState(UState.STATE_MOVING_TO_HARVEST);
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
        _moving = false;

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




}

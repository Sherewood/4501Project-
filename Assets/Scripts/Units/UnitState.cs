using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/* Data type */
//Purpose: Represents the state of the unit

public enum UState
{
    STATE_IDLE,
    STATE_MOVING,
    STATE_MOVING_TO_CONSTRUCT,
    STATE_ATTACKING,
    STATE_GUARDING,
    STATE_FORTIFIED,
    STATE_HARVESTING,
}

/* Unit class */
//Purpose: Holds the state of the unit

public class UnitState : MonoBehaviour
{

    private UState _state;

    //storing some components so they can be disabled upon request
    //todo: refactor this
    private Movement _movement;
    private Attack _attack;
    private Targeting _targeting;
    private UnitBuilderComponent _unitBuilder;

    // Start is called before the first frame update
    void Awake()
    {
        _state = UState.STATE_IDLE;

        _movement = GetComponent<Movement>();
        _attack = GetComponent<Attack>();
        _targeting = GetComponent<Targeting>();
        _unitBuilder = GetComponent<UnitBuilderComponent>();
    }

    public void SetState(UState state)
    {
        _state = state;
    }

    public UState GetState()
    {
        return _state;
    }

    //disable relevant active components of unit
    public void DisableUnit()
    {
        if(_movement != null)
        {
            _movement.enabled = false;
        }
        if(_attack != null)
        {
            _attack.enabled = false;
        }
        if(_targeting != null)
        {
            _targeting.enabled = false;
        }
        if(_unitBuilder != null)
        {
            _unitBuilder.enabled = false;
        }
    }

    public bool IsState(UState state)
    {
        return _state == state;
    }

    public UState StringToUState(string stateString)
    {
        switch (stateString)
        {
            case "idle":
                return UState.STATE_IDLE;
            case "moving":
                return UState.STATE_MOVING;
            case "movingToConstruct":
                return UState.STATE_MOVING_TO_CONSTRUCT;
            case "attack":
                return UState.STATE_ATTACKING;
            case "guard":
                return UState.STATE_GUARDING;
            case "fortify":
                return UState.STATE_FORTIFIED;
            default:
                Debug.LogError("Invalid state string: " + stateString);
                return UState.STATE_IDLE;
        }
    }
}

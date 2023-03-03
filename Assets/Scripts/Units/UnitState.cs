using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/* Data type */
//Purpose: Represents the state of the unit

public enum UState
{
    STATE_IDLE,
    STATE_MOVING,
    STATE_MOVING_TO_HARVEST,
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

    // Start is called before the first frame update
    void Awake()
    {
        _state = UState.STATE_IDLE;
    }

    public void SetState(UState state)
    {
        _state = state;
    }

    public UState GetState()
    {
        return _state;
    }
}

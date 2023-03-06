using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/* Unit Component */
//Purpose: Handles unit attacking behaviour

public class Attack : MonoBehaviour
{
    private Targeting _targeting;
    private UnitState _unitState;
    private Movement _movement;
    private Weapon _weapon;

    private GameObject _currentTarget;


    // Start is called before the first frame update
    void Start()
    {
        _targeting = GetComponent<Targeting>();
        _unitState = GetComponent<UnitState>();
        _movement = GetComponent<Movement>();
        _weapon = GetComponent<Weapon>();

        _currentTarget = null;
    }

    // Update is called once per frame
    void Update()
    {
        GameObject latestTarget = UpdateTarget();
        //target changed
        if(_currentTarget != latestTarget)
        {
            //loss of target handling
            if(latestTarget == null)
            {
                HandleTargetLoss();
                return;
            }

            //target change handling
            HandleTargetChange(latestTarget);
        }

        //if no target at this point, finish up as there is nothing to be done.
        if (_currentTarget == null)
        {
            return;
        }
        //weapon handling

        //check if target is in range

        //if in range, perform 'got in range' handling if not in range
        //if not in range, perform 'approach' handling if in range

        //if weapon in range, check if weapon can be fired
        //if weapon can be fired, fire it by creating a projectile object
        float distanceToTarget = Vector3.Distance(transform.position, _currentTarget.transform.position);
        if (_weapon.IsWeaponInRange(distanceToTarget))
        {
            if (_weapon.IsWeaponReadyToFire(distanceToTarget))
            {
                _weapon.FireWeapon(_currentTarget);
            }
        }
    }

    //get the latest target from the targeting component
    private GameObject UpdateTarget()
    {
        GameObject latestTarget = null;

        //if no target, use full range detection, else just use the auto detection
        if (_currentTarget == null)
        {
            latestTarget = _targeting.GetTargetAfterFullDetection();
        }
        else
        {
            latestTarget = _targeting.GetTarget();
        }
        return latestTarget;
    }

    //handle loss of target
    private void HandleTargetLoss()
    {
        UState curState = _unitState.GetState();

        switch (curState)
        {
            case UState.STATE_ATTACKING:
                //if attacking - stop moving and return to idle state
                _unitState.SetState(UState.STATE_IDLE);
                _movement.StopMovement();
                break;
            case UState.STATE_GUARDING:
                //if guarding - return to guard position
                _movement.OrderReturn();
                break;
            case UState.STATE_FORTIFIED:
                //if fortifying - do nothing :)
                break;
            default:
                //other states - do nothing :) (don't want the attacking component randomly interrupting movement state, for example)
                break;
        }
        _currentTarget = null;
    }

    //handle change of target (or finding new target)
    private void HandleTargetChange(GameObject newTarget)
    {
        UState curState = _unitState.GetState();

        switch (curState)
        {
            case UState.STATE_ATTACKING:
            case UState.STATE_GUARDING:
            case UState.STATE_IDLE:
                //if attacking, guarding, or in idle state, should request movement component to move towards target
                break;
            default:
                //else, no action needed here
                break;
        }

        //might want to move this into switch statement (ignore new target if not in relevant state)
        _currentTarget = newTarget;
    }

    private bool CheckIfInRange()
    {
        //use weapon component
        //only check max range here (can check min range in ready to fire handling)
        return false;
    }

    private bool CheckIfReadyToFire()
    {
        //use weapon component
        return false;
    }

    private void HandleEnteredAttackRange()
    {
        _movement.StopMovement();
    }

    private void HandleLeftAttackRange()
    {
        //could refactor this part into another helper to minimize code re-use, for now leave as is in case there are differences in handling
        UState curState = _unitState.GetState();

        switch (curState)
        {
            case UState.STATE_ATTACKING:
            case UState.STATE_GUARDING:
            case UState.STATE_IDLE:
                //if attacking, guarding, or in idle state, should request movement component to move towards target
                break;
            default:
                //else, no action needed here
                break;
        }
    }

    private void HandleFiring()
    {
        //use weapon component to get projectile spawned and fired towards target
    }


}

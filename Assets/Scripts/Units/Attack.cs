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

    private bool _targetInRange;


    // Start is called before the first frame update
    void Start()
    {
        _targeting = GetComponent<Targeting>();
        _unitState = GetComponent<UnitState>();
        _movement = GetComponent<Movement>();
        _weapon = GetComponent<Weapon>();

        _currentTarget = null;

        _targetInRange = false;
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

        //determine if enemy is close enough to justify distance calculation
        if (CheckIfEnemyFarOutOfRange())
        {
            if (_targetInRange)
            {
                HandleLeftAttackRange();
            }
            return;
        }

        //get distance between target and player
        float distanceToTarget = Vector3.Distance(transform.position , _currentTarget.transform.position);

        //weapon handling

        //check if target is in range

        //if in range, perform 'got in range' handling if not in range
        //if not in range, perform 'approach' handling if in range

        if (!CheckIfInRange(distanceToTarget))
        {
            if (_targetInRange)
            {
                HandleLeftAttackRange();
            }
            return;
        }

        if (!_targetInRange)
        {
            HandleEnteredAttackRange();
        }

        //if weapon in range, check if weapon can be fired
        //if weapon can be fired, fire it by creating a projectile object
        if (_weapon.IsWeaponReadyToFire(distanceToTarget))
        {
            _weapon.FireWeapon(_currentTarget);
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
        _targetInRange = false;
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
        _targetInRange = false;
        UState curState = _unitState.GetState();

        switch (curState)
        {
            case UState.STATE_ATTACKING:
            case UState.STATE_GUARDING:
            case UState.STATE_IDLE:
                //if attacking, guarding, or in idle state, should request movement component to move towards target
                _movement.SetDynamicDestination(newTarget.transform, false);
                break;
            case UState.STATE_FORTIFIED:
                //fortify state: turn towards unit, but do not move towards it
                _movement.SetDynamicDestination(newTarget.transform, true);
                break;
            default:
                //else, no action needed here
                break;
        }

        //might want to move this into switch statement (ignore new target if not in relevant state)
        _currentTarget = newTarget;
    }

    //return true if enemy is clearly out of range, and distance calculation is not needed
    private bool CheckIfEnemyFarOutOfRange()
    {
        /*
        Calculate manhattan x and z distances, if either of these is larger than the radius of
        the weapon, then there is zero chance the enemy is within range of the weapon, and we do not
        need to calculate euclidean distance
        */
        float distX = Mathf.Abs(transform.position.x - _currentTarget.transform.position.x);
        float distZ = Mathf.Abs(transform.position.x - _currentTarget.transform.position.z);

        return (distX >= _weapon.MaxRange + 0.01) || (distZ >= _weapon.MaxRange + 0.01);
    }

    //check if weapon is in range of enemy
    private bool CheckIfInRange(float distance)
    {
        //use weapon component
        return _weapon.IsWeaponInRange(distance);
    }

    //check if weapon is ready to fire
    private bool CheckIfReadyToFire(float distance)
    {
        //use weapon component
        return _weapon.IsWeaponReadyToFire(distance);
    }

    private void HandleEnteredAttackRange()
    {
        //stop prior movement, resume targetting, but only rotate towards it now.
        _movement.StopMovement();
        _movement.SetDynamicDestination(_currentTarget.transform, true);
    }

    private void HandleLeftAttackRange()
    {
        _targetInRange = false;
        //could refactor this part into another helper to minimize code re-use, for now leave as is in case there are differences in handling
        UState curState = _unitState.GetState();

        switch (curState)
        {
            case UState.STATE_ATTACKING:
            case UState.STATE_GUARDING:
            case UState.STATE_IDLE:
                //if attacking, guarding, or in idle state, should request movement component to move towards target
                _movement.SetDynamicDestination(_currentTarget.transform, false);
                break;
            default:
                //else, no action needed here
                break;
        }
    }

    //unused method...
    private void HandleFiring()
    {
        //use weapon component to get projectile spawned and fired towards target
    }


}

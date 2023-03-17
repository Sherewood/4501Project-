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

    //the target, as determined by the targeting component
    private GameObject _currentTarget;

    //true if target was in range as of the last frame
    private bool _targetInRange;

    //true if ordered movement is overriding the commands of the attack component
    private bool _orderedMovementOngoing;

    //true if unit is currently colliding with its target
    //note: if unit is colliding with more than 1 possible target and it switches target, this boolean will not be adjusted properly.
    private bool _collidedWithTarget;

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

        //check if ordered movement that overrode command has ceased.
        //if so, reset locally stored information to re-trigger any relevant handling
        if (_orderedMovementOngoing && !_movement.IsOrderedMovementInProgress())
        {
            _orderedMovementOngoing = false;
            _currentTarget = null;
            _targetInRange = false;
        }

        //target changed
        //either the current target doesn't match the latest target, or the latest target is null when a target was in range on the last frame
        //probably could use refactor here
        if(_currentTarget != latestTarget || (latestTarget == null && _targetInRange))
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

        //get distance between target and player, and direction from player to target
        Vector3 playerToTarget = _currentTarget.transform.position - transform.position;
        float distanceToTarget = playerToTarget.magnitude;
        Vector3 targetDirection = playerToTarget / distanceToTarget;

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
        if (CheckIfReadyToFire(distanceToTarget, targetDirection))
        {
            _weapon.FireWeapon(_currentTarget);
        }
        //might be case where unit is turned away from enemy due to ordered movement
        //but is still in attack range and has not picked up a new target, so it never
        //starts checking if ordered movement has ceased and thus does not resume action.
        //Therefore, need to check if ordered movement is preventing the unit from firing aswell.
        else
        {
            CheckIfOrderedMovementOverridesCommand();
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

    /* State transitions/behaviour triggered by component */

    //handle loss of target
    private void HandleTargetLoss()
    {
        _targetInRange = false;
        _collidedWithTarget = false;
        UState curState = _unitState.GetState();

        switch (curState)
        {
            case UState.STATE_ATTACKING:
                //if attacking - stop moving and return to idle state
                _unitState.SetState(UState.STATE_IDLE);
                _movement.StopMovement(false);
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

        CheckIfOrderedMovementOverridesCommand();

        _currentTarget = null;
    }

    //handle change of target (or finding new target)
    private void HandleTargetChange(GameObject newTarget)
    {
        _targetInRange = false;
        _collidedWithTarget = false;
        UState curState = _unitState.GetState();

        switch (curState)
        {
            case UState.STATE_ATTACKING:
                goto case UState.STATE_IDLE;
            case UState.STATE_GUARDING:
                goto case UState.STATE_IDLE;
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

        CheckIfOrderedMovementOverridesCommand();

        //might want to move this into switch statement (ignore new target if not in relevant state)
        _currentTarget = newTarget;
    }

    private void HandleEnteredAttackRange()
    {
        UState curState = _unitState.GetState();
        _targetInRange = true;

        //stop prior movement, resume targetting, but only rotate towards it now.
        _movement.StopMovement(false);
        _movement.SetDynamicDestination(_currentTarget.transform, true);

        CheckIfOrderedMovementOverridesCommand();
    }

    private void HandleLeftAttackRange()
    {
        _targetInRange = false;
        //could refactor this part into another helper to minimize code re-use, for now leave as is in case there are differences in handling
        UState curState = _unitState.GetState();

        switch (curState)
        {
            case UState.STATE_ATTACKING:
                goto case UState.STATE_IDLE;
            case UState.STATE_GUARDING:
                goto case UState.STATE_IDLE;
            case UState.STATE_IDLE:
                //if attacking, guarding, or in idle state, should request movement component to move towards target
                _movement.SetDynamicDestination(_currentTarget.transform, false);
                break;
            default:
                //else, no action needed here
                break;
        }

        CheckIfOrderedMovementOverridesCommand();
    }

    /* condition checking helpers */

    //return true if enemy is clearly out of range, and distance calculation is not needed
    private bool CheckIfEnemyFarOutOfRange()
    {
        /*
        Calculate manhattan x and z distances, if either of these is larger than the radius of
        the weapon, then there is zero chance the enemy is within range of the weapon, and we do not
        need to calculate euclidean distance
        */
        float distX = Mathf.Abs(transform.position.x - _currentTarget.transform.position.x);
        float distZ = Mathf.Abs(transform.position.z - _currentTarget.transform.position.z);

        return (distX >= _weapon.MaxRange + 0.01) || (distZ >= _weapon.MaxRange + 0.01);
    }

    //to be called after attack component issues a command
    private void CheckIfOrderedMovementOverridesCommand()
    {
        if (_movement.IsOrderedMovementInProgress())
        {
            _orderedMovementOngoing = true;
        }
    }

    //check if weapon is in range of enemy
    private bool CheckIfInRange(float distance)
    {
        //use weapon component
        //alternatively, if collided with the target, then unit is clearly in range...
        return _weapon.IsWeaponInRange(distance) || _collidedWithTarget;
    }

    //check if weapon is ready to fire
    private bool CheckIfReadyToFire(float distance, Vector3 direction)
    {
        //use weapon component
        return _weapon.IsWeaponReadyToFire(distance, direction);
    }

    /* collision based code for helping melee attackers tell if they're in range */
    //using overlapsphere instead is probably better, but this works for now
    void OnCollisionEnter(Collision collision)
    {
        GameObject possibleTarget = collision.gameObject;
        if (possibleTarget == _currentTarget)
        {
            _collidedWithTarget = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        GameObject possibleTarget = collision.gameObject;
        if (possibleTarget == _currentTarget)
        {
            _collidedWithTarget = false;
        }
    }


}

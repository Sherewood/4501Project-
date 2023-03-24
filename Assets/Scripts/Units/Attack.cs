using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
/* Unit Component */
//Purpose: Handles unit attacking behaviour

//enum to track the status of the target
public enum TargetStatus
{
    IN_RANGE,
    OUT_RANGE,
    NEW_TARGET,
    INVALID,
}

public class Attack : MonoBehaviour
{
    public AIEvent AICallback;

    private Weapon _weapon;

    //the target, as determined by the unit's AI
    private GameObject _currentTarget;

    //the last reported status of the target
    private TargetStatus _lastTargetStatus;

    //true if unit is currently colliding with its target
    //note: if unit is colliding with more than 1 possible target and it switches target, this boolean will not be adjusted properly.
    private bool _collidedWithTarget;

    //animator
    private animation_Controller _animator;
    // Start is called before the first frame update
    void Start()
    {
        _weapon = GetComponent<Weapon>();

        _currentTarget = null;

        _lastTargetStatus = TargetStatus.INVALID;

        _animator = this.GetComponent<animation_Controller>();
        _animator.SetAnim("IDlE");
    }

    // Update is called once per frame
    void Update()
    {
        if (_animator.IsIdle())
        {
            _animator.SetAnim("IDLE");
        }

        //if no target, then do nothing and disable attacking animations
        if (_currentTarget == null)
        {
            _currentTarget = null; //for the case where the UnitInfo component was null
            if (_weapon.WeaponType.Equals("melee"))
            {
                _animator.UnSetAnim("ATTACK");
            }
            else
            {
                _animator.UnSetAnim("FIRE");
            }
            return;
        }

        //weapon handling

        //handle checking if target is in range
        if (!TargetRangeChecking())
        {
            return;
        }

        //get distance between target and player, and direction from player to target
        Vector3 playerToTarget = _currentTarget.transform.position - transform.position;
        float distanceToTarget = playerToTarget.magnitude;
        Vector3 targetDirection = playerToTarget / distanceToTarget;

        //if weapon in range, check if weapon can be fired
        //if weapon can be fired, fire it by creating a projectile object
        if (CheckIfReadyToFire(distanceToTarget, targetDirection))
        {
            if (_animator != null)
            {
                if (_weapon.WeaponType.Equals("melee"))
                {
                    _animator.SetAnim("ATTACK");
                }
                else
                {
                    _animator.SetAnim("FIRE");
                }
            }
            
            _weapon.FireWeapon(_currentTarget);
        }

    }

    //handles all range checking, and corresponding target status update
    //returns true if target is in range, false otherwise
    private bool TargetRangeChecking()
    {
        TargetStatus latestTargetStatus = TargetStatus.INVALID;

        //determine if enemy is close enough to justify distance calculation
        if (CheckIfEnemyFarOutOfRange())
        {
            latestTargetStatus = TargetStatus.OUT_RANGE;
        }

        if(latestTargetStatus != TargetStatus.OUT_RANGE)
        {
            //get distance between target and player, and direction from player to target
            float distanceToTarget = Vector3.Distance(_currentTarget.transform.position, transform.position);

            //if in range, perform 'got in range' handling if previously not in range
            //if not in range, perform 'approach' handling if previously in range

            latestTargetStatus = CheckIfInRange(distanceToTarget) ? TargetStatus.IN_RANGE : TargetStatus.OUT_RANGE;

        }

        //enter/left range notification if status changed since last frame
        if (latestTargetStatus == TargetStatus.IN_RANGE && (_lastTargetStatus == TargetStatus.OUT_RANGE || _lastTargetStatus == TargetStatus.NEW_TARGET))
        {
            HandleEnteredAttackRange();
        }
        else if (latestTargetStatus == TargetStatus.OUT_RANGE && (_lastTargetStatus == TargetStatus.IN_RANGE || _lastTargetStatus == TargetStatus.NEW_TARGET))
        {
            HandleLeftAttackRange();
        }

        //store this value for the next frame
        _lastTargetStatus = latestTargetStatus;

        if(latestTargetStatus == TargetStatus.INVALID)
        {
            Debug.LogError("Error: Undefined behaviour when determining if target is in range");
        }

        return latestTargetStatus == TargetStatus.IN_RANGE;
    }

    //assign a new target for the attacking component
    public void SetTarget(GameObject newTarget)
    {
        _lastTargetStatus = TargetStatus.NEW_TARGET;
        _currentTarget = newTarget;
    }

    public void ClearTarget()
    {
        _currentTarget = null;
    }

    /* State transitions/behaviour triggered by component */

    private void HandleEnteredAttackRange()
    {
        AICallback.Invoke("targetInRange");
    }

    private void HandleLeftAttackRange()
    {
        AICallback.Invoke("targetNotInRange");
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

    //check if weapon is in range of enemy
    private bool CheckIfInRange(float distance = -1.0f)
    {
        //if distance not specified, then compute it
        if (distance == -1.0f)
        {
            distance = Vector3.Distance(_currentTarget.transform.position, transform.position);
        }
        //use weapon component
        //alternatively, if collided with the target, then unit is clearly in range...
        return _weapon.IsWeaponInRange(distance) || _collidedWithTarget;
    }

    //general method for determining if enemy that might not be the target is in range
    //need this because the correct target will not be assigned to the attack component when target change happens
    //^ that above behaviour could be a source of problems
    //AI checks this method
    public bool CheckIfEnemyInRange(GameObject enemy)
    {
        if (enemy == null)
        {
            return false;
        }
        float distance = Vector3.Distance(enemy.transform.position, transform.position);

        //todo: add melee-based check aswell (switch to overlapsphere?)
        return _weapon.IsWeaponInRange(distance);
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

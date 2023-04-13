using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/* Unit Component */
//Purpose: Handle general combat related prereqs and actions
public class CombatAIControl : AIControl
{

    protected Attack _attack;
    protected TimedSpecialAbility _specialAbility;

    protected override void GetComponents()
    {
        _attack = GetComponent<Attack>();
        _specialAbility = GetComponent<TimedSpecialAbility>();

        if (_attack == null)
        {
            Debug.LogError("Combat AI Control cannot find Attack component");
        }

        base.GetComponents();
    }

    //needed to get the callback to display properly in the inspector....
    public override void HandleAIEvent(string aiEvent)
    {
        base.HandleAIEvent(aiEvent);
    }

    //prereqs for target status
    protected override bool IsSingleWordPrereqSatisfied(string prereq, string aiEvent)
    {
        if (DebugMode)
        {
            Debug.Log("CombatAIControl - Checking if prereq: " + prereq + " is satisfied for aiEvent: " + aiEvent);
        }

        switch (prereq)
        {
            case "targetChanged":
                return (prereq.Equals(aiEvent));
            case "targetLost":
                return (prereq.Equals(aiEvent));
            case "targetNotInRange":
                if (prereq.Equals(aiEvent))
                {
                    return true;
                }

                return !_attack.CheckIfEnemyInRange(DetermineTarget());
            case "targetInRange":
                if (prereq.Equals(aiEvent))
                {
                    return true;
                }

                return _attack.CheckIfEnemyInRange(DetermineTarget());
            case "specialAbilityReady":
                return _specialAbility.CanActivate();
            case "specialAbilityNotReady":
                return !_specialAbility.CanActivate();
            default:
                return base.IsSingleWordPrereqSatisfied(prereq, aiEvent);
        }
    }

    protected override void PerformStandardAction(string action)
    {
        if (DebugMode)
        {
            Debug.Log("CombatAIControl - Performing action: " + action);
        }

        GameObject target = DetermineTarget();

        switch (action)
        {
            case "moveTarget":
                //move towards the target
                //_movement.StopMovement();
                if (target == null)
                {
                    break;
                }
                _movement.MoveToDynamicDestination(target.transform, false, MovementMode.MODE_PATHFINDING);
                break;
            case "attackTarget":
                //rotate towards the target while firing at it
                _movement.StopMovement();
                if (target == null)
                {
                    break;
                }
                _movement.MoveToDynamicDestination(target.transform, true);
                break;
            case "setFocusTarget":
                _targeting.SetTargetFocus(target);
                _attack.SetTarget(target);
                break;
            case "setTarget":
                _attack.SetTarget(target);
                break;
            case "clearTarget":
                _attack.ClearTarget();
                break;
            case "refreshTarget":
                //refresh targeting to avoid bad state
                _targeting.Refresh();
                break;
            case "activateSpecialAbility":
                _specialAbility.Activate();
                break;
            default:
                base.PerformStandardAction(action);
                return;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/* Unit Component */
//Purpose: Handle general combat related prereqs and actions
public class CombatAIControl : AIControl
{

    private Attack _attack;

    protected override void GetComponents()
    {
        _attack = GetComponent<Attack>();

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
    protected override bool IsPrereqSatisfied(string prereq, string aiEvent)
    {
        if(prereq.Contains("==") || prereq.Contains("!="))
        {
            return IsEqualityPrereqSatisfied(prereq);
        }

        if (DebugMode)
        {
            Debug.Log("Checking if prereq: " + prereq + " is satisfied for aiEvent: " + aiEvent);
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
            default:
                return base.IsPrereqSatisfied(prereq, aiEvent);
        }
    }

    protected override void PerformAction(string action)
    {
        //handle equality actions separately
        if (action.Contains("="))
        {
            PerformSetAction(action);
            return;
        }

        if (DebugMode)
        {
            Debug.Log("Performing action: " + action);
        }

        GameObject target = DetermineTarget();

        //todo: store directly when refactored into hierarchy of AI control classes (use in worker class)
        Construction constructComp = GetComponent<Construction>();

        //pretty much all actions are todo...
        switch (action)
        {
            case "moveTarget":
                //move towards the target
                _movement.StopMovement();
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
            default:
                base.PerformAction(action);
                return;
        }
    }
}

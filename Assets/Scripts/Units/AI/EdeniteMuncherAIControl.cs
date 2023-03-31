using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/* Unit Component */
//Purpose: Handle general combat related prereqs and actions
public class EdeniteMuncherAIControl : CombatAIControl
{

    private CommandableUnit _commandableUnit;

    protected override void GetComponents()
    {
        _commandableUnit = GetComponent<CommandableUnit>();

        if (_commandableUnit == null)
        {
            Debug.LogError("Edenite Muncher AI Control cannot find CommandableUnit component");
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
            Debug.Log("EdeniteMuncherAIControl - Checking if prereq: " + prereq + " is satisfied for aiEvent: " + aiEvent);
        }

        switch (prereq)
        {
            case "unitTookControl":
                return prereq.Equals(aiEvent);
            case "unitLostControl":
                return prereq.Equals(aiEvent);
            case "isUnitUnderCommand":
                return _commandableUnit.IsUnderCommand();
            case "isUnitNotUnderCommand":
                return !_commandableUnit.IsUnderCommand();
            default:
                return base.IsSingleWordPrereqSatisfied(prereq, aiEvent);
        }
    }

    protected override void PerformStandardAction(string action)
    {
        if (DebugMode)
        {
            Debug.Log("EdeniteMuncherAIControl - Performing action: " + action);
        }

        GameObject target = DetermineTarget();

        switch (action)
        {
            //should really just move this to base class but w/e
            case "refreshTargeting":
                _targeting.Refresh();
                break;
            default:
                base.PerformStandardAction(action);
                return;
        }
    }
}

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
            case "isUnitUnderCommand":
                return _commandableUnit.IsUnderCommand();
            case "isUnitNotUnderCommand":
                return !_commandableUnit.IsUnderCommand();
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

        switch (action)
        {
            default:
                base.PerformAction(action);
                return;
        }
    }
}

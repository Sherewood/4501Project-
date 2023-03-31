using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/* Unit Component */
//Purpose: Handle edenite-devil specific prereqs and actions
public class EdeniteDevilAIControl : CombatAIControl
{
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
            Debug.Log("EdeniteDevilAIControl - Checking if prereq: " + prereq + " is satisfied for aiEvent: " + aiEvent);
        }

        switch (prereq)
        {
            default:
                return base.IsSingleWordPrereqSatisfied(prereq, aiEvent);
        }
    }

    protected override void PerformStandardAction(string action)
    {
        if (DebugMode)
        {
            Debug.Log("EdeniteDevilAIControl - Performing action: " + action);
        }

        GameObject target = DetermineTarget();

        switch (action)
        {
            default:
                base.PerformStandardAction(action);
                return;
        }
    }
}

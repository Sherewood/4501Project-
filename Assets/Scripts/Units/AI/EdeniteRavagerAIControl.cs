using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/* Unit Component */
//Purpose: Handle general combat related prereqs and actions
public class EdeniteRavagerAIControl : CombatAIControl
{

    private Commander _commander;

    //boolean to track whether the unit should be gathering units or not
    private bool _isGatheringUnits = true;

    protected override void GetComponents()
    {
        _commander = GetComponent<Commander>();

        if (_commander == null)
        {
            Debug.LogError("Edenite Ravager AI Control cannot find Commander component");
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

        //todo: add rest of prereqs
        switch (prereq)
        {
            case "shouldGatherUnits":
                return _isGatheringUnits;
            case "shouldFight":
                return !_isGatheringUnits;
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

        //todo: add rest of actions
        switch (action)
        {
            case "moveToDestination": //overriden from base class
                //self explanatory
                _movement.MoveToDestination(_commandTargetPosition, MovementMode.MODE_PATHFINDING);
                //todo: refactor into separate method for setting moving state
                if (_unitState.GetState() != UState.STATE_ATTACKING && _unitState.GetState() != UState.STATE_GUARDING)
                {
                    _unitState.SetState(UState.STATE_MOVING);
                }

                //now, have commanded units follow aswell
                _commander.OrderFollowCommander();
                break;
            case "stopMovement": //overriden from base class
                //stop both this unit's, and its commanded units movement...
                //if commanded units were not previously following the commander, then the behaviour will be unexpected.
                _movement.StopMovement();

                _commander.OrderHalt();
                break;
            case "moveTarget":
                //move towards the target
                _movement.StopMovement();
                if (target == null)
                {
                    break;
                }
                _movement.MoveToDynamicDestination(target.transform, false, MovementMode.MODE_PATHFINDING);

                //order units under command to attack
                _commander.OrderAttack(target);
                break;
            case "attackTarget":
                //rotate towards the target while firing at it
                _movement.StopMovement();
                if (target == null)
                {
                    break;
                }
                _movement.MoveToDynamicDestination(target.transform, true);

                //order units under command to attack the target
                //might want better control here....
                _commander.OrderAttack(target);
                break;
            case "retreat":
                //return to spawn point, and have units under command follows
                _movement.MoveToReturnPoint();

                _commander.OrderFollowCommander();
                break;
            default:
                base.PerformAction(action);
                return;
        }
    }
}

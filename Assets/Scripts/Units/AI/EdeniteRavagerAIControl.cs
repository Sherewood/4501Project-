using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/* Unit Component */
//Purpose: Handle general combat related prereqs and actions
public class EdeniteRavagerAIControl : CombatAIControl
{

    private Commander _commander;

    //properties
    [Tooltip("The maximum radius from its spawn that the ravager can wander to when looking for munchers")]
    public float WanderMaxRadius;

    [Tooltip("The minimum distance the ravager can wander in one go.")]
    public int WanderMinDist;

    [Tooltip("The maximum distance the ravager can wander in one go.")]
    public int WanderMaxDist;

    [Tooltip("The arc (in degrees) from the ravager's forward direction that it can wander towards.")]
    public int Arc;

    //boolean to track whether the unit should be gathering units or not
    private bool _isGatheringUnits = true;

    //true if retreating, false otherwise
    private bool _retreating = false;

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
        Debug.Log("Ravager AI event: " + aiEvent);
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
            case "insideSpawnPerimeter":
                return Vector3.Distance(transform.position, _movement.GetReturnPoint()) <= WanderMaxRadius;
            case "outsideSpawnPerimeter":
                return Vector3.Distance(transform.position, _movement.GetReturnPoint()) > WanderMaxRadius;
            case "notAtCommandThreshold":
                return aiEvent.Equals(prereq);
            case "reachedCommandThreshold":
                return aiEvent.Equals(prereq);
            case "unitLost":
                return aiEvent.Equals(prereq);
            case "atRetreatThreshold":
                return _commander.UnitCountAtRetreatThreshold();
            //todo: actually make use of the bloody equality checking system I added so I don't need to define two prereqs for each variable
            case "shouldGatherUnits":
                return _isGatheringUnits;
            case "shouldFight":
                return !_isGatheringUnits;
            case "retreating":
                return _retreating;
            case "notRetreating":
                return !_retreating;
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
            case "setFightMode":
                _isGatheringUnits = false;
                break;
            case "setGatheringMode":
                _isGatheringUnits = true;
                break;
            case "wanderNearSpawn":
                //wander near the spawn point
                _movement.WanderToPointWithinRadius(WanderMaxRadius, WanderMinDist, WanderMaxDist, true, Arc);
                //todo: refactor into separate method for setting moving state
                if (_unitState.GetState() != UState.STATE_ATTACKING && _unitState.GetState() != UState.STATE_GUARDING)
                {
                    _unitState.SetState(UState.STATE_MOVING);
                }

                _commander.OrderFollowCommander();
                break;
            case "moveToDestination": //overriden from base class
                //self explanatory
                _movement.MoveToDestination(DetermineTargetPosition(), MovementMode.MODE_PATHFINDING);
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

                //special case: need to refresh the targeting after ending the retreat, because targeting/attack
                //updates are ignored during retreating, so the state will be stale
                if (_retreating)
                {
                    _retreating = false;
                    _targeting.Refresh();
                }

                _commander.OrderHalt();
                break;
            case "moveTarget": //overridden from base class
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
            case "attackTarget": //overridden from base class
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
                //return to near spawn point, and have units under command follow
                //spawn offset added to make it less strict
                _movement.MoveToReturnPoint(3.0f, MovementMode.MODE_PATHFINDING);
                _retreating = true;

                _commander.OrderFollowCommander();
                break;
            case "takeControl":
                //use commander component to take control of nearby edenite munchers
                _commander.SeizeUnitControl();
                break;
            case "requestMainBasePos":
                //trigger position request for main base
                _positionRequestEvent.Invoke("mainBase", this.gameObject);
                break;
            default:
                base.PerformAction(action);
                return;
        }
    }
}

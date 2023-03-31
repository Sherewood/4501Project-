using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Unit Component */

//Purpose: Handle worker related prereqs and actions
public class WorkerAIControl : AIControl
{

    private Construction _construction;
    private Harvesting _harvesting;

    //get construction and harvesting components
    protected override void GetComponents()
    {
        _construction = GetComponent<Construction>();
        if (_construction == null)
        {
            Debug.LogError("Worker AI control cannot find construction component");
        }

        _harvesting = GetComponent<Harvesting>();
        if (_harvesting == null)
        {
            Debug.LogError("Worker AI control cannot find harvesting component");
        }

        base.GetComponents();
    }

    //needed to get the callback to display properly in the inspector....
    public override void HandleAIEvent(string aiEvent)
    {
        base.HandleAIEvent(aiEvent);
    }

    //deposit-related prereq
    protected override bool IsSingleWordPrereqSatisfied(string prereq, string aiEvent)
    {
        if (DebugMode)
        {
            Debug.Log("WorkerAIControl - Checking if prereq: " + prereq + " is satisfied for aiEvent: " + aiEvent);
        }

        switch (prereq)
        {
            case "depositDepleted":
                return (prereq.Equals(aiEvent));
            //todo: add support for other prereqs
            default:
                return base.IsSingleWordPrereqSatisfied(prereq, aiEvent);
        }
    }

    //construction and harvesting related actions
    protected override void PerformStandardAction(string action)
    {
        if (DebugMode)
        {
            Debug.Log("WorkerAIControl - Performing action: " + action);
        }

        GameObject target = DetermineTarget();


        switch (action)
        {
            case "moveToConstructAtDestination":
                //move to the construction site, but stop short according to the offset
                _movement.MoveToDestination(_commandTargetPosition, MovementMode.MODE_PATHFINDING, _construction.GetConstructionSiteOffset());
                //todo: refactor into separate method for setting moving state
                if (_unitState.GetState() != UState.STATE_ATTACKING && _unitState.GetState() != UState.STATE_GUARDING)
                {
                    _unitState.SetState(UState.STATE_MOVING);
                }
                break;
            case "startHarvesting":

                //if able to harvest successfully, then enter harvesting state, else go to idle
                if (_harvesting.StartHarvesting())
                {
                    _unitState.SetState(UState.STATE_HARVESTING);
                }
                else
                {
                    Debug.LogWarning("Unit was told to start harvesting but there was no deposit!");
                    _unitState.SetState(UState.STATE_IDLE);
                }
                break;
            case "startConstruction":
                //construct the building
                _construction.ConstructBuilding();
                break;
            default:
                base.PerformStandardAction(action);
                return;
        }
    }
}

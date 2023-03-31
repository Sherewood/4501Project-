using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/* Unit Component */

//Purpose: Allow the unit to take control of, and issue commands to certain units.
public class Commander : MonoBehaviour
{
    //callback to AI Control
    public AIEvent AICallback;

    //relevant components

    private Targeting _targeting;

    //properties

    [Tooltip("The maximum number of units that this unit can command")]
    public int UnitCap;

    [Tooltip("The threshold of units commanded for which the unit must fall back")]
    public int RetreatThreshold;

    [Tooltip("The range in which the command component should scan when trying to get new units")]
    public int PingRange;

    [Tooltip("The type of unit that this unit can command.")]
    public string CommandedUnitType;

    //units under this unit's command
    private List<GameObject> _controlledUnits;

    void Awake()
    {
        _targeting = GetComponent<Targeting>();

        _controlledUnits = new List<GameObject>();
    }

    void Update()
    {
        //handle clearing out lost units
        CheckIfUnitLost();
    }
    
    /* methods for managing controlled units */

    //attempt to seize control of units in range
    public void SeizeUnitControl()
    {
        //get all units of the desired type in range
        List<GameObject> candidateUnits = _targeting.GetUnitsOfTypeInRange(PingRange, CommandedUnitType);

        foreach(GameObject unit in candidateUnits)
        {
            CommandableUnit unitCommandStatus = unit.GetComponent<CommandableUnit>();

            //skip unit if currently commanded, or for whatever reason lacks a commandable unit component
            //do not need to check if unit is in _controlledUnits because if it is in said array, then it will be under command.
            if (unitCommandStatus == null || unitCommandStatus.IsUnderCommand())
            {
                continue;
            }

            //should not fail since we checked that unit is under command beforehand
            if (!unitCommandStatus.TakeCommand(gameObject))
            {
                Debug.LogWarning(gameObject.name + ": Could not take command of unit: " + unit.name);
                continue;
            }

            _controlledUnits.Add(unit);
            //get the newly commanded unit to stop moving
            Movement unitMovement = unit.GetComponent<Movement>();
            unitMovement.StopMovement();
            //idea for keeping unit from doing anything
            //give unit "idle" command that is only breakable by other command running, or explicit order to break idle
            //that way no rules can be activated
            AIControl unitAI = unit.GetComponent<AIControl>();
            unitAI.SendCommand("idle");

            //if reached threshold for unit control, inform AI control and stop trying to take control of more units
            if(_controlledUnits.Count >= UnitCap)
            {
                AICallback.Invoke("commandThresholdReached");
                return;
            }
        }

        AICallback.Invoke("notAtCommandThreshold");
    }

    private void CheckIfUnitLost()
    {
        bool wasUnitLost = false;

        for (int i = 0; i < _controlledUnits.Count; i++)
        {
            GameObject curUnit = _controlledUnits[i];

            //remove unit if no longer commanded
            if (curUnit == null || curUnit.GetComponent<UnitInfo>() == null)
            {
                wasUnitLost = true;
                _controlledUnits.RemoveAt(i);
                i--;
            }
        }

        //if a unit was lost, inform AI control
        if (wasUnitLost)
        {
            AICallback.Invoke("unitLost");
        }
    }

    /* methods for controlling commanded units */

    //order units to follow the commander
    public void OrderFollowCommander()
    {
        //determine flock, to be sent to unit (not registering these flocks with unit controller, might change later if this sucks)
        List<GameObject> unitFlock = new List<GameObject>(_controlledUnits);
        unitFlock.Add(gameObject);

        //command each unit to follow the commander in a flock
        foreach (GameObject unit in _controlledUnits)
        {
            AIControl unitAI = unit.GetComponent<AIControl>();
            Movement unitMovement = unit.GetComponent<Movement>();

            unitAI.SendCommand("moveFlock", gameObject);
            unitMovement.SetFlock(unitFlock);
        }
    }

    //order units to stop what they are doing
    public void OrderHalt()
    {
        //send each unit an idle command
        foreach(GameObject unit in _controlledUnits)
        {
            AIControl unitAI = unit.GetComponent<AIControl>();

            unitAI.SendCommand("idle");
        }
    }

    //order units to attack a specified target
    public void OrderAttack(GameObject target)
    {
        if(target == null)
        {
            return;
        }
        //send each unit an attack command
        foreach (GameObject unit in _controlledUnits)
        {
            AIControl unitAI = unit.GetComponent<AIControl>();

            unitAI.SendCommand("attack", target);
        }
    }

    //possible other command if I have time - ordering a diversion?
    //basically, order one of the specific units to attack, and the others to follow the commander

    /* queries */
    public bool UnitCountAtRetreatThreshold()
    {
        return _controlledUnits.Count <= RetreatThreshold;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/* Internal Controller Class */
//Purpose: Handle controlling of units, issuing commands

public class UnitController : MonoBehaviour
{

    private SelectionController _selectionController;

    private OrderController _orderController;

    private CapabilityController _capabilityController;

    private GameStateController _gameStateController;

    private EventChainController _eventChainController;

    //the amount of active flocks in the game
    Dictionary<GameObject, List<GameObject>> _flocks;

    // Start is called before the first frame update
    void Start()
    {
        _selectionController = GetComponent<SelectionController>();

        _orderController = GetComponent<OrderController>();

        _capabilityController = GetComponent<CapabilityController>();

        _gameStateController = GetComponent<GameStateController>();

        _eventChainController = GetComponent<EventChainController>();

        _flocks = new Dictionary<GameObject, List<GameObject>>();
    }

    /* Unit command handling (will need to refactor later once we have more capabilities) */

    //Handle order with target
    public void HandleTargetedOrder(Order order, RaycastHit target)
    {
        List<GameObject> selectedUnits = _selectionController.GetSelectedUnits();

        Debug.Log("Handling order: " + order);

        //setting enemy target if attack order given
        if (order == Order.ORDER_ATTACK)
        {
            _selectionController.SetTargetIndicator(target.collider.gameObject);
        }

        /* some future notes for flocking */
        /*
         * If done with player controlled units - should consider the following extra handling in unit controller
         * if move order is given and multiple units are selected
         * 1) Select one of the units that can move, randomly or otherwise, as the flock leader
         * 2) Track that unit, and the group of units that can move associated with it
         *      i) Need to check if any of the units selected were in prior flocking groups, and if so remove them from those groups
         *      ii) If the leader of another flock is in this new flocking group, need to invoke the flocking termination behaviour listed on step 4
         * 3) Order all of the units to move using MODE_PHYSICAL movement, only the flocking leader should be given a destination
         *      i) in movement component, only use steering-type behaviours on flock leader (unit using physics-based movement with a destination)
         *         units that are not the flock leader will only move based on flocking forces (might want to inform them of the leader somehow for the "follow leader" forces)
         *      note: Specifying no destination to a unit will require an update to these methods which I leave to you.
         *      
         * 4) When the flock leader reaches its destination, it will have to send a callback to the internal controller, the unit controller
         *    should do the following
         *      i) Identify the flock leader who finished moving
         *      ii) Order the other units in the flock to stop moving
         *      note: can use the DestinationReachedEvent callback, just bind it and add handling in the event handler to call the unit controller when it happens
         *            will add some overhead due to extra callbacks from units reaching their destination reaching the internal controller, but shouldn't be noticeable.
         *      
         * NOTE: If this proves too complicated, backup option might be to have a group of edenite munchers hardcoded follow a edenite ravager which wanders, or something similar
         *       Would then specify to the TA to check out a group of enemy units on the map demonstrating this behaviour.
         *       Simpler, but leaves more work later if you want to give player units flocking behaviour in the final product. 
         *       
         * NOTE 2: Make sure all flocking commands use "ordered movement", otherwise the unit's attacking component will break the flocking behaviour
         *         by issuing overriding commands.
         */

        GameObject curFlockLeader = null;

        foreach (GameObject selectedUnit in selectedUnits)
        {
            List<Capability> unitCapabilities = _capabilityController.GetCapabilitiesOfUnit(selectedUnit);

            string bestAction = _orderController.DetermineBestActionBasedOnOrder(order, unitCapabilities);

            AIControl unitAI = selectedUnit.GetComponent<AIControl>();

            Movement unitMovement = selectedUnit.GetComponent<Movement>();

            //handle actions
            switch (bestAction)
            {
                case "move":
                    //order AI to move to a location
                    //check and remove object from previous flock if found
                    //need to call this whenever action is being carried out....
                    DeleteUnitFromFlock(selectedUnit);
                    if (selectedUnits.Count > 1)
                    {
                        //will activate always on first unit in list, creates a flock and adds this as the leader
                        //todo: better leader selection algorithm
                        if (curFlockLeader == null)
                        {
                            curFlockLeader = selectedUnit;
                            unitAI.SendCommand("move", target.point);

                            _flocks.Add(curFlockLeader, new List<GameObject>());
                            //still track the flock leader in the flock itself for aiding movement 
                            _flocks[curFlockLeader].Add(curFlockLeader);
                        }
                        //all other selected units will be added to that flock
                        else
                        {
                            unitAI.SendCommand("moveFlock", curFlockLeader);
                            _flocks[curFlockLeader].Add(selectedUnit);
                        }
                    }
                    //if single unit, just order it to move to a destination
                    else
                    {
                        unitAI.SendCommand("move", target.point);
                    }
                    break;
                case "attack":
                    //order AI to attack target
                    unitAI.SendCommand("attack", target.collider.gameObject);
                    break;
                case "harvest":
                    //order AI component to harvest at the specified location
                    unitAI.SendCommand("harvest", target.collider.gameObject.transform.position);
                    break;
                case "construct":
                    //order the unit to move to a location to construct a building
                    unitMovement = selectedUnit.GetComponent<Movement>();
                    Construction construction = selectedUnit.GetComponent<Construction>();
                    string buildingType = construction.GetCurrentBuilding();
                    //purchase actually made here, if the building is obstructed currently nothing in place to refund....
                    if (_gameStateController.CanAffordUnit(buildingType))
                    {
                        _gameStateController.PurchaseUnit(buildingType);
                        unitAI.SendCommand("construct", target.point);
                    }
                    //otherwise, need to reset event chain to prevent bad state
                    else
                    {
                        _eventChainController.RevertEventChainStage();
                    }

                    break;
                case "":
                    break;
                default:
                    Debug.Log("Unsupported action");
                    break;
            }
        }
    }

    //Handle order with no target
    public void HandleUntargetedOrder(Order order, string command)
    {
        List<GameObject> selectedUnits = _selectionController.GetSelectedUnits();

        Debug.Log("Handling order: " + order);

        foreach (GameObject selectedUnit in selectedUnits)
        {
            List<Capability> unitCapabilities = _capabilityController.GetCapabilitiesOfUnit(selectedUnit);

            string bestAction = _orderController.DetermineBestActionBasedOnOrder(order, unitCapabilities);

            AIControl unitAI = selectedUnit.GetComponent<AIControl>();

            switch (bestAction)
            {
                case "guard":
                    //order AI to enter guard mode
                    unitAI.SendCommand("guard");
                    break;
                case "fortify":
                    //order AI to enter fortify mode
                    unitAI.SendCommand("fortify");
                    break;
                case "hyperBoost":
                    //order AI to activate hyper boost
                    unitAI.SendCommand("hyperBoost");
                    break;
                case "returnToBase":
                    //get main base, and order unit to travel relatively close to it
                    GameObject mainBase = _gameStateController.GetMainBase();

                    //assuming main base is square-shaped
                    //add unit scale for extra padding, even though it isn't necessarily correct
                    float mainBaseOffset = mainBase.transform.localScale.x + selectedUnit.transform.localScale.z;

                    //send command to unit AI
                    unitAI.SendCommand("returnToBase", mainBase.transform.position, mainBaseOffset);
                    break;
                case "construct":
                    //get the building type that the player wants to select for the unit, then save it in the unit's construction component.
                    Construction construction = selectedUnit.GetComponent<Construction>();
                    string buildingType = command.Split("_")[1];
                    //only check the wealth here
                    if (_gameStateController.CanAffordUnit(buildingType))
                    {
                        construction.SetCurrentBuilding(buildingType);
                    }
                    //otherwise, need to reset event chain to prevent bad state
                    else
                    {
                        _eventChainController.RevertEventChainStage();
                    }
                    break;

                case "evacuateCivies":
                    //trigger civilian evacuation
                    Civilian civilianComp = selectedUnit.GetComponent<Civilian>();
                    civilianComp.TriggerEvacuation();
                    break;
                case "evacuateMainBase":
                    //try to trigger planetary evacuation
                    PlanetaryEvacuation planetaryEvac = selectedUnit.GetComponent<PlanetaryEvacuation>();
                    planetaryEvac.InitPlanetaryEvac(_gameStateController.GetPlayerResource("fuel"));
                    break;
                case "buildUnit":
                    UnitBuilderComponent unitBuilder = selectedUnit.GetComponent<UnitBuilderComponent>();
                    Debug.Log(command);
                    string unitType = command.Split("_")[1];
                    if (_gameStateController.CanAffordUnit(unitType))
                    {
                        _gameStateController.PurchaseUnit(unitType);
                        unitBuilder._buildQueue.Add(unitType);
                        //adjust to account for varying build times for different units maybe
                        unitBuilder._queueTimers.Add(10);
                    }
                    break;
                default:
                    Debug.Log("Unsupported action");
                    break;

            }
        }
    }

    //deletes unit from its flock, and updates flock movement accordingly based on the status of said unit
    public void DeleteUnitFromFlock(GameObject unit, List<GameObject> selectedUnits = null, string action = "")
    {
        List<int> deleteFlockIndexes = new List<int>();

        //check if unit is flock leader
        if (_flocks.ContainsKey(unit))
        {
            List<GameObject> flockMembers = _flocks[unit];

            /* todo: will not implement until keeping original flock is handled gracefully
            //if selected units are identical to the members of the flock, then keep the flock
            //note: this will not work if enemy units have been selected alongside player units
            if(flockMembers == selectedUnits && action.Equals("move"))
            {

            }
            */
            //order all the flock members to stop moving
            //note: can re-add handling for assigning a new leader to the flock later, but this is easier for now.
            foreach(GameObject flockMember in flockMembers)
            {
                AIControl memberAI = flockMember.GetComponent<AIControl>();
                if (flockMember != unit)
                {
                    //terminate flock movement
                    //do not bother terminating the leader's movement as it will be overridden by its next command

                    //todo: might want more graceful handling for flock units to end up closer to their destination?
                    //perhaps replace flock leader with waypoint at the flock leader's destination, and units terminate their movement once they get close enough...

                    memberAI.StopCommand("moveFlock");
                }
            }
            _flocks.Remove(unit);
            return;
        }
        //otherwise, check active flocks for our flock member, and if found, terminate flock movement and remove it
        else
        {
            List<GameObject> targetFlock = null;

            foreach(List<GameObject> flock in _flocks.Values)
            {
                int index = flock.IndexOf(unit);
                if(index != -1)
                {
                    flock.RemoveAt(index);

                    AIControl memberAI = unit.GetComponent<AIControl>();
                    memberAI.StopCommand("moveFlock");
                }
            }
        }
    }

    //finds the flock based on a given leader of a flock
    public List<GameObject> GetFlock(GameObject leader)
    {
        return _flocks[leader];
    }
}



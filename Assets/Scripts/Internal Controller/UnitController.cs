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

    // Start is called before the first frame update
    void Start()
    {
        _selectionController = GetComponent<SelectionController>();

        _orderController = GetComponent<OrderController>();

        _capabilityController = GetComponent<CapabilityController>();

        _gameStateController = GetComponent<GameStateController>();

        _eventChainController = GetComponent<EventChainController>();
    }

    /* Unit command handling (will need to refactor later once we have more capabilities) */

    //Handle order with target
    public void HandleTargetedOrder(Order order, RaycastHit target)
    {
        List<GameObject> selectedUnits = _selectionController.GetSelectedUnits();

        Debug.Log("Handling order: " + order);

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
         *      note: Specifying no destination to an unit will require an update to these methods which I leave to you.
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

        foreach (GameObject selectedUnit in selectedUnits)
        {
            List<Capability> unitCapabilities = _capabilityController.GetCapabilitiesOfUnit(selectedUnit);

            string bestAction = _orderController.DetermineBestActionBasedOnOrder(order, unitCapabilities);

            AIControl unitAI = selectedUnit.GetComponent<AIControl>();

            //todo remove
            Movement unitMovement = selectedUnit.GetComponent<Movement>();
            
            //handle actions
            switch (bestAction)
            {
                case "move":
                    //order AI to move to a location
                    unitAI.SendCommand("move", target.point);
                    break;
                case "attack":
                    //order unit to move towards enemy, and set enemy as ordered target
                    unitMovement = selectedUnit.GetComponent<Movement>();
                    Targeting unitTargeting = selectedUnit.GetComponent<Targeting>();
                    //move to enemy, not rotation only
                    //temp fix: do not set ordered dynamic destination because the attack component cannot stop it properly...
                    //will have to stop the previously ordered movement in order to prevent it from overriding this action
                    //will also cause the enemy to abandon this order to target nearby enemies.... no real wins here :(
                    unitMovement.StopMovement();
                    unitMovement.MoveToDynamicDestination(target.collider.gameObject.transform, false, MovementMode.MODE_SPLINE);
                    //set ordered target
                    unitTargeting.SetOrderedTarget(target.collider.gameObject);
                    //set attacking state
                    UnitState unitState = selectedUnit.GetComponent<UnitState>();
                    unitState.SetState(UState.STATE_ATTACKING);
                    break;
                case "harvest":
                    unitMovement = selectedUnit.GetComponent<Movement>();
                    Harvesting unitHarvester = selectedUnit.GetComponent<Harvesting>();
                    unitMovement.MoveToHarvest(target.collider.gameObject.transform.position, MovementMode.MODE_SPLINE);
                    unitHarvester.SetTargetResourceDeposit(target.collider.gameObject);
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
                        unitMovement.MoveToConstruct(target.point, MovementMode.MODE_SPLINE);
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
                    //set unit state to guarding, and set return point to the unit's current position
                    UnitState unitState = selectedUnit.GetComponent<UnitState>();
                    Movement movement = selectedUnit.GetComponent<Movement>();
                    movement.SetReturnPoint(selectedUnit.transform.position);
                    unitState.SetState(UState.STATE_GUARDING);
                    break;
                case "fortify":
                    //set unit state to fortified
                    unitState = selectedUnit.GetComponent<UnitState>();
                    unitState.SetState(UState.STATE_FORTIFIED);
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
}

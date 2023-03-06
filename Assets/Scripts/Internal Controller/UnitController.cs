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

    // Start is called before the first frame update
    void Start()
    {
        _selectionController = GetComponent<SelectionController>();

        _orderController = GetComponent<OrderController>();

        _capabilityController = GetComponent<CapabilityController>();
    }

    /* Unit command handling (will need to refactor later once we have more capabilities) */

    //Handle order with target
    public void HandleTargetedOrder(Order order, RaycastHit target)
    {
        List<GameObject> selectedUnits = _selectionController.GetSelectedUnits();

        Debug.Log("Handling order: " + order);

        foreach (GameObject selectedUnit in selectedUnits)
        {
            List<Capability> unitCapabilities = _capabilityController.GetCapabilitiesOfUnit(selectedUnit);

            string bestAction = _orderController.DetermineBestActionBasedOnOrder(order, unitCapabilities);

            switch (bestAction)
            {
                case "move":
                    Movement unitMovement = selectedUnit.GetComponent<Movement>();
                    unitMovement.SetOrderedDestination(target.point);
                    break;
                case "harvest":
                    unitMovement = selectedUnit.GetComponent<Movement>();
                    Harvesting unitHarvester = selectedUnit.GetComponent<Harvesting>();
                    unitMovement.SetOrderedHarvestDestination(target.collider.gameObject.transform.position);
                    unitHarvester.SetTargetResourceDeposit(target.collider.gameObject);
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

            switch (bestAction)
            {
                case "evacuateCivies":
                    //trigger civilian evacuation
                    Civilian civilianComp = selectedUnit.GetComponent<Civilian>();
                    civilianComp.TriggerEvacuation();
                    break;
                case "planetaryEvac":
                    //try to trigger planetary evacuation
                    PlanetaryEvacuation planetaryEvac = selectedUnit.GetComponent<PlanetaryEvacuation>();
                    planetaryEvac.InitPlanetaryEvac(_gameStateController.GetPlayerResource("fuel"));
                    break;
                default:
                    Debug.Log("Unsupported action");
                    break;
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/* Internal Controller Class */
//Purpose: Handles parsing of player commands into "Orders" which will be used to issue commands to units

public class OrderController : MonoBehaviour
{
    private SelectionController _selectionController;

    private OrderModel _orderModel;

    // Start is called before the first frame update
    void Start()
    {
        _selectionController = GetComponent<SelectionController>();
        _orderModel = FindObjectOfType<OrderModel>();
    }

    //determine the Order given a target
    public Order DetermineTargetedOrder(RaycastHit mouseCommand)
    {
        Order chosenOrder = Order.ORDER_INVALID;
        List<string> unitTypes = DetermineSelectedUnitTypes();

        string target = "";
        GameObject targetEntity = mouseCommand.transform.gameObject;
        UnitInfo targetInfo = targetEntity.GetComponent<UnitInfo>();
        
        //if a unit was not targeted, assume terrain was the target
        //might need revisiting depending on what happens with projectiles
        if(targetInfo == null)
        {
            target = "Terrain";
        }
        else
        {
            target = targetInfo.GetUnitType();
        }

        List<Order> possibleOrders = _orderModel.GetValidOrders(true);

        possibleOrders = _orderModel.GetValidOrdersForUnitTypes(possibleOrders, unitTypes);

        possibleOrders = _orderModel.GetValidOrdersForTarget(possibleOrders, target);

        //update once event chain controller is up
        possibleOrders = _orderModel.GetValidOrdersForEventChain(possibleOrders, "");

        if(possibleOrders.Count == 1)
        {
            chosenOrder = possibleOrders[0];
        }
        else if(possibleOrders.Count > 1)
        {
            chosenOrder = _orderModel.GetHighestPriorityOrder(possibleOrders);
        }




        return chosenOrder;
    }

    //determine the Order given no target
    public Order DetermineUntargetedOrder(string command)
    {
        Order chosenOrder = Order.ORDER_INVALID;
        List<string> unitTypes = DetermineSelectedUnitTypes();

        List<Order> possibleOrders = _orderModel.GetValidOrders(true);

        possibleOrders = _orderModel.GetValidOrdersForUnitTypes(possibleOrders, unitTypes);

        //update once event chain controller is up
        possibleOrders = _orderModel.GetValidOrdersForEventChain(possibleOrders, "");

        if (possibleOrders.Count == 1)
        {
            chosenOrder = possibleOrders[0];
        }
        else if (possibleOrders.Count > 1)
        {
            chosenOrder = _orderModel.GetHighestPriorityOrder(possibleOrders);
        }

        return chosenOrder;
    }

    //determine the best available action for a unit based on the given order and the unit's capabilities
    public string DetermineBestActionBasedOnOrder(Order order, List<Capability> capabilities)
    {
        List<string> actions = new List<string>();
        foreach (Capability cap in capabilities)
        {
            actions.Add(cap.ActionName);
        }


        return _orderModel.GetBestActionForOrder(order, actions);
    }

    //helpers

    //get all unique unit types from list of selected units
    private List<string> DetermineSelectedUnitTypes()
    {
        List<GameObject> selectedUnits = _selectionController.GetSelectedUnits();

        List<string> unitTypes = new List<string>();

        foreach (GameObject unit in selectedUnits)
        {
            UnitInfo unitInfo = unit.GetComponent<UnitInfo>();

            string unitType = unitInfo.GetUnitType();

            if (!unitTypes.Contains(unitType))
            {
                unitTypes.Add(unitType);
            }
        }

        return unitTypes;
    }
}

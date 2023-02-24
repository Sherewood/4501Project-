using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/* Data type */
//Purpose: Represents a command given by the player

public enum Order
{
    ORDER_MOVE,
    ORDER_INVALID
}

/* Model class */
//Purpose: Represents 

public class OrderModel : MonoBehaviour
{

    //the list of selected units each Order supports
    private Dictionary<Order, List<string>> _orderUnitTypes;

    //the list of target unit types that each Order supports, or "" if the Order does not have a target
    private Dictionary<Order, List<string>> _orderTargets;

    //the event chains that each Order requires, or "" if the Order requires no event chain
    private Dictionary<Order, string> _orderEventChains;

    //the priorities of each Order, used as a tiebreaker (might not need, revisit later)
    private Dictionary<Order, int> _orderPriorities;

    //the list of actions that can carry out each order, in order of significance
    private Dictionary<Order, List<string>> _orderActions;

    // Start is called before the first frame update
    void Start()
    {
        _orderUnitTypes = new Dictionary<Order, List<string>>();
        _orderTargets = new Dictionary<Order, List<string>>();
        _orderEventChains = new Dictionary<Order, string>();
        _orderPriorities = new Dictionary<Order, int>();
        _orderActions = new Dictionary<Order, List<string>>();

        //movement order
        SetOrderUnitTypes(Order.ORDER_MOVE, new string[] { "all-player-dynamic" });
        SetOrderTargets(Order.ORDER_MOVE, new string[] { "Terrain" });
        SetOrderEventChain(Order.ORDER_MOVE, "");
        SetOrderPriority(Order.ORDER_MOVE, 1);
        SetOrderActions(Order.ORDER_MOVE, new string[] { "move" });



    }

    //helpers for initializing
    private void SetOrderUnitTypes(Order order, string[] orderUnitTypes)
    {
        _orderUnitTypes.Add(order, new List<string>(orderUnitTypes));
    }

    private void SetOrderTargets(Order order, string[] orderTargets)
    {
        _orderTargets.Add(order, new List<string>(orderTargets));
    }

    private void SetOrderEventChain(Order order, string orderEventChain)
    {
        _orderEventChains.Add(order, orderEventChain);
    }

    private void SetOrderPriority(Order order, int priority)
    {
        _orderPriorities.Add(order, priority);
    }

    private void SetOrderActions(Order order, string[] orderActions)
    {
        _orderActions.Add(order, new List<string>(orderActions));
    }

    /* methods for determining orders */

    //get list of valid orders given there being or not being a target
    public List<Order> GetValidOrders(bool hasTarget)
    {
        List<Order> validOrders = new List<Order>();

        //either get all orders with a target, or get all orders with no target
        foreach(Order order in _orderTargets.Keys)
        {
            if ((_orderTargets[order].Count == 0) != hasTarget)
            {
                validOrders.Add(order);
            }

        }

        return validOrders;
    }

    //get list of orders which support any of the selected unit types.
    public List<Order> GetValidOrdersForUnitTypes(List<Order> orders, List<string> unitTypes)
    {
        List<Order> supportedOrders = new List<Order>();

        foreach(Order order in orders)
        {
            List<string> orderSupportedUnitTypes = _orderUnitTypes[order];
            bool foundMatch = false;
            //check if any of the unit types supported by the order match the types of the selected units.
            foreach(string supportedUnitType in orderSupportedUnitTypes)
            {
                string testedUnitType = supportedUnitType;
                bool checkPerfectMatch = true;
                //deal with all flag
                if (supportedUnitType.StartsWith("all-"))
                {
                    testedUnitType = supportedUnitType.Substring(4);
                    checkPerfectMatch = false;
                }

                //compare supported unit type to all selected unit types
                foreach(string unitType in unitTypes)
                {
                    //if the selected unit type matches a supported unit type, add the order to the list of supported orders.
                    if ((checkPerfectMatch && (String.Compare(testedUnitType, unitType) == 0))
                        || (String.Compare(testedUnitType, 0, unitType, 0, testedUnitType.Length) == 0))
                    {
                        foundMatch = true;
                        supportedOrders.Add(order);
                        break;
                    }
                }
                if (foundMatch)
                {
                    break;
                }
            }
        }

        return supportedOrders;
    }

    //get list of orders that support the given target
    //does not need to be called if there is no target
    public List<Order> GetValidOrdersForTarget(List<Order> orders, string targetUnitType)
    {
        List<Order> supportedOrders = new List<Order>();

        foreach(Order order in orders)
        {
            List<string> orderSupportedTargets = _orderTargets[order];

            //check if any of the targets supported by the order match the given target
            foreach(string supportedTarget in orderSupportedTargets)
            {
                string testedTarget = supportedTarget;
                bool checkPerfectMatch = true;
                //deal with all flag
                if (supportedTarget.StartsWith("all-"))
                {
                    testedTarget = supportedTarget.Substring(4);
                    checkPerfectMatch = false;
                }

                //if the selected unit type matches a supported unit type, add the order to the list of supported orders.
                if ((checkPerfectMatch && (String.Compare(testedTarget, targetUnitType) == 0))
                    || (String.Compare(testedTarget, 0, targetUnitType, 0, testedTarget.Length) == 0))
                {
                    supportedOrders.Add(order);
                    break;
                }
            }
        }

        return supportedOrders;
    }

    //get list of orders that require the given event chain
    public List<Order> GetValidOrdersForEventChain(List<Order> orders, string eventChain)
    {
        List<Order> supportedOrders = new List<Order>();

        foreach(Order order in orders)
        {
            if(String.Compare(eventChain, _orderEventChains[order]) == 0)
            {
                supportedOrders.Add(order);
            }
        }

        return supportedOrders;
    }

    //get the highest priority order amongst the given orders
    public Order GetHighestPriorityOrder(List<Order> orders)
    {
        Order bestOrder = Order.ORDER_INVALID;
        int bestPriority = -100;
        foreach(Order order in orders)
        {
            if (_orderPriorities[order] > bestPriority)
            {
                bestOrder = order;
                bestPriority = _orderPriorities[order];
            }
        }

        return bestOrder;
    }

    public string GetBestActionForOrder(Order chosenOrder, List<string> possibleActions)
    {
        List<string> orderActions = _orderActions[chosenOrder];

        string bestAction = "";
        int bestIndex = 999;

        //find the best action for the order by checking which of the possible actions land in
        //the accepted range.
        foreach(string action in possibleActions)
        {
            int x = 0;
            foreach(string orderAction in orderActions)
            {
                if(x >= bestIndex)
                {
                    break;
                }

                if(String.Compare(action, orderAction) == 0)
                {
                    bestAction = orderAction;
                    bestIndex = x;

                    break;
                }
                x++;
            }
        }

        return bestAction;
    }
}

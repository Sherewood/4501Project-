using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/* Data type */
//Purpose: Represents a command given by the player

public enum Order
{
    ORDER_MOVE,
    ORDER_RETURN_TO_BASE,
    ORDER_ATTACK,
    ORDER_GUARD,
    ORDER_FORTIFY,
    ORDER_HARVEST,
    //ORDER_SELECT_BUILDING and ORDER_CONSTRUCT are both for construction
    ORDER_SELECT_BUILDING,
    ORDER_CONSTRUCT,
    ORDER_EVAC_CIVIES,
    ORDER_PLANETARY_EVAC,
    ORDER_BUILD_UNIT,
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

    //the command associated with a given order
    private Dictionary<Order, string> _orderCommand;

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
        _orderCommand = new Dictionary<Order, string>();
        _orderEventChains = new Dictionary<Order, string>();
        _orderPriorities = new Dictionary<Order, int>();
        _orderActions = new Dictionary<Order, List<string>>();

        //movement order
        CreateOrder(Order.ORDER_MOVE, new string[] { "all-player-dynamic" }, new string[] { "Terrain" }, "", "", 1, new string[] { "move" });

        //return to base order
        CreateOrder(Order.ORDER_RETURN_TO_BASE, new string[] { "all-player-dynamic" }, new string[] { }, "returnToBase", "", 1, new string[] { "returnToBase" });

        //attack order
        CreateOrder(Order.ORDER_ATTACK, new string[] { "all-player-dynamic-military" }, new string[] { "all-enemy" }, "", "", 1, new string[] { "attack" });
        
        //guard order
        CreateOrder(Order.ORDER_GUARD, new string[] { "all-player-dynamic-military" }, new string[] { }, "guard", "", 1, new string[] { "guard" });

        //fortify order
        CreateOrder(Order.ORDER_FORTIFY, new string[] { "all-player-dynamic" }, new string[] { }, "fortify", "", 1, new string[] { "fortify" });

        //harvesting order
        CreateOrder(Order.ORDER_HARVEST, new string[] { "player-dynamic-worker" }, new string[] { "neutral-static-mineraldep", "neutral-static-fueldep" }, "", "", 1, new string[] { "harvest" });

        //construction order
        /*
        kind of ugly, but due to the unit controller's design splitting handling of targetted and untargetted order, can have two orders with
        the same action as long as one is targetted and the other is untargeted, the action will mean different things depending on the situation
        Better way of doing this would be to just update the capability model to have one component map to multiple actions if needed, but this works for now...
        Update: ended up doing that capability model change, but this works so I'm too lazy to update it
        */
        CreateOrder(Order.ORDER_SELECT_BUILDING, new string[] { "player-dynamic-worker" }, new string[] { }, "all-construct", "constructionChain-2", 1, new string[] { "construct" });
        CreateOrder(Order.ORDER_CONSTRUCT, new string[] { "player-dynamic-worker" }, new string[] { "Terrain" }, "", "constructionChain-end", 1, new string[] { "construct" });

        //evacuation order
        CreateOrder(Order.ORDER_EVAC_CIVIES, new string[] { "player-static-civilianbuilding" }, new string[] { }, "evacCivies", "", 1, new string[] { "evacuateCivies" });

        //planetary evac order
        CreateOrder(Order.ORDER_PLANETARY_EVAC, new string[] { "player-static-mainbase" }, new string[] { }, "planetaryEvac", "", 1, new string[] { "planetaryEvac" });

        //unit creation order 
        CreateOrder(Order.ORDER_BUILD_UNIT, new string[] { "player-static-barracks", "player-static-factory" }, new string[] { }, "all-buildUnit", "", 1, new string[] { "buildUnit" });
    }

    private void CreateOrder(Order order, string[] orderUnitTypes, string[] orderTargets, string orderCommand, string eventChain, int priority, string[] orderActions)
    {
        SetOrderUnitTypes(order, orderUnitTypes);
        SetOrderTargets(order, orderTargets);
        SetOrderCommand(order, orderCommand);
        SetOrderEventChain(order, eventChain);
        SetOrderPriority(order, priority);
        SetOrderActions(order, orderActions);
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

    private void SetOrderCommand(Order order, string orderCommand)
    {
        _orderCommand.Add(order, orderCommand);
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
                //compare supported unit type to all selected unit types
                foreach(string unitType in unitTypes)
                {
                    //if the selected unit type matches a supported unit type, add the order to the list of supported orders.
                    if (DoFieldsMatch(unitType, supportedUnitType))
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
                //if the selected unit type matches a supported unit type, add the order to the list of supported orders.
                if (DoFieldsMatch(targetUnitType, supportedTarget))
                {
                    supportedOrders.Add(order);
                    break;
                }
            }
        }

        return supportedOrders;
    }

    //get list of orders that require the given command
    public List<Order> GetValidOrdersForCommand(List<Order> orders, string command)
    {
        List<Order> supportedOrders = new List<Order>();

        foreach (Order order in orders)
        {
            if (DoFieldsMatch(command, _orderCommand[order]))
            {
                supportedOrders.Add(order);
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
            if(DoFieldsMatch(eventChain, _orderEventChains[order]))
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
        //if invalid, no action is best action
        if(chosenOrder == Order.ORDER_INVALID)
        {
            return "";
        }

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

    private bool DoFieldsMatch(string currentField, string targetField)
    {
        //handle all flag
        if (targetField.StartsWith("all-"))
        {
            string actualTargetField = targetField.Substring(4);
            return (String.Compare(currentField, 0, actualTargetField, 0, actualTargetField.Length) == 0);
        }
        else
        {
            return (String.Compare(currentField, targetField) == 0);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/* Event definitions go here */

[System.Serializable]
public class SelectionEvent : UnityEvent<RaycastHit> { }

[System.Serializable]
public class MouseOrderEvent : UnityEvent<RaycastHit> { }

[System.Serializable]
public class UIOrderEvent : UnityEvent<string> { }

[System.Serializable]
public class MenuSelectionEvent : UnityEvent<string> { }

[System.Serializable]
public class DirectionKeyEvent : UnityEvent<string> { }

[System.Serializable]
public class EntitySpawnEvent : UnityEvent<GameObject> { }

[System.Serializable]
public class EntityDeadEvent : UnityEvent<GameObject> { }

[System.Serializable]
public class ResourceHarvestEvent : UnityEvent<string, int> { }

[System.Serializable]
public class CivilianEvacEvent : UnityEvent<int> { }

[System.Serializable]
public class EndOfGameEvent : UnityEvent<bool> { }

/* Internal Controller Class */
// Handles incoming events to the Internal Controller


public class InternalControllerEventHandler : MonoBehaviour
{
    // Controller classes
    private CameraController _cameraController;

    private SelectionController _selectionController;

    private UnitCreationController _unitCreationController;

    private OrderController _orderController;

    private UnitController _unitController;

    private GameStateController _gameStateController;

    private EventChainController _eventChainController;

    private DisplayInfoController _displayInfoController;

    // Link other controller classes here
    void Start()
    {
        _cameraController = GetComponent<CameraController>();

        _selectionController = GetComponent<SelectionController>();

        _unitCreationController = GetComponent<UnitCreationController>();

        _orderController = GetComponent<OrderController>();

        _unitController = GetComponent<UnitController>();

        _gameStateController = GetComponent<GameStateController>();

        _eventChainController = GetComponent<EventChainController>();

        _displayInfoController = GetComponent<DisplayInfoController>();
    }

    // Event callback functions

    //handle unit selection given via mouse click
    public void HandleSelectionEvent(RaycastHit selectionTarget)
    {
        Debug.Log("Selection event received");

        //for now, won't consider the target here when updating event chain
        //if that has to change, special method should be defined in event chain controller for it
        _eventChainController.HandleEventChainUpdateGeneral("unitSelection");

        _selectionController.HandleSingleSelection(selectionTarget);

        //clear additional menu options open if a new unit is selected
        //usage might need to be revisited when we test MenuSelectionEvents/get Display Info Controllers
        _displayInfoController.ClearAdditionalInfo();
    }

    //handle command given by mouse click
    public void HandleMouseOrderEvent(RaycastHit orderTarget)
    {
        Debug.Log("Mouse order event received");

        //event chain can influence determined order, therefore must be update first.
        _eventChainController.HandleEventChainMouseOrderUpdate("mouseOrder", orderTarget);

        Order order = _orderController.DetermineTargetedOrder(orderTarget);

        if (order != Order.ORDER_INVALID)
        {
            _unitController.HandleTargetedOrder(order, orderTarget);

            //clear additional menu options open if a new unit is selected
            //usage might need to be revisited when we test MenuSelectionEvents/get Display Info Controllers
            _displayInfoController.ClearAdditionalInfo();
        }
        else
        {
            Debug.LogWarning("Invalid Order received from prior mouse order event.");
        }
    }

    //handle command given by UI action
    public void HandleUIOrderEvent(string command)
    {
        Debug.Log("UI order event received, command " + command);

        //event chain can influence determined order, therefore must be determined first.
        _eventChainController.HandleEventChainUIEventUpdate("UIOrder", command);

        Order order = _orderController.DetermineUntargetedOrder(command);

        if (order != Order.ORDER_INVALID)
        {
            _unitController.HandleUntargetedOrder(order, command);
        }
        else
        {
            Debug.LogWarning("Invalid Order received from prior UI order event.");
        }
    }

    //handle command to fetch additional menu information from UI
    public void HandleMenuSelectionEvent(string command)
    {
        Debug.Log("Menu Selection event received, command: " + command);

        //event chain might be advanced by menu selection
        _eventChainController.HandleEventChainUIEventUpdate("menuSelection", command);

        //display info controller will find additional information to display based on the command.
        _displayInfoController.UpdateAdditionalDisplayInfo(command);
    }

    //handle indication of direction from key presses
    public void HandleDirectionKeyString(string direction)
    {
        //set the camera's movement direction
        _cameraController.SetDirection(direction);
    }

    //handle reporting of new entity
    public void HandleUnitSpawnEvent(GameObject newUnit)
    {
        Debug.Log("Unit Spawn Event received - unit instance id " + newUnit.GetInstanceID());

        _unitCreationController.StoreCreatedEntity(newUnit);
    }

    //handle reporting of entity death
    public void HandleUnitDeadEvent(GameObject newUnit)
    {
        Debug.Log("Unit Dead Event received - unit instance id " + newUnit.GetInstanceID());

        _unitCreationController.DeleteDeadEntity(newUnit);
    }

    //handle harvesting of resource deposit
    public void HandleResourceHarvestEvent(string resourceType, int resourceAmount)
    {
        Debug.Log("Resource Harvest Event received - resource type: " + resourceType + ", resource amount: " + resourceAmount);

        //store newly gained resources in game state model
        _gameStateController.StoreHarvestedResource(resourceType, resourceAmount);
    }

    //handle evacuation of civilian(s) from a civilian building
    public void HandleCivilianEvacEvent(int numCivilians)
    {
        Debug.Log("Evacuate Civilian Event received - " + numCivilians + " evacuated.");

        _gameStateController.EvacuateCivilians(numCivilians);
    }

    //handle end of game
    public void HandleEndOfGameEvent(bool won)
    {
        Debug.Log("End Of Game Event received - " + (won ? "player won!" : "player lost."));
    }

    // Helper functions
}

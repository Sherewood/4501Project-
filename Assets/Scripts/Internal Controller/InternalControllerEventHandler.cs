using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
using UnityEngine.SceneManagement;

/* Event definitions go here */

[System.Serializable]
public class SelectionEvent : UnityEvent<RaycastHit> { }

[System.Serializable]
public class AreaSelectionEvent : UnityEvent<RaycastHit, RaycastHit> { }

[System.Serializable]
public class MouseOrderEvent : UnityEvent<RaycastHit> { }

[System.Serializable]
public class UIOrderEvent : UnityEvent<string> { }

[System.Serializable]
public class MenuSelectionEvent : UnityEvent<string> { }

[System.Serializable]
public class ResearchTechEvent : UnityEvent<string> { }

[System.Serializable]
public class DirectionKeyEvent : UnityEvent<string> { }

[System.Serializable]
public class EntitySpawnEvent : UnityEvent<GameObject> { }

[System.Serializable]
public class EntityDeadEvent : UnityEvent<GameObject> { }

//for requesting the position of a certain game element (usually the main base)
[System.Serializable]
public class PositionRequestEvent : UnityEvent<string, GameObject> { }

//for when a unit reaches its destination
[System.Serializable]
public class DestinationReachedEvent : UnityEvent<GameObject> { }

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
    void Awake()
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
        Debug.Log("Unit Selection event received, selection instanceID: " + selectionTarget.collider.gameObject.GetInstanceID());

        //for now, won't consider the target here when updating event chain
        //if that has to change, special method should be defined in event chain controller for it
        _eventChainController.HandleEventChainUpdateGeneral("unitSelection");

        _selectionController.HandleSingleSelection(selectionTarget);

        //clear additional menu options open if a new unit is selected
        _displayInfoController.ClearAdditionalInfo();
    }

    //handle selection of a region of the map done via the mouse
    public void HandleAreaSelectionEvent(RaycastHit initialSelection, RaycastHit finalSelection)
    {
        Debug.Log("Area Selection event received, Point 1: " + initialSelection.point + ", Point 2: " + finalSelection.point);

        //for now, won't consider the target here when updating event chain
        //if that has to change, special method should be defined in event chain controller for it
        _eventChainController.HandleEventChainUpdateGeneral("areaSelection");

        _selectionController.HandleAreaSelection(initialSelection, finalSelection);

        //clear additional menu options open if a new unit is selected
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

        //lazy corner case handling - if worker unit fortifies during construction chain,
        //it will break the event chain but the construction options in the UI do not disappear, so they need to be cleared
        if (order == Order.ORDER_FORTIFY)
        {
            _displayInfoController.ClearAdditionalInfo();
        }

        if (order != Order.ORDER_INVALID)
        {
            _unitController.HandleUntargetedOrder(order, command);
        }
        else
        {
            Debug.LogWarning("Invalid Order received from prior UI order event.");
        }
    }

    //handle request to research technology
    public void HandleResearchTechEvent(string techId)
    {
        Debug.Log("Research tech event received, tech id: " + techId);

        //try to complete research of tech, if successful, refresh available technologies
        if (_gameStateController.ResearchTechnology(techId))
        {
            Debug.Log("Tech research successful.");
            _displayInfoController.UpdateResearchMenuInfo();
        }
    }

    //handle command to fetch additional menu information from UI
    public void HandleMenuSelectionEvent(string command)
    {
        Debug.Log("Menu Selection event received, command: " + command);

        //event chain might be advanced by menu selection
        _eventChainController.HandleEventChainUIEventUpdate("menuSelection", command);

        //display info controller will find additional information to display based on the command.
        _displayInfoController.UpdateAdditionalMenuInfo(command);
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

    //handle position request
    public void HandlePositionRequestEvent(string requestType, GameObject requestingUnit)
    {
        _gameStateController.HandlePositionRequest(requestType, requestingUnit);
    }

    //handle unit reaching destination
    public void HandleDestinationReachedEvent(GameObject unit)
    {
        //todo: dedicated unit controller method for reaching destination?
        _unitController.DeleteUnitFromFlock(unit);
    }

    //handle harvesting of resource deposit
    public void HandleResourceHarvestEvent(string resourceType, int resourceAmount)
    {
        //Debug.Log("Resource Harvest Event received - resource type: " + resourceType + ", resource amount: " + resourceAmount);

        //store newly gained resources in game state model
        _gameStateController.StoreHarvestedResource(resourceType, resourceAmount);
    }

    //handle evacuation of civilian(s) from a civilian building
    public void HandleCivilianEvacEvent(int numCivilians)
    {
        //Debug.Log("Evacuate Civilian Event received - " + numCivilians + " evacuated.");

        _gameStateController.EvacuateCivilians(numCivilians);
    }

    //handle end of game
    public void HandleEndOfGameEvent(bool won)
    {
        Debug.Log("End Of Game Event received - " + (won ? "player won!" : "player lost."));

        if (!won)
        {
            SceneManager.LoadScene("GameOverScene", LoadSceneMode.Single);
        } 
    }

    // Helper functions
}

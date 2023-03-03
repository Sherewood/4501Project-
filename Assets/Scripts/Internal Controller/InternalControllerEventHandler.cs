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
public class DirectionKeyEvent : UnityEvent<string> { }

[System.Serializable]
public class EntitySpawnEvent : UnityEvent<GameObject> { }

[System.Serializable]
public class EntityDeadEvent : UnityEvent<GameObject> { }

[System.Serializable]
public class ResourceHarvestEvent : UnityEvent<string, int> { }

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

    // Link other controller classes here
    void Start()
    {
        _cameraController = GetComponent<CameraController>();

        _selectionController = GetComponent<SelectionController>();

        _unitCreationController = GetComponent<UnitCreationController>();

        _orderController = GetComponent<OrderController>();

        _unitController = GetComponent<UnitController>();
    }

    // Event callback functions

    //handle unit selection given via mouse click
    public void HandleSelectionEvent(RaycastHit selectionTarget)
    {
        Debug.Log("Selection event received");

        _selectionController.HandleSingleSelection(selectionTarget);
    }

    //handle command given by mouse click
    public void HandleMouseOrderEvent(RaycastHit orderTarget)
    {
        Debug.Log("Mouse order event received");

        Order order = _orderController.DetermineTargetedOrder(orderTarget);

        if (order != Order.ORDER_INVALID)
        {
            _unitController.HandleTargetedOrder(order, orderTarget);
        }
    }

    //handle command given by UI action
    public void HandleUIOrderEvent(string command)
    {
        Debug.Log("UI order event received");

        Order order = _orderController.DetermineUntargetedOrder(command);

        if (order != Order.ORDER_INVALID)
        {
            _unitController.HandleUntargetedOrder(order, command);
        }
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

        //TODO: Game State Controller method which updates Game State Model
    }

    // Helper functions
}

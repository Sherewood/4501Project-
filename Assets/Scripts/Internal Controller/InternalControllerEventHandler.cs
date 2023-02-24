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
public class DirectionKeyEvent : UnityEvent<string> { }

[System.Serializable]
public class EntitySpawnEvent : UnityEvent<GameObject> { }

[System.Serializable]
public class EntityDeadEvent : UnityEvent<GameObject> { }

/* Internal Controller Class */
// Handles incoming events to the Internal Controller


public class InternalControllerEventHandler : MonoBehaviour
{
    // Controller classes
    private CameraController _cameraController;

    private SelectionController _selectionController;

    private UnitCreationController _unitCreationController;

    // Link other controller classes here
    void Start()
    {
        _cameraController = GetComponent<CameraController>();

        _selectionController = GetComponent<SelectionController>();

        _unitCreationController = GetComponent<UnitCreationController>();
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
        //temporary, for debug purposes (will be replaced by proper handling when ready)
        Debug.Log("Mouse order event received");
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

    // Helper functions
}

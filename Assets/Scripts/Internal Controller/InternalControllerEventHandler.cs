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

/* Internal Controller Class */
// Handles incoming events to the Internal Controller


public class InternalControllerEventHandler : MonoBehaviour
{
    // Controller classes


    // Link other controller classes here
    void Start()
    {

    }

    // Event callback functions

    //handle unit selection given via mouse click
    public void HandleSelectionEvent(RaycastHit selectionTarget)
    {
        //temporary, for debug purposes (will be replaced by proper handling when ready)
        Debug.Log("Selection event received");
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
        //temporary, for debug purposes (will be replaced by proper handling when ready)
        Debug.Log("Current direction is: " + direction);
    }

    // Helper functions
}

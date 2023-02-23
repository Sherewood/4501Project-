using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/* Event definitions go here */

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

    //handle indication of direction from key presses
    public void HandleDirectionKeyString(string direction)
    {
        //temporary, for debug purposes (will be replaced by proper handling when ready
        Debug.Log("Current direction is: " + direction);
    }

    // Helper functions
}

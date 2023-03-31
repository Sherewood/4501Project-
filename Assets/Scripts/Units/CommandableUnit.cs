using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
/* Unit component */

//Purpose: Provide an interface that allows Commander units to mark this unit as under their control
//commands from the commander component to the unit ARE NOT issued through here...
public class CommandableUnit : MonoBehaviour
{
    //callback to AI Control
    public AIEvent AICallback;

    //the current commander of this unit
    private GameObject _commander;

    private AIControl _unitAI;

    void Awake()
    {
        _unitAI = GetComponent<AIControl>();
    }

    //handle a request to hand control of the unit to the given commander
    public bool TakeCommand(GameObject newCommander)
    {
        //do not set if already under command from another unit
        if (_commander != null)
        {
            return false;
        }

        _commander = newCommander;
        AICallback.Invoke("unitTookControl");
        return true;
    }

    //handle a request to release control of the unit from its commander
    public void ReleaseCommand()
    {
        _commander = null;
        AICallback.Invoke("unitLostControl");
    }

    //return true if unit has a commander, false otherwise
    public bool IsUnderCommand()
    {
        return (_commander != null);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/* Internal Controller class */
//Purpose: Handle chaining together events

//should be called for every event, but for efficiency's sake probably best to avoid calling on events
//that are never triggered by the player, in particular periodic ones, as these events probably won't influence the event chain.

public class EventChainController : MonoBehaviour
{

    private EventChainModel _eventChainModel;

    private string _curEventChain;

    void Start()
    {
        _curEventChain = "";

        _eventChainModel = FindObjectOfType<EventChainModel>();
    }

    public string GetCurrentEventChain()
    {
        return _curEventChain;
    }

    //handles event chain updates for mouse orders
    public void HandleEventChainMouseOrderUpdate(string eventName, RaycastHit target)
    {
        //determine full event name based on the target

        //if the target is a unit, only need to specify unit is targetted for now
        if(target.collider.gameObject.GetComponent<UnitInfo>() != null)
        {
            eventName = eventName + "-" + "unitTarget";
        }
        //else, specify terrain was targetted
        else
        {
            eventName = eventName + "-" + "terrain";
        }

        //perform general handling with full event name
        HandleEventChainUpdateGeneral(eventName);
    }

    //handles event chain updates for UI commands
    public void HandleEventChainUIEventUpdate(string eventName, string eventCommand)
    {
        //determine full event nname based on the command
        eventName = eventName + "-" + eventCommand;

        //perform general handling with full event name
        HandleEventChainUpdateGeneral(eventName);
    }

    //handles general event chain updating
    public void HandleEventChainUpdateGeneral(string eventName)
    {
        //if at end of event chain, assume action was already carried out from prior event that reached end of chain
        if (_curEventChain.EndsWith("-end")){
            _curEventChain = "";
        }

        Debug.Log("Event Chain Controller - Handling Event Chain Update for event: " + eventName);

        //update current event chain using event chain model
        _curEventChain = _eventChainModel.UpdateCurrentEventChainStatus(_curEventChain, eventName);

        Debug.Log("Event Chain Controller - new event chain status is: " + _curEventChain);
    }
}

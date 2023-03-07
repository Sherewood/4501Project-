using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
/* Model class */
//Purpose: Stores information on what events continue event chains, and what events break them

public class EventChainModel : MonoBehaviour
{

    private Dictionary<string, List<string>> _eventChainCompletionRequirements;

    private Dictionary<string, List<string>> _eventChainBreakingEvents;

    // Start is called before the first frame update
    void Start()
    {
        _eventChainCompletionRequirements = new Dictionary<string, List<string>>();

        _eventChainBreakingEvents = new Dictionary<string, List<string>>();

        //construction event chain
        //1st stage - construct option selected
        //2nd stage - building selected
        //3rd stage - mouse order given, targeting terrain
        CreateEventChain("constructionChain", new string[]{ "menuSelect-construct", "all-uiOrder-construct", "mouseOrder-terrain" },
            new string[] {"all-unitSelection","all-areaSelection","all-mouseOrder"});
    }

    private void CreateEventChain(string eventChainName, string[] eventChainSequence, string[] breakingEvents)
    {
        _eventChainCompletionRequirements.Add(eventChainName, new List<string>(eventChainSequence));
        _eventChainBreakingEvents.Add(eventChainName, new List<string>(breakingEvents));
    }

    //updates current event chain status depending on the event that just occurred.
    public string UpdateCurrentEventChainStatus(string currentEventChain, string currentEvent)
    {
        //if not in event chain
        if(currentEventChain == "")
        {
            //check if the current event starts any existing event chains
            foreach (string eventChain in _eventChainCompletionRequirements.Keys)
            {
                string firstEventInChain = _eventChainCompletionRequirements[eventChain][0];

                //if it does, the first event in the chain has occurred
                if(DoesEventMatch(currentEvent, firstEventInChain))
                {
                    return DetermineEventChainString(eventChain, 0);
                }
            }
        }
        else
        {
            //a) If current event matches next event in the current event chain, advance the current event chain
            //b) Otherwise, check if it is one of the events that can break the event chain
            //  i) If it can break the event chain, call this method again with currentEventChain=""
            //  ii) Else, return the current event chain value as the event chain state has not changed.
            string currentEventChainName = currentEventChain.Split("-")[0];
            
            int eventChainIndex = GetEventChainIndex(currentEventChainName);

            string nextEventInChain = _eventChainCompletionRequirements[currentEventChainName][eventChainIndex+1];

            if (DoesEventMatch(currentEvent, nextEventInChain))
            {
                return DetermineEventChainString(currentEventChainName, eventChainIndex+1);
            }

            //checking if event can break event chain
            foreach (string breakingCandidate in _eventChainBreakingEvents[currentEventChainName])
            {
                //if so, event chain is broken, re-determine current event chain as if there is no event.
                if (DoesEventMatch(currentEvent, breakingCandidate))
                {
                    return UpdateCurrentEventChainStatus("",currentEvent);
                }
            }
        }

        return currentEventChain;
    }

    //current event chain value is <event chain name>-<step in event chain>
    //this method converts that step to an index in the corresponding completion requirements list
    private int GetEventChainIndex(string currentEventChain)
    {
        return int.Parse(currentEventChain.Split("-")[1]) - 1;
    }

    private string DetermineEventChainString(string eventChain, int index)
    {
        if(index >= _eventChainCompletionRequirements[eventChain].Count)
        {
            return eventChain + "-end";
        }
        else
        {
            return eventChain + "-" + (index + 1);
        }
    }

    private bool DoesEventMatch(string currentEvent, string eventChainEvent)
    {
        //handle all flag
        if (eventChainEvent.StartsWith("all-"))
        {
            string actualEventChainEvent = eventChainEvent.Substring(4);
            return (String.Compare(currentEvent, 0, actualEventChainEvent, 0, actualEventChainEvent.Length) == 0);
        }
        else
        {
            return (String.Compare(currentEvent, eventChainEvent) == 0);
        }
    }
}

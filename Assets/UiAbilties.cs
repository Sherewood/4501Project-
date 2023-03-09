using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiAbilties : MonoBehaviour
{
    public (string,UIEvTrigger) Event; //Trigger for button 
    public Sprite Icon; //button icon
    private string Description; //string for later on which holds details 
    public bool menuEvent;
    public string testEvent="test";//meant for debugging
    // Start is called before the first frame update
    void Start()
    {

        Event.Equals(("", UIEvTrigger.TRIGGER_NONE));
        menuEvent = false;
        testEvent = "test";
    }

    // Update is called once per frame
    void Update()
    {
        this.GetComponent<Image>().sprite = Icon;
        this.testEvent = Event.Item1;
        
    }
    public void setTrigger((string, UIEvTrigger) odio)
    {
        Event=odio;
        this.testEvent = Event.Item1;
        
    }
    //send corresponding event depending on the UI trigger type
    public void SendTrigger()
    {
        if (Event.Item2 == UIEvTrigger.TRIGGER_NONE) { return; }
        if (Event.Item2 == UIEvTrigger.TRIGGER_MENUSELECT) { GameObject.Find("InternalController").GetComponent<InternalControllerEventHandler>().HandleMenuSelectionEvent(Event.Item1); Debug.Log("Crump"); }//if the event is a menuselection, triggers the method 
        else { GameObject.Find("InternalController").GetComponent<InternalControllerEventHandler>().HandleUIOrderEvent(Event.Item1); }
        // otherwise passes it to the ui handler 
    }
}

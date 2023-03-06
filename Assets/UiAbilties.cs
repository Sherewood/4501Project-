using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiAbilties : MonoBehaviour
{
    public (string,UIEvTrigger) Event; //Trigger for button 
    
    public Sprite Icon; //button icon
    private string Description; //string for later on which holds details 
    // Start is called before the first frame update
    void Start()
    {
        Event.Equals(("", UIEvTrigger.TRIGGER_NONE));
        
    }

    // Update is called once per frame
    void Update()
    {
        this.GetComponent<Image>().sprite = Icon;
    }
    public void setTrigger((string, UIEvTrigger) odio)
    {
        Event=odio;
    }
    public void SendTrigger()
    {
        GameObject.Find("InternalController").GetComponent<InternalControllerEventHandler>().HandleUIOrderEvent(Event.Item1);
    }
    public void Seticon()
    {
        //sets the material 
    }
}

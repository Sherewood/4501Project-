using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UiAbilties : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public (string,UIEvTrigger) Event; //Trigger for button 
    public Sprite Icon; //button icon
    private string Description; //string for later on which holds details 
    public bool menuEvent;
    public bool researchEvent;
    private string describe = "";
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
       // this.GetComponent<Image>().sprite = Icon;
        this.testEvent = Event.Item1;
     
        
    }
    public void setTrigger((string, UIEvTrigger) odio)
    {
        Event=odio;
        this.testEvent = Event.Item1;
        
    }
    public void setIcon (Sprite newicon)
    {
        Icon= newicon;
        this.GetComponent<Image>().sprite = Icon;
    }
    //send corresponding event depending on the UI trigger type
    public void SendTrigger()
    {
        if (Event.Item2 == UIEvTrigger.TRIGGER_NONE) { return; }
        else if (Event.Item2 == UIEvTrigger.TRIGGER_MENUSELECT) {
            //trigger Menu Selection Event
            FindObjectOfType<InternalControllerEventHandler>().HandleMenuSelectionEvent(Event.Item1);
        }
        else if (Event.Item2 == UIEvTrigger.TRIGGER_RESEARCHTECH)
        {
            FindObjectOfType<InternalControllerEventHandler>().GetComponent<InternalControllerEventHandler>().HandleResearchTechEvent(Event.Item1);
        }
        else {
            // otherwise triggers UIOrder Event
            FindObjectOfType<InternalControllerEventHandler>().GetComponent<InternalControllerEventHandler>().HandleUIOrderEvent(Event.Item1); 
        }
        
    }
    public void setDescription(string line)
    {
        describe = line;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (researchEvent)
        {
            GameObject.Find("ScienceDescription").GetComponent<TextMeshProUGUI>().text =describe;
        }
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (researchEvent)
        {
            GameObject.Find("ScienceDescription").GetComponent<TextMeshProUGUI>().text = "";
        }
    }
}

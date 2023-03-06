using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//represents the type of event that is triggered when a certain action is selected
public enum UIEvTrigger
{
    TRIGGER_UIORDER,
    TRIGGER_MENUSELECT,
    TRIGGER_NONE
}

/* Internal Controller */

//Purpose: Providing information to the UI upon request
//Originally intended to push information to the UI controller, but will instead provide interfaces for the UI controller to access

public class DisplayInfoController : MonoBehaviour
{

    private SelectionController _selectionController;

    private GameStateController _gameStateController;

    private UnitDatabase _unitDatabase;

    //stores mappings between actions that will be displayed in the UI, and the event types triggered when these actions are selected
    //should be in the capability model in some form but whatever...
    private Dictionary<string, UIEvTrigger> _actionEventTypeMappings;

    //additional information for the UI to display, gathered based on request
    private Dictionary<string, UIEvTrigger> _additionalDisplayInfo;

    // Start is called before the first frame update
    void Start()
    {
        _selectionController = GetComponent<SelectionController>();
        _gameStateController = GetComponent<GameStateController>();

        _unitDatabase = FindObjectOfType<UnitDatabase>();

        _additionalDisplayInfo = new Dictionary<string, UIEvTrigger>();

        InitActionEventTypeMappings();
    }

    private void InitActionEventTypeMappings()
    {
        _actionEventTypeMappings = new Dictionary<string, UIEvTrigger>();

        _actionEventTypeMappings.Add("guard", UIEvTrigger.TRIGGER_UIORDER);
        _actionEventTypeMappings.Add("evacuateCivies", UIEvTrigger.TRIGGER_UIORDER);
        _actionEventTypeMappings.Add("evacuateMainBase", UIEvTrigger.TRIGGER_UIORDER);
    }

    /* Helpers (to be called directly by the UI) */

    //return the list of selected units
    public List<GameObject> GetSelectedUnits()
    {
        return _selectionController.GetSelectedUnits();
    }

    //return the list of actions available to the selected units, alongside the type of event selecting them from the UI should trigger
    public Dictionary<string, UIEvTrigger> GetSelectedUnitActions()
    {
        List<Capability> capabilities = _selectionController.GetSelectedUnitCapabilities();

        Dictionary<string, UIEvTrigger> actions = new Dictionary<string, UIEvTrigger>();

        foreach(Capability c in capabilities)
        {
            actions.Add(c.ActionName, GetActionUIEvTrigger(c.ActionName));
        }

        return actions;
    }

    public Dictionary<string, UIEvTrigger> GetAdditionalMenuInfo()
    {
        return _additionalDisplayInfo;
    }

    //given a list of resource types (minerals, fuel), returns a dictionary mapping resource types to the number of resources the player has
    public Dictionary<string, int> GetPlayerResources(List<string> resourceTypes)
    {
        Dictionary<string, int> resourceAmounts = new Dictionary<string, int>();

        foreach (string resourceType in resourceTypes)
        {
            int resourceAmount = _gameStateController.GetPlayerResource(resourceType);
            resourceAmounts.Add(resourceType, resourceAmount);
        }

        return resourceAmounts;
    }

    //given a unit type, returns the sprite associated with that unit type
    public Texture2D GetUnitIcon(string unitType)
    {
        return _unitDatabase.GetUnitIcon(unitType);
    }


    /* event handling */

    public void DisplayAdditionalInfo(string command)
    {

        /*
        get additional display information /alter display information based on command
        example: if 'construct' command sent, should
          1. Get selected worker unit (if more than 1 unit selected log error)
          2. Get list of supported buildings from worker unit's Construction component
          3. Update additionalDisplayInfo with 'construct-<building's unit type>' entries,
             and their corresponding UIEvTrigger (should all be TRIGGER_MENUSELECT) (might revisit this when I get to construction...)

        example2: If 'buildUnit' command sent, should...
          1. Get selected barracks/factory unit (if more than 1 selected log error)
          2. Get list of supported units from Unit Builder component
          3. Update additionalDisplayInfo with 'build-<unit's type>' entries, 
             and their corresponding UIEvTrigger (should all be TRIGGER_UIORDER)
        */
    }

    //clear all additional display information
    public void ClearAdditionalInfo()
    {
        _additionalDisplayInfo.Clear();
    }



    //future todo: for MenuSelectionEvent, will need helper method that only returns a list of values when a certain state is set
    //for example: if build unit button clicked, this list of values would become the list of unit types that can be built at the selected barracks/factory

    /* internal helpers */

    //returns the UI event to be triggered when the given action is selected
    private UIEvTrigger GetActionUIEvTrigger(string actionName)
    {
        if (_actionEventTypeMappings.ContainsKey(actionName))
        {
            return _actionEventTypeMappings[actionName];
        }
        else
        {
            return UIEvTrigger.TRIGGER_NONE;
        }
    }

}

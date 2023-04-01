using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//represents the type of event that is triggered when a certain action is selected
public enum UIEvTrigger
{
    TRIGGER_UIORDER,
    TRIGGER_MENUSELECT,
    TRIGGER_RESEARCHTECH,
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

    private ResearchModel _researchModel;

    //stores mappings between actions that will be displayed in the UI, and the event types triggered when these actions are selected
    //should be in the capability model in some form but whatever...
    private Dictionary<string, UIEvTrigger> _actionEventTypeMappings;

    //information for the construction menu (units/buildings available to build)
    private Dictionary<string, UIEvTrigger> _constructionMenuInfo;

    //information for the research menu
    private Dictionary<Technology, UIEvTrigger> _researchMenuInfo;
    private bool _researchMenuActive;
    private bool _researchMenuChanged;

    private Sun sun;
    

    // Start is called before the first frame update
    void Start()
    {
        _selectionController = GetComponent<SelectionController>();
        _gameStateController = GetComponent<GameStateController>();

        _unitDatabase = FindObjectOfType<UnitDatabase>();

        _researchModel = FindObjectOfType<ResearchModel>();

        _constructionMenuInfo = new Dictionary<string, UIEvTrigger>();

        _researchMenuInfo = new Dictionary<Technology, UIEvTrigger>();
        _researchMenuActive = false;
        sun = GetComponent<Sun>();

        InitActionEventTypeMappings();
    }

    private void InitActionEventTypeMappings()
    {
        _actionEventTypeMappings = new Dictionary<string, UIEvTrigger>();

        //mappings for actions that trigger UI order or menu select events

        _actionEventTypeMappings.Add("guard", UIEvTrigger.TRIGGER_UIORDER);
        _actionEventTypeMappings.Add("fortify", UIEvTrigger.TRIGGER_UIORDER);
        _actionEventTypeMappings.Add("returnToBase", UIEvTrigger.TRIGGER_UIORDER);
        _actionEventTypeMappings.Add("evacuateCivies", UIEvTrigger.TRIGGER_UIORDER);
        _actionEventTypeMappings.Add("evacuateMainBase", UIEvTrigger.TRIGGER_UIORDER);

        _actionEventTypeMappings.Add("construct", UIEvTrigger.TRIGGER_MENUSELECT);
        _actionEventTypeMappings.Add("buildUnit", UIEvTrigger.TRIGGER_MENUSELECT);
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

    public Dictionary<string, UIEvTrigger> GetConstructionMenuInfo()
    {
        return _constructionMenuInfo;
    }

    /* research menu specific methods */

    public bool IsResearchMenuOpen()
    {
        return _researchMenuActive;
    }

    public Dictionary<Technology, UIEvTrigger> GetResearchMenuInfo()
    {
        return _researchMenuInfo;
    }

    public bool IsTechResearchable(string techId)
    {
        return _gameStateController.IsTechResearchable(techId);
    }

    //refresh the available technologies in the research menu
    public void UpdateResearchMenuInfo()
    {
        Dictionary<Technology, UIEvTrigger> oldResearchMenuInfo = new Dictionary<Technology, UIEvTrigger>(_researchMenuInfo);
        _researchMenuInfo.Clear();
        List<Technology> availableTechs = _researchModel.GetResearchableTechnologies();

        foreach (Technology tech in availableTechs)
        {
            _researchMenuInfo.Add(tech, UIEvTrigger.TRIGGER_RESEARCHTECH);
        }

        if (_researchMenuInfo != oldResearchMenuInfo)
        {
            _researchMenuChanged = true;
        }
    }

    public bool IsResearchMenuUpdated()
    {
        if (_researchMenuChanged)
        {
            _researchMenuChanged = false;
            return true;
        }
        else
        {
            return false;
        }
    }

    /* other helpers */


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

    //return the number of evacuated civies
    public int GetEvacuatedCivs()
    {
        return _gameStateController.GetEvacuatedCivs();
    }

    public List<string> CheckEvents()
    {
        List<string> events = new List<string>();
        if (sun.HeatRises())
        {
            events.Add("Heat Rising");
        }
        else
        {
            events.Add("Nothing to report");
        }
        return events;
    }
    /* event handling */

    public void UpdateAdditionalMenuInfo(string command)
    {

        /*
            get additional display information /alter display information based on command
        */

        /*
        example: If 'construct' command sent, should
          1. Get selected worker unit (if more than 1 unit selected log error)
          2. Get list of supported buildings from worker unit's Construction component
          3. Update constructionMenuInfo with 'construct_<building's unit type>' entries,
             and their corresponding UIEvTrigger (should all be TRIGGER_UIORDER)
        */

        if(command == "construct")
        {
            
            List<GameObject> selectedUnits = _selectionController.GetSelectedUnits();
            Debug.Log("ASD" + selectedUnits.Count);
            if (selectedUnits.Count != 1)
            {
                
                Debug.LogError("Got construction command, but selected unit count != 1. Should not happen.");
                return;
            }

            GameObject selectedUnit = selectedUnits[0];

            UnitInfo selectedUnitInfo = selectedUnit.GetComponent<UnitInfo>();

            if (!selectedUnitInfo.DoesUnitHaveComponent("construction"))
            {
                
               // Debug.LogError("Got construction command, but selected unit does not have construction component. Should not happen.");
                return;
            }

            Construction unitConstructionComp = selectedUnit.GetComponent<Construction>();

            ClearAdditionalInfo();

            foreach (string supportedBuildingType in unitConstructionComp.SupportedBuildingTypes) {
                string supportedBuildingMenuOption = "construct_" + supportedBuildingType;
                _constructionMenuInfo.Add(supportedBuildingMenuOption, UIEvTrigger.TRIGGER_UIORDER);
            }
        }
        /*
        example2: If 'buildUnit' command sent, should...
          1. Get selected barracks/factory unit (if more than 1 selected log error)
          2. Get list of supported units from Unit Builder component
          3. Update constructionMenuInfo with 'build_<unit's type>' entries, 
             and their corresponding UIEvTrigger (should all be TRIGGER_UIORDER)
        */
        else if (command == "buildUnit")
        {
            List<GameObject> selectedUnits = _selectionController.GetSelectedUnits();

            GameObject selectedUnit = selectedUnits[0];
            UnitInfo selectedUnitInfo = selectedUnit.GetComponent<UnitInfo>();
            UnitBuilderComponent unitbuilderComp = selectedUnit.GetComponent<UnitBuilderComponent>();

            ClearAdditionalInfo();

            foreach (string supportedUnitType in unitbuilderComp._supportedUnitTypes)
            {
                string supportedUnitMenuOption = "buildUnit_" + supportedUnitType;
                _constructionMenuInfo.Add(supportedUnitMenuOption, UIEvTrigger.TRIGGER_UIORDER);
            }
        }
        /* case 3: open/close research menu */
        else if (command == "researchMenu")
        {
            //if menu is open, close it
            if(_researchMenuActive)
            {
                _researchMenuInfo.Clear();
                _researchMenuActive = false;
            }
            //else, open the menu and update information
            else
            {
                _researchMenuActive = true;

                UpdateResearchMenuInfo();
            }
        }
 
    }

    public string GetUnitName(string unitType)
    {
        return _unitDatabase.GetUnitName(unitType);
    }

    //clear all additional display information
    public void ClearAdditionalInfo()
    {
        _constructionMenuInfo.Clear();
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

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UserInterfaceController : MonoBehaviour
{
    //prefabs (to be used later)
    public GameObject AbilPrefab;

    //UI elements
    //link all base sections of the UI here
    //information panels
    public GameObject UnitInfo;
    public GameObject ResourceSection;
    //UI buttons
    public GameObject EvacButton;
    public GameObject ResearchButton;
    //action panels
    public GameObject AbilityPanel;
    public GameObject BuildPanel;

    //UI icons
    //list all icons to be used here
    public List<Sprite> AbilityIcons;
    public List<Sprite> BuildIcons;
    public List<Sprite> UnitIcons;
    //default sprite for tabs
    public Sprite def;
    //copied from the selection controller 
    //selections variables 
    private List<GameObject> _selectedUnits;
    private Dictionary<string, UIEvTrigger> _selectedUnitCapabilities;
    private Dictionary<string, UIEvTrigger> _constructDisplay;
    //Unity GameObject 
    public Material tracker;

    //internal controller
    private DisplayInfoController _displayInfoController;
    
    //action panel button lists
    private List<GameObject> _abilityOptions;

    private List<GameObject> _buildOptions;
    


    
    // Start is called before the first frame update
    void Start()
    {
        _displayInfoController = FindObjectOfType<DisplayInfoController>();
        EvacButton.GetComponent<UiAbilties>().setTrigger(("evacuateMainBase", UIEvTrigger.TRIGGER_UIORDER));


        InitButtonLists();

    }

    //initialize all button lists
    private void InitButtonLists()
    {
        _abilityOptions = new List<GameObject>();
        _buildOptions = new List<GameObject>();

        if(AbilityPanel == null)
        {
            Debug.LogError("Failed to assign Ability Panel in UI Controller!");
            return;
        }
        if (BuildPanel == null)
        {
            Debug.LogError("Failed to assign Build Panel in UI Controller!");
            return;
        }

        //get all ability buttons (include inactive ones)
        UiAbilties[] abilityButtons = AbilityPanel.GetComponentsInChildren<UiAbilties>(true);

        foreach(UiAbilties abilityButton in abilityButtons)
        {
            _abilityOptions.Add(abilityButton.gameObject);
        }

        //get all build buttons (include inactive ones)
        UiAbilties[] buildButtons = BuildPanel.GetComponentsInChildren<UiAbilties>(true);

        foreach (UiAbilties buildButton in buildButtons)
        {
            _buildOptions.Add(buildButton.gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Gets a selected unit+ it's actions
        _selectedUnits = _displayInfoController.GetSelectedUnits();
        _selectedUnitCapabilities = _displayInfoController.GetSelectedUnitActions();
        _constructDisplay = _displayInfoController.GetConstructionMenuInfo();
        
        //Updating the resources panel
        List<string> check = new List<string>() { "minerals", "fuel", "research points" };
        Dictionary<string, int> curResources= _displayInfoController.GetPlayerResources(check);
        string resourcePrint = "";
        TextMeshProUGUI mineralText = ResourceSection.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI fuelText = ResourceSection.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI rpText = ResourceSection.transform.GetChild(2).GetComponent<TextMeshProUGUI>();

        mineralText.text = "Minerals: " + curResources["minerals"];
        fuelText.text = "Fuel: " + curResources["fuel"];
        rpText.text = "RP: " + curResources["research points"];

        //civies evacuated
        TextMeshProUGUI civiesText = ResourceSection.transform.GetChild(3).GetComponent<TextMeshProUGUI>();

        civiesText.text = "Civs evacuated: " + _displayInfoController.GetEvacuatedCivs();

        //Displaying selected units 
        if (_selectedUnits.Count > 0)
        {
            
            displayUnit(); //loads the unit info+abilities 
            display_buildOptions(); //Loads all possible building capabilities 
        }
        else
        {
            
            Clear();
        }

    }
    void displayUnit()
    {
        //refresh before repopulating
        ClearAbilities();


        UnitInfo unitInfo = _selectedUnits[0].GetComponent<UnitInfo>();

        //another byproduct of cursed death handling - needing to check if the UnitInfo component exists on an already selected unit
        if (unitInfo == null)
        {
            ClearUnitInformation();
            return;
        }

        //get unit name
        TextMeshProUGUI unitNameComp = UnitInfo.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        string unitName = _displayInfoController.GetUnitName(unitInfo.GetUnitType());
        unitNameComp.text = unitName;

        //get unit health
        TextMeshProUGUI healthTextComp = UnitInfo.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        Health unitHealthComp = _selectedUnits[0].GetComponent<Health>();
        if (unitHealthComp != null)
        {
            healthTextComp.text = unitHealthComp.GetUnitHealth().ToString() + "/" + unitHealthComp.MaxHealth.ToString();
        }
        else
        {
            healthTextComp.text = "";
        }

        //future: other unit-specific statistics?

        //get unit icon
        DisplayUnitIcon();

        //some really primitive attempt to place buttons from left to right
        //ability display. 
        int i = 0;
        
        foreach ( KeyValuePair<string, UIEvTrigger> ability in _selectedUnitCapabilities) 
        {
            _abilityOptions[i].GetComponent<UiAbilties>().setTrigger((ability.Key, ability.Value));
            
            foreach (Sprite sp in AbilityIcons)
            {

                if (sp.name.Equals(ability.Key))
                {

                    _abilityOptions[i].GetComponent<UiAbilties>().Icon= sp;
                 

                    break;
                }
            }
            _abilityOptions[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = ability.Key;

            i++;
            
        }
    }

    private void DisplayUnitIcon()
    {
        string unitName = _displayInfoController.GetUnitName(_selectedUnits[0].GetComponent<UnitInfo>().GetUnitType());
        Image unitIcon = UnitInfo.transform.GetChild(2).GetComponent<Image>();
        unitIcon.enabled = true;
        foreach (Sprite sp in UnitIcons)
        {
            //Debug.Log(sp.name + "," + unitName);
            if (sp.name.Equals(unitName))
            {
                unitIcon.sprite = sp;
            }
        }
        foreach (Sprite sp in BuildIcons)
        {
            //Debug.Log(sp.name + "," + unitName);
            if (sp.name.Equals(unitName))
            {
                unitIcon.sprite = sp;
            }
        }
    }

    private void display_buildOptions()
    {
        //refresh before repopulating
        Clear_buildOptions();

        int i = 0;

        foreach (KeyValuePair<string, UIEvTrigger> ability in _constructDisplay)
        {
            //get command type (construct or buildUnit), unit type, and the name of that unit
            string commandType = ability.Key.Split("_")[0];
            string unitType = ability.Key.Split("_")[1];
            string unitName = _displayInfoController.GetUnitName(unitType);

            UiAbilties buildButton = _buildOptions[i].GetComponent<UiAbilties>();
            TextMeshProUGUI buildName = _buildOptions[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>();


            _buildOptions[i].GetComponent<UiAbilties>().setTrigger((ability.Key, ability.Value));

            List<Sprite> spritesToCheck = new List<Sprite>();
            
            //based on the command type, determine the set of sprites to check for a match
            if (commandType.Equals("construct"))
            {
                spritesToCheck = BuildIcons;
            }
            else if (commandType.Equals("buildUnit"))
            {
                spritesToCheck = UnitIcons;
            }

            foreach (Sprite sp in spritesToCheck)
            {

                //match using unit name
                if (sp.name.Equals(unitName))
                {

                    _buildOptions[i].GetComponent<UiAbilties>().Icon = sp;

                    break;
                }
            }

            //tada
            buildName.text = unitName;

            i++;

        }
    }

    private void Clear()
    {
        ClearUnitInformation();
        ClearAbilities();
        Clear_buildOptions();
    }

    private void ClearUnitInformation()
    {
        //clear unit name
        TextMeshProUGUI unitNameComp = UnitInfo.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        unitNameComp.text = "";
        //clear unit health
        TextMeshProUGUI healthTextComp = UnitInfo.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        healthTextComp.text = "";
        //clear unit icon
        Image unitIcon = UnitInfo.transform.GetChild(2).GetComponent<Image>();
        unitIcon.sprite = null;
        unitIcon.enabled = false;
    }

    private void ClearAbilities()
    {
        for (int i = 0; i < _abilityOptions.Count; i++)
        {
            _abilityOptions[i].GetComponent<UiAbilties>().Icon = def;
            _abilityOptions[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "";
        }
    }

    private void Clear_buildOptions()
    {
        for (int i = 0; i < _buildOptions.Count; i++)
        {
            _buildOptions[i].GetComponent<UiAbilties>().Icon = def;
            _buildOptions[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "";
        }
    }
}

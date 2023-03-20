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
    public GameObject InternalController;
    public GameObject UnitInfo;
    public GameObject AbilPrefab;
    public List<Sprite> AbilityIcons;
    public List<Sprite> BuildIcons;
    public List<Sprite> UnitIcons;
    //copied from the selection controller 
    //selections variables 
    private List<GameObject> _selectedUnits;
    private Dictionary<string, UIEvTrigger> _selectedUnitCapabilities;
    private Dictionary<string, UIEvTrigger> _constructDisplay;
    //Unity GameObject 
    private CapabilityController _capabilityController;
    public Material tracker;
    private DisplayInfoController  component;
    //Icons list
    public List<GameObject> buttonlist;
    public GameObject resourceText;
    public List<GameObject> BuildOptions;
    
    public Sprite def;//default sprite for tabs
    public GameObject Evac_button;
    
    // Start is called before the first frame update
    void Start()
    {
        InternalController = GameObject.Find("InternalController");
        component= InternalController.GetComponent<DisplayInfoController>();
        Evac_button.GetComponent<UiAbilties>().setTrigger(("evacuateMainBase", UIEvTrigger.TRIGGER_UIORDER));
        



    }

    // Update is called once per frame
    void Update()
    {
        // Gets a selected unit+ it's actions
        _selectedUnits = component.GetSelectedUnits();
        _selectedUnitCapabilities = component.GetSelectedUnitActions();
        _constructDisplay = component.GetConstructionMenuInfo();
        
        //Updating the resources panel
        List<string> check = new List<string>() { "minerals", "fuel", "research points" };
        Dictionary<string, int> curResources= component.GetPlayerResources(check);
        string resourcePrint = "";
        TextMeshProUGUI mineralText = resourceText.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI fuelText = resourceText.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI rpText = resourceText.transform.GetChild(2).GetComponent<TextMeshProUGUI>();

        mineralText.text = "Minerals: " + curResources["minerals"];
        fuelText.text = "Fuel: " + curResources["fuel"];
        rpText.text = "RP: " + curResources["research points"];

        //civies evacuated
        TextMeshProUGUI civiesText = resourceText.transform.GetChild(3).GetComponent<TextMeshProUGUI>();

        civiesText.text = "Civs evacuated: " + component.GetEvacuatedCivs();

        //Displaying selected units 
        if (_selectedUnits.Count > 0)
        {
            
            displayUnit(); //loads the unit info+abilities 
            displayBuildOptions(); //Loads all possible building capabilities 
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


        //get unit name
        TextMeshProUGUI unitNameComp = UnitInfo.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        string unitName = component.GetUnitName(_selectedUnits[0].GetComponent<UnitInfo>().GetUnitType());
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
            buttonlist[i].GetComponent<UiAbilties>().setTrigger((ability.Key, ability.Value));
            
            foreach (Sprite sp in AbilityIcons)
            {

                if (sp.name.Equals(ability.Key))
                {

                    buttonlist[i].GetComponent<UiAbilties>().Icon= sp;
                 

                    break;
                }
            }
            buttonlist[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = ability.Key;

            i++;
            
        }
    }

    private void DisplayUnitIcon()
    {
        string unitName = component.GetUnitName(_selectedUnits[0].GetComponent<UnitInfo>().GetUnitType());
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

    private void displayBuildOptions()
    {
        //refresh before repopulating
        ClearBuildOptions();

        int i = 0;

        foreach (KeyValuePair<string, UIEvTrigger> ability in _constructDisplay)
        {
            //get command type (construct or buildUnit), unit type, and the name of that unit
            string commandType = ability.Key.Split("_")[0];
            string unitType = ability.Key.Split("_")[1];
            string unitName = component.GetUnitName(unitType);

            UiAbilties buildButton = BuildOptions[i].GetComponent<UiAbilties>();
            TextMeshProUGUI buildName = BuildOptions[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>();


            BuildOptions[i].GetComponent<UiAbilties>().setTrigger((ability.Key, ability.Value));

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

                    BuildOptions[i].GetComponent<UiAbilties>().Icon = sp;

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
        ClearBuildOptions();
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
        for (int i = 0; i < buttonlist.Count; i++)
        {
            buttonlist[i].GetComponent<UiAbilties>().Icon = def;
            buttonlist[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "";
        }
    }

    private void ClearBuildOptions()
    {
        for (int i = 0; i < BuildOptions.Count; i++)
        {
            BuildOptions[i].GetComponent<UiAbilties>().Icon = def;
            BuildOptions[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "";
        }
    }
}

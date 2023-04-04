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
    public GameObject UnitInfoPrefab;
    public GameObject UnitInfoCanvas;
    public GameObject ResourceSection;
    //UI buttons
    public GameObject EvacButton;
    public GameObject ResearchButton;
    public GameObject init_ScienceButton;
    //action panels
    public GameObject AbilityPanel;
    public GameObject BuildPanel;
    public GameObject SciencePanel;
   

    //UI icons
    //list all icons to be used here
    public List<Sprite> AbilityIcons;
    public List<Sprite> BuildIcons;
    public List<Sprite> UnitIcons;
    public List<Sprite> ResearchIcons;
    //default sprite for tabs
    public Sprite def;
    //copied from the selection controller 
    //selections variables 
    public List<GameObject> _selectedUnits;
    public List<GameObject> _OldselectedUnits;
    private bool HaveNewUnitsBeenSelected=false;
    private Dictionary<string, UIEvTrigger> _selectedUnitCapabilities;
    private Dictionary<string, UIEvTrigger> _constructDisplay;
    private Dictionary<Technology, UIEvTrigger> _researchDisplay;
    //Unity GameObject 
    public Material tracker;

    //internal controller
    private DisplayInfoController _displayInfoController;
    
    //action panel button lists
    private List<GameObject> _abilityOptions;

    private List<GameObject> _buildOptions;

    private List<GameObject> _researchOptions;

    //UI-obstructed regions
    [Tooltip("Include regions of the UI that (at least partially) obstruct the game.")]
    public List<GameObject> ObstructingUIRegions;
    //the boundaries of these obstructing UI regions
    private Dictionary<GameObject, Vector4> _obstructingUIRegionBoundaries;


    
    // Start is called before the first frame update
    void Start()
    {
        _displayInfoController = FindObjectOfType<DisplayInfoController>();
        EvacButton.GetComponent<UiAbilties>().setTrigger(("evacuateMainBase", UIEvTrigger.TRIGGER_UIORDER));


        InitButtonLists();

        CalculateUIObstructingRegionBoundaries();
    }

    //initialize all button lists
    private void InitButtonLists()
    {
        _abilityOptions = new List<GameObject>();
        _buildOptions = new List<GameObject>();
        _researchOptions = new List<GameObject>();

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

    private void CalculateUIObstructingRegionBoundaries()
    {
        _obstructingUIRegionBoundaries = new Dictionary<GameObject, Vector4>();

        foreach(GameObject obstructingElement in ObstructingUIRegions)
        {
            //determine the x,y size as percentage of screen

            Vector3[] elementBoundaries = new Vector3[4]; 
                
            obstructingElement.GetComponent<RectTransform>().GetWorldCorners(elementBoundaries);

            //width is x coord of corner 3 (bottom right) - x coord of corner 1 (top left)
            //height is same idea

            float width = elementBoundaries[2].x - elementBoundaries[0].x;
            float height = elementBoundaries[2].y - elementBoundaries[0].y;

            float widthPct = width / Screen.width;
            float heightPct = height / Screen.height;

            //determine x,y pos as percentage of screen

            //take top left corner
            float xPct = elementBoundaries[0].x / Screen.width;
            float yPct = elementBoundaries[0].y / Screen.height;

            //calculate the bounding box
            //invert y-axis because unity
            Vector4 boundingBox = new Vector4(xPct, 1 - (yPct + heightPct), xPct + widthPct,1 - yPct);

            Debug.Log("Obstructing UI region for " + obstructingElement.name + ": " + boundingBox);

            //add to list of obstructing UI regions
            _obstructingUIRegionBoundaries.Add(obstructingElement, boundingBox);
        }

        Debug.Log("Successfully initialized bounding regions of UI, to be used by Mouse Controller");
        Debug.Log("Number of obstructing regions: " + _obstructingUIRegionBoundaries.Count);
    }

    public List<Vector4> GetUIObstructingRegions()
    {
        List<Vector4> boundingRegions = new List<Vector4>();

        foreach(GameObject obstructingElement in ObstructingUIRegions)
        {
            //skip currently hidden regions of UI
            if (!obstructingElement.activeInHierarchy)
            {
                continue;
            }

            boundingRegions.Add(_obstructingUIRegionBoundaries[obstructingElement]);
        }

        return boundingRegions;
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
            if (HaveUnitOptionsChanged())
            {
                displayUnit(); //loads the unit info+abilities 
                display_buildOptions(); //Loads all possible building capabilities 
            }
        }
        else
        {
            Clear();
        }
        if (_displayInfoController.IsResearchMenuOpen())
        {
            display_ResearchOptions();
        }


    }
    void displayUnit()
    {
        //refresh before repopulating
        //

        if (_selectedUnits.Count ==1 )
        {
            ClearAbilities();
            ClearUnitInformation();
            UnitInfo.SetActive(true);

            UnitInfo unitInfo = _selectedUnits[0].GetComponent<UnitInfo>();

            //another byproduct of cursed death handling - needing to check if the UnitInfo component exists on an already selected unit
            if (unitInfo == null)
            {
                ClearUnitInformation();
              HaveNewUnitsBeenSelected = false;
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
            DisplayUnitIconSingle(0);

            //some really primitive attempt to place buttons from left to right
            //ability display. 
            int i = 0;

            foreach (KeyValuePair<string, UIEvTrigger> ability in _selectedUnitCapabilities)
            {
                _abilityOptions[i].GetComponent<UiAbilties>().setTrigger((ability.Key, ability.Value));

                foreach (Sprite sp in AbilityIcons)
                {

                    if (sp.name.Equals(ability.Key))
                    {

                        _abilityOptions[i].GetComponent<UiAbilties>().setIcon( sp);


                        break;
                    }
                }
                _abilityOptions[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = ability.Key;

                i++;


            }
            HaveNewUnitsBeenSelected = true;
        }
        else if (_selectedUnits.Count > 1 )
        {
            UnitInfo.SetActive(false);
            ClearAbilities();
            ClearUnitInformation();
            for (int x = 0;x < _selectedUnits.Count; x++)  
            {
                
                GameObject unit = Instantiate(UnitInfoPrefab);
                unit.transform.localScale = new Vector3(.5f, .5f, .5f);
                if (x <= 2)
                {
                    unit.transform.position = new Vector3(-691f + (500 * x), 268f, 0f);
                }
                else if (x <=5)
                {
                    unit.transform.position = new Vector3(-691f + (500 * (x-3)), -61f, 0f);
                }
                else 
                {
                    unit.transform.position = new Vector3(-691f + (500 * (x-6)), -378f, 0f);
                }
                unit.transform.SetParent(UnitInfoCanvas.transform, false);
                UnitInfo unitInfo = _selectedUnits[x].GetComponent<UnitInfo>();

                //another byproduct of cursed death handling - needing to check if the UnitInfo component exists on an already selected unit
                
                if (unit == null)
                {
                    ClearUnitInformation();
                    return;
                }

                //get unit name
                TextMeshProUGUI unitNameComp = unit.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
                string unitName = _displayInfoController.GetUnitName(unitInfo.GetUnitType());
                unitNameComp.text = unitName;

                //get unit health
                TextMeshProUGUI healthTextComp = unit.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
                Health unitHealthComp = _selectedUnits[x].GetComponent<Health>();
                if (unitHealthComp != null)
                {
                    healthTextComp.text = unitHealthComp.GetUnitHealth().ToString() + "/" + unitHealthComp.MaxHealth.ToString();
                }
                else
                {
                    healthTextComp.text = "";
                }
                unit.SetActive(true);
                //future: other unit-specific statistics?

                //get unit icon
                DisplayUnitIcon(unit,x);

                //some really primitive attempt to place buttons from left to right
                //ability display. 
                int i = 0;

                foreach (KeyValuePair<string, UIEvTrigger> ability in _selectedUnitCapabilities)
                {
                    _abilityOptions[i].GetComponent<UiAbilties>().setTrigger((ability.Key, ability.Value));

                    foreach (Sprite sp in AbilityIcons)
                    {

                        if (sp.name.Equals(ability.Key))
                        {

                            _abilityOptions[i].GetComponent<UiAbilties>().Icon = sp;


                            break;
                        }
                    }
                    _abilityOptions[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = ability.Key;

                    i++;


                }
            }
            HaveNewUnitsBeenSelected = true;
        }

           
    }

    private void DisplayUnitIcon(GameObject instance, int place)
    {
        string unitName = _displayInfoController.GetUnitName(_selectedUnits[place].GetComponent<UnitInfo>().GetUnitType());
        Image unitIcon = instance.transform.GetChild(2).GetComponent<Image>();
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
    private void DisplayUnitIconSingle( int place)
    {
        string unitName = _displayInfoController.GetUnitName(_selectedUnits[place].GetComponent<UnitInfo>().GetUnitType());
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

                    _buildOptions[i].GetComponent<UiAbilties>().setIcon(sp);

                    break;
                }
            }

            //tada
            buildName.text = unitName;

            i++;

        }
    }
    private void display_ResearchOptions()
    {
        //check if research menu options changed
        Dictionary<Technology, UIEvTrigger> newResearchInfo = _displayInfoController.GetResearchMenuInfo();
        //use screen height to keep scaling proper
        int pos = Screen.height/12;
        int i = 0;
        init_ScienceButton.SetActive(false);
        //update if menu is empty or data has changed
        if (_researchOptions.Count == 0 || _displayInfoController.IsResearchMenuUpdated())
        {
            //clear old menu elements
            for (int x = 2; x < SciencePanel.transform.childCount; x++)
            {
                Destroy(SciencePanel.transform.GetChild(x).gameObject);
            }
            _researchOptions.Clear();

            //store new menu info
            _researchDisplay = newResearchInfo;
            //dynamically initialize research buttons
            foreach (KeyValuePair<Technology, UIEvTrigger> science in _researchDisplay)
            {

                GameObject button = Instantiate(init_ScienceButton, new Vector3(0,0,0), Quaternion.identity);
                button.name = science.Key.Name;
                //display the name
                button.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = science.Key.Name;
                //display the cost
                button.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Cost: " + science.Key.Cost;
                //adjustments
                button.transform.SetParent(SciencePanel.transform, false);
                //can clean this up a bit later
                if (i < 5)
                {
                    button.transform.Translate(-Screen.width / 16, Screen.height / 4.5f - pos * i, 0);
                }
                else if (i < 10)
                {
                    button.transform.Translate(0, Screen.height / 4.5f - pos * (i-5), 0);
                }
                else if (i < 15)
                {
                    button.transform.Translate(Screen.width / 16, Screen.height / 4.5f - pos * (i - 10), 0);
                }
                //configure event trigger
                button.GetComponent<UiAbilties>().setTrigger((science.Key.Id, science.Value));
                //you get the idea (todo get tech icons)
                button.GetComponent<UiAbilties>().setIcon(def);
                button.SetActive(true);
                _researchOptions.Add(button);
                i++;


            }
        }
        
        
    }
    private void Clear()
    {
        _OldselectedUnits = new List<GameObject>(_selectedUnits);
        ClearUnitInformation();
        ClearAbilities();
        Clear_buildOptions();
      HaveNewUnitsBeenSelected = false;
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
        if (UnitInfoCanvas.transform.childCount > 1)
        {
            for (int i=1;i<UnitInfoCanvas.transform.childCount; i++)
            {
                Destroy(UnitInfoCanvas.transform.GetChild(i).gameObject);
            }
        }
    }

    private void ClearAbilities()
    {
        for (int i = 0; i < _abilityOptions.Count; i++)
        {
            _abilityOptions[i].GetComponent<UiAbilties>().setIcon( def);
            _abilityOptions[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "";
        }
    }

    private void Clear_buildOptions()
    {
        for (int i = 0; i < _buildOptions.Count; i++)
        {
            _buildOptions[i].GetComponent<UiAbilties>().setIcon( def);
            _buildOptions[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "";
        }
    }
    public void EnableDisableScience()
    {
        _displayInfoController.UpdateAdditionalMenuInfo("researchMenu");
        if (SciencePanel.activeSelf)
        {
            SciencePanel.SetActive(false);

            for (int i = 2; i < SciencePanel.transform.childCount; i++)
            {
                Destroy(SciencePanel.transform.GetChild(i).gameObject);
            }
            _researchOptions.Clear();
        }
        else SciencePanel.SetActive(true);

    }
    private bool HaveUnitOptionsChanged()
    {
        if (_OldselectedUnits.Equals(_selectedUnits))
        {
            return false;
        }
        else
        {
            return true;
        }
    }
    public List<GameObject> GetMiniMap()
    {
        return _displayInfoController.MiniMap();
    }
}

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
    //copied from the selection controller 
    //selections variables 
    private List<GameObject> _selectedUnits;
    private Dictionary<string, UIEvTrigger> _selectedUnitCapabilities;
    private Dictionary<string, UIEvTrigger> _constructDisplay;
    //Unity GameObject 
    private CapabilityController _capabilityController;
    public Material tracker;
    private DisplayInfoController  component;
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
        _constructDisplay = component.GetAdditionalMenuInfo();
        
        //Updating the resources panel
        List<string> check = new List<string>() { "minerals", "fuel" };
        Dictionary<string, int> curResources= component.GetPlayerResources(check);
        string resourcePrint = "";
        foreach (KeyValuePair<string,int> res in curResources)
        {
            resourcePrint+= res.Value.ToString()+"x"+res.Key+"  ";
        }
        resourceText.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = resourcePrint;
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
           
            //Debug.Log(Unitblock.transform.GetChild(0).transform.GetChild(0).name);
            UnitInfo.transform.GetChild(0).transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = _selectedUnits[0].GetComponent<UnitInfo>().UnitType;
            UnitInfo.transform.GetChild(0).transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = _selectedUnits[0].GetComponent<Health>()._actualHealth.ToString()+ "/"+ _selectedUnits[0].GetComponent<Health>().MaxHealth.ToString(); //need a way to get health 
             UnitInfo.transform.GetChild(0).GetComponent<RawImage>().texture = component.GetUnitIcon(_selectedUnits[0].GetComponent<UnitInfo>().UnitType);
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
            

            i++;
            
        }
    }
    private void displayBuildOptions()
    {
        int i = 0;

        foreach (KeyValuePair<string, UIEvTrigger> ability in _constructDisplay)
        {

            BuildOptions[i].GetComponent<UiAbilties>().setTrigger((ability.Key, ability.Value));
            
            foreach (Sprite sp in BuildIcons)
            {

                if (sp.name.Equals(ability.Key))
                {
                    
                    BuildOptions[i].GetComponent<UiAbilties>().Icon = sp;
                    
                    break;
                }
            }


            i++;

        }
    }
    private void Clear()
    {
        for (int i = 0; i < buttonlist.Count; i++)
        {
            buttonlist[i].GetComponent<UiAbilties>().Icon= def;
        }
        for (int i = 0; i < BuildOptions.Count; i++)
        {
            BuildOptions[i].GetComponent<UiAbilties>().Icon=def;
        }
    }
}

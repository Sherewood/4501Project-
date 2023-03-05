using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UserInterfaceController : MonoBehaviour
{
    public GameObject InternalController;
    public GameObject UnitInfo;
    public GameObject AbilPrefab;
    //copied from the selection controller 

    private List<GameObject> _selectedUnits;
    private Dictionary<string, UIEvTrigger> _selectedUnitCapabilities;
    private CapabilityController _capabilityController;
    public Material tracker;
    private DisplayInfoController  component;

    // Start is called before the first frame update
    void Start()
    {
        InternalController = GameObject.Find("InternalController");
        component= InternalController.GetComponent<DisplayInfoController>();

    }

    // Update is called once per frame
    void Update()
    {
        // clearing function above
        _selectedUnits = component.GetSelectedUnits();
        _selectedUnitCapabilities = component.GetSelectedUnitActions();

        if (_selectedUnits.Count >0) displayUnit();


    }
    void displayUnit()
    {
           
            //Debug.Log(Unitblock.transform.GetChild(0).transform.GetChild(0).name);
            UnitInfo.transform.GetChild(0).transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = _selectedUnits[0].GetComponent<UnitInfo>().UnitType;
            UnitInfo.transform.GetChild(0).transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = _selectedUnits[0].GetComponent<Health>()._actualHealth.ToString()+ "/"+ _selectedUnits[0].GetComponent<Health>().MaxHealth.ToString(); //need a way to get health 
             UnitInfo.transform.GetChild(0).GetComponent<RawImage>().texture = component.GetUnitIcon(_selectedUnits[0].GetComponent<UnitInfo>().UnitType);
        //some really primitive attempt to place buttons from left to right
        //ability display. 
        int i = -20;
        foreach (var ability in _selectedUnitCapabilities) 
        {
            GameObject abl = Instantiate(AbilPrefab);
            abl.GetComponent<UiAbilties>().setTrigger((ability.Key, ability.Value));
            abl.transform.position = new Vector3(i, 0, 0);
            i += 10;
        }
    }
}

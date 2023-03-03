using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserInterfaceController : MonoBehaviour
{
    public GameObject InternalController;
    public GameObject UnitInfo;
    //copied from the selection controller 

    private List<GameObject> _selectedUnits;
    private List<Capability> _selectedUnitCapabilities;
    private CapabilityController _capabilityController;
    public Material tracker;

    // Start is called before the first frame update
    void Start()
    {
        InternalController = GameObject.Find("InternalController");
    }

    // Update is called once per frame
    void Update()
    {
        // clearing function above
        _selectedUnits = InternalController.GetComponent<SelectionController>().GetSelectedUnits();

        if (_selectedUnits.Count >0) displayUnit();


    }
    void displayUnit()
    {
        //commented stuff in regards to creating instances of unit blocks and placing them within the selection canvas 
     //   for (int i = 0; i < _selectedUnits.Count; i++)
      //  {
           
            //Debug.Log(Unitblock.transform.GetChild(0).transform.GetChild(0).name);
            UnitInfo.transform.GetChild(0).transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = _selectedUnits[0].GetComponent<UnitInfo>().UnitType;
           
            UnitInfo.transform.GetChild(0).transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = _selectedUnits[0].GetComponent<Health>()._actualHealth.ToString()+ "/"+ _selectedUnits[0].GetComponent<Health>().MaxHealth.ToString(); //need a way to get health 
                                                                                                                                                                                                                                               //Unitblock.transform.position = new Vector3(0 + i, 0,0);*/
                                                                                                                                                                                                                                               //     }
        UnitInfo.transform.GetChild(0).GetComponent<RawImage>().texture = _selectedUnits[0].GetComponent<UnitInfo>().UnitPic;
    }
}

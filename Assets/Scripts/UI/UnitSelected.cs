using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSelected : MonoBehaviour
{
    private List<GameObject> _selectedUnits;
    
    // Start is called before the first frame update
    void Start()
    {
        _selectedUnits.Equals(GameObject.Find("InternalController").GetComponent<SelectionController>().GetSelectedUnits());
    }

    // Update is called once per frame
    void Update()
    {
        _selectedUnits.Equals(GameObject.Find("InternalController").GetComponent<SelectionController>().GetSelectedUnits());


    }
    void contructPanel()
    {
        GameObject.Find("UnitName").GetComponent<TMPro.TextMeshProUGUI>().text = _selectedUnits[0].name;
    }
}
 
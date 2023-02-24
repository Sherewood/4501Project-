using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/* Internal Controller class */
//Purpose: Handle events related to the selection of units and buildings

public class SelectionController : MonoBehaviour
{

    //list of selected units
    private List<GameObject> _selectedUnits;

    //list of supported actions for the selected units
    private List<Capability> _selectedUnitCapabilities;

    private CapabilityController _capabilityController;

    private EntityStorage _entityStorage;

    void Start()
    {
        _selectedUnits = new List<GameObject>();
        _selectedUnitCapabilities = new List<Capability>();

        _capabilityController = GetComponent<CapabilityController>();
        _entityStorage = FindObjectOfType<EntityStorage>();
    }

    //Handle single unit selection
    public void HandleSingleSelection(RaycastHit selection)
    {
        //get the hit GameObject
        GameObject selectedEntity = selection.transform.gameObject;

        //if GameObject not found in entity storage, then the GameObject does not correspond to a unit.
        if (_entityStorage.FindEntity(selectedEntity.GetInstanceID()) == null)
        {
            Debug.Log("Selection target is not a unit - No selection made");
            return;
        }

        //clear old selected unit data
        _selectedUnits.Clear();
        _selectedUnitCapabilities.Clear();

        _selectedUnits.Add(selectedEntity);
        _selectedUnitCapabilities = _capabilityController.GetCapabilitiesOfUnit(selectedEntity);

        Debug.Log("New selection made - unit with instance id " + selectedEntity.GetInstanceID());
        Debug.Log("Selected unit has " + _selectedUnitCapabilities.Count + " capabilities. Too lazy to list them out right now lmao.");
    }

    //TODO: Handle area selection (use .point parameter of raycast hit)
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/* Internal Controller class */
//Purpose: Handle events related to the selection of units and buildings

public class SelectionController : MonoBehaviour
{

    [Tooltip("The prefab used to represent a selection ring.")]
    public SelectionIndicator SelectionIndicatorPrefab;

    //list of selected units
    private List<GameObject> _selectedUnits;

    //list of supported actions for the selected units
    private List<Capability> _selectedUnitCapabilities;

    private CapabilityController _capabilityController;

    private EntityStorage _entityStorage;

    //all currently instantiated selection indicators
    private List<SelectionIndicator> _selectionIndicators;

    void Start()
    {
        _selectedUnits = new List<GameObject>();
        _selectedUnitCapabilities = new List<Capability>();
        _selectionIndicators = new List<SelectionIndicator>();

        _capabilityController = GetComponent<CapabilityController>();
        _entityStorage = FindObjectOfType<EntityStorage>();
    }

    void Update()
    {
        //check if any selected units have been destroyed, and clear them if they have
        CheckIfSelectedUnitsGone();
    }

    //Handle single unit selection
    public void HandleSingleSelection(RaycastHit selection)
    {
        //get the hit GameObject
        GameObject selectedEntity = selection.transform.gameObject;

        if (selectedEntity == null)
        {
            Debug.Log("Selected unit destroyed inbetween selection and selection controller handling, ignoring");
            ClearOldSelectionData(true);
            return;
        }

        //if GameObject not found in entity storage, then the GameObject does not correspond to a unit.
        if (_entityStorage.FindEntity(selectedEntity.GetInstanceID()) == null)
        {
            Debug.Log("Selection target is not a unit - No selection made");
            ClearOldSelectionData(true);
            return;
        }

        //clear old selected unit data
        ClearOldSelectionData(true);

        _selectedUnits.Add(selectedEntity);
        _selectedUnitCapabilities = _capabilityController.GetCapabilitiesOfUnit(selectedEntity);
        AddNewSelectionIndicator(selectedEntity);

        Debug.Log("New selection made - unit with instance id " + selectedEntity.GetInstanceID());
        Debug.Log("Selected unit has " + _selectedUnitCapabilities.Count + " capabilities. Too lazy to list them out right now lmao.");
    }

    //TODO: Handle area selection (use .point parameter of raycast hit)

    //helpers

    public List<GameObject> GetSelectedUnits()
    {
        //check if any selected units died before delivering info
        CheckIfSelectedUnitsGone();
        return _selectedUnits;
    }

    public List<Capability> GetSelectedUnitCapabilities()
    {
        return _selectedUnitCapabilities;
    }

    private void ClearOldSelectionData(bool eraseUnits)
    {
        if (eraseUnits)
        {
            _selectedUnits.Clear();
        }
        _selectedUnitCapabilities.Clear();
        ClearSelectionIndicators();
    }

    private void CheckIfSelectedUnitsGone()
    {
        bool unitDestroyed = false;
        //remove any selected units that were destroyed
        for(int i = 0; i < _selectedUnits.Count; i++)
        {
            if (_selectedUnits[i] == null)
            {
                _selectedUnits.RemoveAt(i);
                unitDestroyed = true;
                i--;
            }
        }

        //if a unit was destroyed, clear the capability data/indicators and re-create them
        if (unitDestroyed)
        {
            ClearOldSelectionData(false);
            _selectedUnitCapabilities = _capabilityController.GetCapabilitiesOfUnits(_selectedUnits);
            foreach (GameObject unit in _selectedUnits)
            {
                AddNewSelectionIndicator(unit);
            }
        }
    }

    //create a new selection indicator mapped to a given target object
    private void AddNewSelectionIndicator(GameObject target)
    {
        SelectionIndicator newIndicator = Instantiate(SelectionIndicatorPrefab);

        newIndicator.SetTarget(target);

        _selectionIndicators.Add(newIndicator);
    }

    //clear out old selection indicators
    private void ClearSelectionIndicators()
    {
        foreach(SelectionIndicator indicator in _selectionIndicators)
        {
            Destroy(indicator.gameObject);
        }

        _selectionIndicators.Clear();
    }
}

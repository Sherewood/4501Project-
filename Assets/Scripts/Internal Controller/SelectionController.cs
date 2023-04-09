using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/* Internal Controller class */
//Purpose: Handle events related to the selection of units and buildings

public class SelectionController : MonoBehaviour
{

    [Tooltip("The prefab used to represent a selection ring.")]
    public SelectionIndicator SelectionIndicatorPrefab;

    [Tooltip("The prefab used to represent a target indicator.")]
    public SelectionIndicator EnemyTargetPrefab;

    //list of selected units
    private List<GameObject> _selectedUnits;

    //list of supported actions for the selected units
    private List<Capability> _selectedUnitCapabilities;

    private CapabilityController _capabilityController;

    private EntityStorage _entityStorage;

    //all currently instantiated selection indicators
    private List<SelectionIndicator> _selectionIndicators;

    //indicator for currently targeted enemy
    //todo: figure out way to properly track all enemies targeted at a time, so we can have multiple indicators? might not want tbh
    private SelectionIndicator _targetIndicator;

    void Start()
    {
        _selectedUnits = new List<GameObject>();
        _selectedUnitCapabilities = new List<Capability>();
        _selectionIndicators = new List<SelectionIndicator>();
        _targetIndicator = null;

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


    //handle area selection
    public void HandleAreaSelection(RaycastHit initialSelection, RaycastHit finalSelection)
    {

        List<GameObject> newSelectedEntities = _entityStorage.FindEntitiesInRange(initialSelection.point, finalSelection.point);

        Debug.Log("Selection Controller - Area Selection selected " + newSelectedEntities.Count + " entities.");

        //todo: apply filters to selection (ex: if 1+ player unit selected, ignore enemy units)


        //clear old selected unit data
        ClearOldSelectionData(true);

        //track all newly selected units
        foreach(GameObject selectedEntity in newSelectedEntities)
        {
            _selectedUnits.Add(selectedEntity);
            AddNewSelectionIndicator(selectedEntity);
        }

        //get capabilities of selected units
        //can cause an overflow in UI Controller since it only has 6 hardcoded ability slots
        _selectedUnitCapabilities = _capabilityController.GetCapabilitiesOfUnits(newSelectedEntities);
    }

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
            if (_selectedUnits[i] == null || _selectedUnits[i].GetComponent<UnitState>().IsDead())
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

        //set color of indicator depending on target's allegiance
        string targetAllegiance = target.GetComponent<UnitInfo>().GetAllegiance();

        Vector3 color = new Vector3();

        switch (targetAllegiance)
        {
            case "player":
                color = new Vector3(0.0f, 1.0f, 0.0f);
                break;
            case "neutral":
                color = new Vector3(0.8f, 0.8f, 0.8f);
                break;
            case "enemy":
                color = new Vector3(1.0f, 0.0f, 0.0f);
                break;
        }

        newIndicator.SetColor(color);

        //keep track of indicator
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

    //set enemy target indicator
    public void SetTargetIndicator(GameObject target)
    {
        if(_targetIndicator != null)
        {
            Destroy(_targetIndicator.gameObject);
        }

        SelectionIndicator targetIndicator = Instantiate(EnemyTargetPrefab);

        targetIndicator.SetTarget(target);

        _targetIndicator = targetIndicator;
    }
}

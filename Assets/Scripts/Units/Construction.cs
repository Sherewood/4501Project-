using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/* Unit Component */
//Purpose: Handle the construction of buildings at certain locations.

public class Construction : MonoBehaviour
{

    /* Configuration */

    //the building types that this unit can build
    public List<string> SupportedBuildingTypes;

    /* private vars */

    //the currently selected building type
    private string _currentBuildingType;

    private UnitState _unitState;

    //note - unit spawner should have 0 repositioning attempts, if the building cannot be built at specified location
    //then it shouldn't be built at all
    private UnitSpawner _unitSpawner;

    void Awake()
    {
        _currentBuildingType = "";

        _unitState = GetComponent<UnitState>();

        _unitSpawner = GetComponent<UnitSpawner>();
    }

    //set the type of building that the construction component is slated to build.
    public void SetCurrentBuilding(string buildingType)
    {
        _currentBuildingType = buildingType;
    }

    //erase the selected building type
    //determining when this should be called is probably going to be messy..
    public void ClearBuildingSelection()
    {
        _currentBuildingType = "";
    }

    //handle reporting of destination being reached by movement component
    public void HandleDestinationReached()
    {
        //check if unit state is "moving to construct"
        if (_unitState.GetState() == UState.STATE_MOVING_TO_CONSTRUCT)
        {
            if(_currentBuildingType == "")
            {
                Debug.LogError("Construction component ordered to construct building, but lacks information on what it should build");
                return;
            }

            ConstructBuilding();

            _unitState.SetState(UState.STATE_IDLE);
        }
    }

    private void ConstructBuilding()
    {
        //try and construct the building
        GameObject newBuilding = _unitSpawner.SpawnUnit(_currentBuildingType);

        //if not null, building was successfully constructed, erase previously stored building type
        if (newBuilding != null)
        {
            _currentBuildingType = "";
        }
    }
}

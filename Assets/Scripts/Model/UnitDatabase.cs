using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
/* Model class */
//Purpose: Stores relevant unit types and their associated information (costs, supported components, prefabs, icons)

public class UnitDatabase : MonoBehaviour
{
    //holds data for the unit such as costs + supported components
    private Dictionary<string, Dictionary<string, string>> _unitData;

    //the following should be in a dictionary, but unity inspector will not display that, so separated into lists instead. yay.
    //keys (set in inspector)
    //index of the key in this array should match index of prefab and icon
    public List<string> UnitTypes;
    //prefabs (set in inspector)
    public List<GameObject> UnitAssetPrefabs;
    //icons (set in inspector)
    //if unit doesn't have an icon for whatever reason, use a default image of sorts
    public List<Texture2D> UnitAssetIcons;

    void Awake()
    {
        Debug.Log("Initializing unit database");

        _unitData = new Dictionary<string, Dictionary<string, string>>();

        //create unit records
        CreateUnitDataRecord("neutral-test", 0, 0, "unitInfo unitState movement health targeting");
        CreateUnitDataRecord("player-dynamic-test", 0, 0, "unitInfo unitState movement attack weapon health targeting");
        CreateUnitDataRecord("neutral-test-spawner", 0, 0, "unitInfo unitState unitSpawner");

        //worker
        CreateUnitDataRecord("player-dynamic-worker", 0, 0, "unitInfo unitState movement health harvester");

        //resource deposits
        CreateUnitDataRecord("neutral-static-mineraldep", 0, 0, "unitInfo unitState resource");
        CreateUnitDataRecord("neutral-static-fueldep", 0, 0, "unitInfo unitState resource");

        //buildings
        CreateUnitDataRecord("player-static-civilianbuilding", 0, 0, "unitInfo unitState health civilian unitSpawner");

    }

    //fills a record in _unitData with the specified information
    private void CreateUnitDataRecord(string unitType, int mineralCost, int fuelCost, string supportedComponents)
    {
        Dictionary<string, string> unitRecord = new Dictionary<string, string>();
        unitRecord.Add("minerals", mineralCost.ToString());
        unitRecord.Add("fuel", fuelCost.ToString());
        //format will be <component1> <component2> ... <component_n>, separated by spaces
        unitRecord.Add("components", supportedComponents);

        _unitData.Add(unitType, unitRecord);
    }

    public int GetUnitCost(string unitType, string resourceType)
    {
        int cost = -1;
        if (_unitData.ContainsKey(unitType) && _unitData[unitType].ContainsKey(resourceType))
        {
            cost = Int32.Parse(_unitData[unitType][resourceType]);
        }
        return cost;
    }

    public bool DoesUnitHaveComponent(string unitType, string component)
    {
        return _unitData.ContainsKey(unitType) && _unitData[unitType]["components"].Contains(component);
    }

    //returns list of components for a specified unit type
    public List<string> GetComponentsForUnitType(string unitType)
    {
        if (!_unitData.ContainsKey(unitType))
        {
            Debug.LogError("Unit Database queried for unit type " + unitType + " that does not exist!");
            return new List<string>();
        }

        string componentsString = _unitData[unitType]["components"];

        return new List<string>(componentsString.Split(" "));
    }

    public GameObject GetUnitPrefab(string type)
    {
        GameObject unitPrefab = null;
        int typeIndex = UnitTypes.IndexOf(type);

        if(typeIndex != -1)
        {
            unitPrefab = UnitAssetPrefabs[typeIndex];
        }

        return unitPrefab;
    }

    public Texture2D GetUnitIcon(string type)
    {
        Texture2D unitIcon = null;
        int typeIndex = UnitTypes.IndexOf(type);

        if (typeIndex != -1)
        {
            unitIcon = UnitAssetIcons[typeIndex];
        }

        return unitIcon;
    }
}

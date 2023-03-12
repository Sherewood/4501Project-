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

    void Awake()
    {
        Debug.Log("Initializing unit database");

        _unitData = new Dictionary<string, Dictionary<string, string>>();

        //worker
        CreateUnitDataRecord("player-dynamic-worker", 0, 0, "unitInfo unitState movement health harvester construction unitSpawner");

        //resource deposits
        CreateUnitDataRecord("neutral-static-mineraldep", 0, 0, "unitInfo unitState resource");
        CreateUnitDataRecord("neutral-static-fueldep", 0, 0, "unitInfo unitState resource");

        //buildings
        CreateUnitDataRecord("player-static-mainbase", 0, 0, "unitInfo unitState health planetaryEvac");
        CreateUnitDataRecord("player-static-civilianbuilding", 0, 0, "unitInfo unitState health civilian unitSpawner");
        CreateUnitDataRecord("player-static-barracks", 0, 0, "unitInfo unitState health unitBuilderComponent unitSpawner");

        //military units

        //human units
        CreateUnitDataRecord("player-dynamic-military-infantry", 0, 0, "unitInfo unitState movement health attack weapon targeting");
        CreateUnitDataRecord("player-dynamic-military-rpg", 0, 0, "unitInfo unitState movement health attack weapon targeting");
        CreateUnitDataRecord("player-dynamic-military-minigun", 0, 0, "unitInfo unitState movement health attack weapon targeting");

        //vehicles
        CreateUnitDataRecord("player-dynamic-military-tank", 0, 0, "unitInfo unitState movement health attack weapon targeting");
        CreateUnitDataRecord("player-dynamic-military-artillery", 0, 0, "unitInfo unitState movement health attack weapon targeting");

        //enemy units

        //edenite muncher
        CreateUnitDataRecord("enemy-dynamic-edenite_muncher", 0, 0, "unitInfo unitState movement health attack weapon targeting");

        //edenite ravager
        CreateUnitDataRecord("enemy-dynamic-edenite_ravager", 0, 0, "unitInfo unitState movement health attack weapon targeting");

        //edenite devil
        CreateUnitDataRecord("enemy-dynamic-edenite_devil", 0, 0, "unitInfo unitState movement health attack weapon targeting");

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

    /* Other queries */

    //get the size of a unit
    public Vector3 GetUnitDimensions(string type)
    {
        GameObject unitPrefab = null;
        int typeIndex = UnitTypes.IndexOf(type);

        if (typeIndex != -1)
        {
            unitPrefab = UnitAssetPrefabs[typeIndex];
        }

        return unitPrefab.transform.localScale;
    }

    //get the unit's name
    public string GetUnitName(string type)
    {
        GameObject unitPrefab = null;
        int typeIndex = UnitTypes.IndexOf(type);

        if (typeIndex != -1)
        {
            unitPrefab = UnitAssetPrefabs[typeIndex];
        }

        if(unitPrefab != null)
        {
            return unitPrefab.name;
        }

        return "";
    }
}

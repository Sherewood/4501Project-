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
    public List<Sprite> UnitAssetIcons;

    void Awake()
    {
        Debug.Log("Initializing unit database");

        _unitData = new Dictionary<string, Dictionary<string, string>>();

        //creating unit data record
        Dictionary<string, string> testUnitRecord = new Dictionary<string, string>();
        testUnitRecord.Add("minerals", "0");
        //format will be <component1> <component2> ... <component_n>, separated by spaces
        testUnitRecord.Add("components", "unitInfo unitState movement health targeting");

        Dictionary<string, string> playerUnitRecord = new Dictionary<string, string>();
        playerUnitRecord.Add("minerals", "0");
        playerUnitRecord.Add("components", "unitInfo unitState movement attack health targeting");

        Dictionary<string, string> testSpawnerRecord = new Dictionary<string, string>();
        testSpawnerRecord.Add("minerals", "0");
        testSpawnerRecord.Add("components", "unitInfo unitState unitSpawner");

        _unitData.Add("neutral-test", testUnitRecord);

        _unitData.Add("player-dynamic-test", playerUnitRecord);

        _unitData.Add("neutral-test-spawner", testSpawnerRecord);

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

    public Sprite GetUnitIcon(string type)
    {
        Sprite unitIcon = null;
        int typeIndex = UnitTypes.IndexOf(type);

        if (typeIndex != -1)
        {
            unitIcon = UnitAssetIcons[typeIndex];
        }

        return unitIcon;
    }
}

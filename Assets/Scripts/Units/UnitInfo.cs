using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/* Unit Component */
//Purpose: Store general information about a specific unit

public class UnitInfo : MonoBehaviour
{

    //type of the unit
    //general format: <allegiance>-<static/dynamic>-<unit name>
    //ex: player-dynamic-infantry -> player infantry unit
    public string UnitType;
    //unit image
    public Texture2D UnitPic;
    //resource costs of the unit
    private Dictionary<string, int> _unitCosts;

    //supported components of the unit
    public List<string> SupportedComponents;

    //allegiance of the unit - derived from unit type
    private string _unitAllegiance;

    void Start()
    {
        _unitCosts = new Dictionary<string, int>();
        _unitCosts.Add("minerals", 0);

        _unitAllegiance = UnitType.Split("-")[0];
    }

    public string GetUnitType()
    {
        return UnitType;
    }

    public string GetAllegiance()
    {
        return _unitAllegiance;
    }

    public int GetCostForResource(string resourceType)
    {
        if (_unitCosts.ContainsKey(resourceType))
        {
            return _unitCosts[resourceType];
        }
        //if unit has no cost defined for the given resource, return -1 to indicate this
        else
        {
            return -1;
        }
    }

    public void SetCostForResource(string resourceType, int cost)
    {
        if (_unitCosts.ContainsKey(resourceType))
        {
            _unitCosts[resourceType] = cost;
        }
        else
        {
            _unitCosts.Add(resourceType, cost);
        }
    }

    public bool DoesUnitHaveComponent(string component)
    {
        return SupportedComponents.Contains(component);
    }

    //set the supported components of the unit
    public void SetSupportedComponents(List<string> supportedComponents)
    {
        SupportedComponents = new List<string>(supportedComponents);
    }
}

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

    //resource costs of the unit
    private Dictionary<string, int> _unitCosts;

    //supported components of the unit
    public List<string> SupportedComponents;

    void Start()
    {
        _unitCosts = new Dictionary<string, int>();
        _unitCosts.Add("minerals", 0);
    }

    public string GetUnitType()
    {
        return UnitType;
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
}

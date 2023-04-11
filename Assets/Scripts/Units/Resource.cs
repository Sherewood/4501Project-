using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/* Unit Component */
//Purpose: Represents the amount of a resource held by a unit

public class Resource : MonoBehaviour
{

    /* Configuration */
    [Tooltip("The type of resource held by the unit")]
    public string ResourceType;

    [Tooltip("The amount of said resource held by the unit")]
    public int ResourceAmount;

    [Tooltip("The bar used to display the available resources")]
    public ProgressBarControl ResourceCapacityBar;

    private int _maxResourceAmount;

    private bool _depleted;

    void Start()
    {
        _maxResourceAmount = ResourceAmount;
        if (ResourceAmount > 0)
        {
            _depleted = false;
        }
    }

    void Update()
    {
        //if capacity bar is linked, update based on remaining resources
        if (ResourceCapacityBar != null)
        {
            ResourceCapacityBar.SetPercentage((float)ResourceAmount / _maxResourceAmount);
        }
    }

    public string GetResourceType()
    {
        return ResourceType;
    }
    
    public bool IsDepleted()
    {
        return ResourceAmount <= 0;
    }

    //handle request to withdraw 'resourceHarvestRate' of the deposit's resource.
    //return value indicates the amount of resources extracted, or -1 if the deposit is depleted.
    public int WithdrawResources(int resourceHarvestRate)
    {
        if (ResourceAmount <= 0)
        {
            return -1;
        }

        ResourceAmount -= resourceHarvestRate;

        //if ran out of resources
        if(ResourceAmount < 0)
        {
            int diff = ResourceAmount;

            ResourceAmount = 0;
            _depleted = true;

            return resourceHarvestRate + diff;
        }
        else
        {
            return resourceHarvestRate;
        }
    }
}

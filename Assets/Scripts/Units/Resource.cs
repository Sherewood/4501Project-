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

    private bool _depleted;

    void Start()
    {
        if (ResourceAmount > 0)
        {
            _depleted = false;
        }
    }

    public string GetResourceType()
    {
        return ResourceType;
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

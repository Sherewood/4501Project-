using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
/* Unit Component */
//Purpose: Handle the extraction of resources from a deposit.

public class Harvesting : MonoBehaviour
{
    /* Callbacks */
    private animation_Controller animator;
    private ResourceHarvestEvent _resourceHarvestEvent;
    public AIEvent AICallback;

    /* Configuration */

    [Tooltip("Amount of resources harvested at a time.")]
    public int HarvestingAmount;

    [Tooltip("Rate at which resources are harvested. (per second)")]
    public int HarvestingRate;

    [Tooltip("Rate at which resources are harvested. (per second)")]
    public int HarvestingCapacity;

    private UnitState _unitState;

    //cooldown for harvesting resource
    private const float BASE_COOLDOWN = 1;
    private float _cooldown;

    //the target resource deposit
    private Resource _targetDeposit;

    //held resources
    public int _heldResources;
    private string _heldResourceType;
    
    void Awake()
    {
        _unitState = GetComponent<UnitState>();

        _resourceHarvestEvent = new ResourceHarvestEvent();

        //just don't set harvesting rate to 0...
        _cooldown = BASE_COOLDOWN / HarvestingRate;
        animator = this.GetComponent<animation_Controller>();
    }



    // Update is called once per frame
    void Update()
    {
        if (_unitState.GetState() == UState.STATE_HARVESTING)
        {
            if (!animator.Equals(null)) animator.SetAnim("HARVEST");
            _cooldown -= Time.deltaTime;

            if (_cooldown < 0)
            {
                HarvestResource();
                _cooldown = BASE_COOLDOWN / HarvestingRate;
            }
        }
    }

    //periodic harvesting of resource from resource deposit
    private void HarvestResource()
    {
        _heldResourceType = _targetDeposit.GetResourceType();

        int harvestAmount;
        if (_heldResources + HarvestingAmount < HarvestingCapacity)
        {
            harvestAmount = _targetDeposit.WithdrawResources(HarvestingAmount);
        }
        else
        {
            harvestAmount = _targetDeposit.WithdrawResources(HarvestingCapacity - _heldResources);
        }

        _heldResources += harvestAmount;

        //-1 = deposit is depleted, stop harvesting.
        //prioritize deposit depleted callback over capacity reached for better termination of movement
        if (harvestAmount == -1 || _targetDeposit.IsDepleted())
        {
            AICallback.Invoke("depositDepleted");
            return;
        }

        if (_heldResources >= HarvestingCapacity)
        {
            AICallback.Invoke("capacityReached");
            return;
        }



        //
    }

    //try and start harvesting if there is a resource deposit at the unit's location
    public bool StartHarvesting()
    {
        //find the resource deposit at the unit's current position
        //short radius is more than enough if the unit is on the deposit
        Collider[] candidateObjects = Physics.OverlapSphere(transform.position, 0.75f);

        foreach(Collider candidateObject in candidateObjects)
        {
            //due to short search radius, can assume that the first gameObject with a Resource component is our guy
            Resource possibleResource = candidateObject.gameObject.GetComponent<Resource>();
            if (possibleResource != null)
            {
                _targetDeposit = possibleResource;
                break;
            }
        }

        return _targetDeposit != null;
    }

    //call to deposit the resources
    public void depositResources()
    {
        _resourceHarvestEvent.Invoke(_heldResourceType, _heldResources);
        AICallback.Invoke("returnForGathering");
        _heldResources = 0;
    }

    public void ConfigureResourceHarvestCallback(UnityAction<string,int> resourceHarvestCallback)
    {
        _resourceHarvestEvent.AddListener(resourceHarvestCallback);
    }


}

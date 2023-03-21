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

    /* Configuration */

    [Tooltip("Amount of resources harvested at a time.")]
    public int HarvestingAmount;

    [Tooltip("Rate at which resources are harvested. (per second)")]
    public int HarvestingRate;


    private UnitState _unitState;

    //cooldown for harvesting resource
    private const float BASE_COOLDOWN = 1;
    private float _cooldown;

    //the target resource deposit
    private Resource _targetDeposit;

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
        else if (_unitState.GetState() == UState.STATE_IDLE) animator.SetAnim("IDLE");
    }

    //periodic harvesting of resource from resource deposit
    private void HarvestResource()
    {
        string resourceType = _targetDeposit.GetResourceType();

        int harvestAmount = _targetDeposit.WithdrawResources(HarvestingAmount);
        
        //-1 = deposit is depleted, stop harvesting.
        if(harvestAmount == -1)
        {
            _unitState.SetState(UState.STATE_IDLE);
            return;
        }

        _resourceHarvestEvent.Invoke(resourceType, harvestAmount);
    }

    //handle reporting of destination being reached by movement component
    public void HandleDestinationReached()
    {
        //check if unit state is "moving to harvest"
        //alternative if this proves too buggy: just check if the target deposit is within a certain distance
        if(_unitState.GetState() == UState.STATE_MOVING_TO_HARVEST && _targetDeposit != null)
        {
            _unitState.SetState(UState.STATE_HARVESTING);
        }
    }

    //set the resource deposit to harvest from when in harvesting state
    //should receive from internal controller
    public void SetTargetResourceDeposit(GameObject targetDeposit)
    {
        _targetDeposit = targetDeposit.GetComponent<Resource>();
    
        if(_targetDeposit == null)
        {
            Debug.LogError("Error: Target deposit specified to worker with instance ID " + gameObject.GetInstanceID() + " does not have Resource component. Target instance ID: " + targetDeposit.GetInstanceID());
        }
    }

    public void ConfigureResourceHarvestCallback(UnityAction<string,int> resourceHarvestCallback)
    {
        _resourceHarvestEvent.AddListener(resourceHarvestCallback);
    }


}

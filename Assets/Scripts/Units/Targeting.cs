using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/* Unit Component */
//Purpose: Track enemy units within range of the player, and determine the unit's target

public class Targeting : MonoBehaviour
{

    private GameObject _currentTarget;
    private GameObject _orderedTarget;

    /* Configuration */

    //splitting into a shorter automatic range and longer requested range for performance reasons.
    [Tooltip("Range at which enemies will be automatically detected.")]
    public float AutoDetectRange;

    [Tooltip("Range at which enemies will be detected if another component asks to check.")]
    public float FullDetectRange;

    //todo: move this parameter elsewhere (unit info?)
    [Tooltip("List of allegiances that are opposed to this unit")]
    public List<string> TargetAllegiances;

    // Start is called before the first frame update
    void Start()
    {
        _currentTarget = null;
        _orderedTarget = null;
    }

    // General targeting update
    void Update()
    {
        //determine target based on auto detection range
        DetermineTarget(AutoDetectRange);
    }

    //get the currently determined target of the unit
    public GameObject GetTarget()
    {
        return _currentTarget;
    }
    
    //perform a max range scan to determine a target, then return what was found.
    public GameObject GetTargetAfterFullDetection()
    {
        DetermineTarget(FullDetectRange);

        return _currentTarget;
    }

    //set target that this unit should focus on
    public void SetOrderedTarget(GameObject orderedTarget)
    {
        _orderedTarget = orderedTarget;
    }

    //stop ordered target fixation
    public void ClearOrderedTarget()
    {
        _orderedTarget = null;
    }

    //determines the current target of the unit, based on the given targetting range
    private void DetermineTarget(float range)
    {
        Collider[] enemiesInRange = Physics.OverlapSphere(transform.position, range);

        //if no enemies in range, clear the current target unless it is the ordered target
        //or it is still in detection range (if using shorter than max range for this DetermineTarget call this is possible)
        if (enemiesInRange.Length == 0)
        {
            if (!IsOrderedTarget(_currentTarget) && !IsCurrentTargetStillInRange())
            {
                _currentTarget = null;
            }
            return;
        }

        //Check distance of each target to the player

        GameObject newClosestTarget = null;
        float newClosestDistance = range + 1.0f;

        foreach (Collider target in enemiesInRange)
        {
            GameObject targetObject = target.GetComponent<Collider>().gameObject;

            //if ordered target is in range, we can skip finding the closest, and just choose the ordered object as our target.
            if (IsOrderedTarget(targetObject))
            {
                newClosestTarget = targetObject;
                break;
            }

            //get info on object
            UnitInfo targetInfo = targetObject.GetComponent<UnitInfo>();

            if (targetInfo == null)
            {
                continue;
            }

            //determine if target is hostile based on allegiances
            if (!IsTargetHostile(targetInfo.GetAllegiance()))
            {
                continue;
            }

            //if target cannot be closest target, skip it 
            if (CheckIfTargetCannotBeClosestTarget(targetObject, newClosestDistance))
            {
                continue;
            }

            //now, find distance to target
            //obviously very inefficient, will likely need to find alternative strategy for this component as a whole
            float targetDistance = Vector3.Distance(transform.position, targetObject.transform.position);

            if (targetDistance < newClosestDistance)
            {
                newClosestTarget = targetObject;
                newClosestDistance = targetDistance;
            }
        }

        //finally, if the ordered target is currently being targetted, do not switch
        if (IsOrderedTarget(_currentTarget))
        {

            return;
        }

        _currentTarget = newClosestTarget;
    }

    //return true if enemy is clearly not the closest target, and distance calculation not needed
    private bool CheckIfTargetCannotBeClosestTarget(GameObject target, float closestDist)
    {
        /*
        Calculate manhattan x and z distances, if either of these is larger than the distance to the current closest target,
        then there is zero chance the target is the closest target, and we can ignore it without calculating
        euclidean distance
        */
        float distX = Mathf.Abs(transform.position.x - target.transform.position.x);
        float distZ = Mathf.Abs(transform.position.x - target.transform.position.z);

        return (distX >= closestDist + 0.01) || (distZ >= closestDist + 0.01);
    }

    //check if the given target object is the ordered target
    private bool IsOrderedTarget(GameObject target)
    {
        return (target == _orderedTarget && _orderedTarget != null);
    }

    //check if the currently selected target is still in range
    private bool IsCurrentTargetStillInRange()
    {
        if(_currentTarget == null)
        {
            return false;
        }

        return Vector3.Distance(transform.position, _currentTarget.transform.position) > FullDetectRange;
    }

    //determine if target is hostile to this unit in question
    private bool IsTargetHostile(string targetAllegiance)
    {
        return TargetAllegiances.Contains(targetAllegiance);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
/* Unit Component */
//Purpose: Track enemy units within range of the player, and determine the unit's target

public class Targeting : MonoBehaviour
{
    public AIEvent AICallback;

    private GameObject _currentTarget;

    //true if should focus on current target
    private bool _focusMode;

    /* Configuration */

    //if game lags too much will have to look at having shorter automatic targeting range again
    //or have targeting done entirely upon request (would need an Update loop for the unit AI)
    [Tooltip("Range at which enemies will be automatically detected.")]
    public float DetectRange;

    //todo: move this parameter elsewhere (unit info?)
    [Tooltip("List of allegiances that are opposed to this unit")]
    public List<string> TargetAllegiances;

    // Start is called before the first frame update
    void Start()
    {
        _currentTarget = null;
        _focusMode = false;
    }

    // General targeting update
    void Update()
    {
        //if currently focused on a target, monitor to see if that target has died
        if (_focusMode)
        {
            if (_currentTarget == null || _currentTarget.GetComponent<UnitInfo>() == null)
            {
                _focusMode = false;
                AICallback.Invoke("targetLost");
            }
        }
        else
        {
            DetermineTarget(DetectRange);
        }
    }

    //get the currently determined target of the unit
    public GameObject GetTarget()
    {
        return _currentTarget;
    }

    //used when targeting component should focus on a specific target, and ignore its targeting range
    public void SetTargetFocus(GameObject target)
    {
        _currentTarget = target;
        _focusMode = true;
    }
    //disable focusing on a specific target if currently happening.
    public void StopTargetFocus()
    {
        if (_focusMode)
        {
            _currentTarget = null;
            _focusMode = false;
        }
    }

    //determines the current target of the unit, based on the given targetting range
    private void DetermineTarget(float range)
    {
        Collider[] enemiesInRange = Physics.OverlapSphere(transform.position, range);

        //if no enemies in range, clear the current target unless it is the ordered target
        //or it is still in detection range (if using shorter than max range for this DetermineTarget call this is possible)
        if (enemiesInRange.Length == 0)
        {
            //if target has been lost, 
            if (_currentTarget != null && !IsCurrentTargetStillInRange())
            {
                _currentTarget = null;
                AICallback.Invoke("targetLost");
            }
            return;
        }

        //Check distance of each target to the player

        GameObject newClosestTarget = null;
        float newClosestDistance = range + 1.0f;

        foreach (Collider target in enemiesInRange)
        {
            GameObject targetObject = target.GetComponent<Collider>().gameObject;

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

        //if no target, but previously had target, then notify of target loss
        if(newClosestTarget == null && _currentTarget != null)
        {
            _currentTarget = null;
            AICallback.Invoke("targetLost");
        }
        //if the determined target is different than the previously tracked target, notify of target change
        else if (newClosestTarget != null && newClosestTarget != _currentTarget)
        {
            _currentTarget = newClosestTarget;
            AICallback.Invoke("targetChanged");
        }
    }

    public List<GameObject> GetTargetsInRange(float range)
    {
        List<GameObject> targets = new List<GameObject>();

        Collider[] enemiesInRange = Physics.OverlapSphere(transform.position, range);

        foreach (Collider target in enemiesInRange)
        {
            GameObject targetObject = target.GetComponent<Collider>().gameObject;

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

            targets.Add(targetObject);
        }

        return targets;
    }

    public List<GameObject> GetUnitsOfTypeInRange(float range, string type)
    {
        List<GameObject> targets = new List<GameObject>();

        Collider[] enemiesInRange = Physics.OverlapSphere(transform.position, range);

        foreach (Collider target in enemiesInRange)
        {
            GameObject targetObject = target.GetComponent<Collider>().gameObject;

            //get info on object
            UnitInfo targetInfo = targetObject.GetComponent<UnitInfo>();

            if (targetInfo == null)
            {
                continue;
            }

            //determine if target is hostile based on allegiances
            if (!targetInfo.GetType().Equals(type))
            {
                continue;
            }

            targets.Add(targetObject);
        }

        return targets;
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
        float distZ = Mathf.Abs(transform.position.z - target.transform.position.z);

        return (distX >= closestDist + 0.01) || (distZ >= closestDist + 0.01);
    }

    //check if the currently selected target is still in range
    private bool IsCurrentTargetStillInRange()
    {
        return Vector3.Distance(transform.position, _currentTarget.transform.position) > DetectRange;
    }

    //determine if target is hostile to this unit in question
    private bool IsTargetHostile(string targetAllegiance)
    {
        return TargetAllegiances.Contains(targetAllegiance);
    }
}

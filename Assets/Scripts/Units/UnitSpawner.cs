using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UnitSpawner : MonoBehaviour
{
    //callback to internal controller to report unit spawn
    //will have to be configured during initialization
    private EntitySpawnEvent _entitySpawnEvent;

    //unit database, needed to fetch associated prefab of unit
    private UnitDatabase _unitDb; 

    /* Unit spawn configuration */

    [Tooltip("types of units to be periodically spawned")]
    public List<string> PeriodicUnitSpawnTypes;

    [Tooltip("intervals for units to be periodically spawned in seconds")]
    public List<float> PeriodicUnitSpawnIntervals;
    private List<float> currentUnitSpawnIntervals;

    [Tooltip("toggles spawner's random spawn location mode (currently disabled)")]
    public bool RandomSpawnMode;

    [Tooltip("offset from the spawner's location where the unit will be spawned")]
    public Vector3 SpawnPositionFixedOffset;

    [Tooltip("the maximum range in which units can be spawned around the spawner. Format (x1,y1,x2,y2)")]
    //also the maximum range in which units can be spawned
    //Ex: If SpawnBox is [0,0,10,10], units will be randomly placed in range ([pos.x,pos.x+10]+fixedOffset.x,pos.y+fixedOffset.y,[pos.z,pos.z+10]+fixedOffset.z)
    public Vector4 SpawnBoundary;

    [Tooltip("number of times the spawning of a unit should be re-attempted due to obstruction at a prior location")]
    public int spawnRetryAttemptLimit;

    private System.Random _random;

    void Awake()
    {
        _entitySpawnEvent = new EntitySpawnEvent();
        _unitDb = FindObjectOfType<UnitDatabase>();
        currentUnitSpawnIntervals = new List<float>(PeriodicUnitSpawnIntervals);
        _random = new System.Random();
    }

    // Update is called once per frame
    void Update()
    {
        for(int i = 0; i < currentUnitSpawnIntervals.Count; i++)
        {
            currentUnitSpawnIntervals[i] -= Time.deltaTime;

            if (currentUnitSpawnIntervals[i] <= 0.0f)
            {
                SpawnUnit(PeriodicUnitSpawnTypes[i]);
                currentUnitSpawnIntervals[i] = PeriodicUnitSpawnIntervals[i] + currentUnitSpawnIntervals[i];
            }
        }
    }

    //helpers

    //spawns a unit of a specified type
    public void SpawnUnit(string unitType)
    {
        Debug.Log("Spawning unit of type - " + unitType);
        //fetch prefab from unit db
        GameObject prefab = _unitDb.GetUnitPrefab(unitType);

        if(prefab == null)
        {
            Debug.LogWarning("Error: attempted to spawn unit of type " + unitType + " which has no associated prefab asset.");
            return;
        }

        //calculate spawn coordinates
        Vector3 spawnPos = PickSpawnCoordinates();

        //todo: determine if spawn position is blocked, and adjust position if it is
        Vector3 finalSpawnPos = new Vector3();

        if(!AdjustSpawnCoordinates(prefab, spawnPos,out finalSpawnPos))
        {
            Debug.LogWarning("Unable to spawn unit due to obstruction");
            return;
        }

        //instantiate new unit
        GameObject newUnit = Instantiate(prefab, finalSpawnPos, new Quaternion());
        
        
        

        //trigger callback to controller
        _entitySpawnEvent.Invoke(newUnit);
    }

    //chooses spawn coordinates for a given unit
    private Vector3 PickSpawnCoordinates()
    {
        Vector3 spawnPos = transform.position + SpawnPositionFixedOffset;

        //todo: add random spawn mode later....
        if (RandomSpawnMode)
        {

        }

        return spawnPos;
    }

    //check if spawn position is blocked, and adjust spawn coordinates if it is
    private bool AdjustSpawnCoordinates(GameObject prefab, Vector3 spawnPos, out Vector3 newSpawnPos)
    {
        bool possiblyBlocked = true;
        int attempts = 0;

        newSpawnPos = spawnPos;

        //get unit dimensions / 2
        Vector3 boxDimensions = new Vector3(prefab.transform.localScale.x/2, prefab.transform.localScale.y/2, prefab.transform.localScale.z/2);


        while (possiblyBlocked && attempts < spawnRetryAttemptLimit)
        {
            //area checked should be slightly larger than the unit
            Collider[] overlappedUnits = Physics.OverlapBox(newSpawnPos, boxDimensions);

            //check if any units in collision range
            bool unitsInCollider = false;
            for (int i = 0; i < overlappedUnits.Length; i++)
            {
                GameObject entity = overlappedUnits[i].gameObject;

                if (entity.GetComponent<UnitInfo>() != null)
                {
                    unitsInCollider = true;
                    break;
                }
            }
            if (!unitsInCollider)
            {
                break;
            }

            //adjust position based on random factor and previously calculated dimensions
            //todo: come up with better repositioning algorithm
            int repositionTries = 0;
            while (repositionTries < spawnRetryAttemptLimit) {
                Vector3 factor = new Vector3(_random.Next(3) - 1, 0, _random.Next(3) - 1);
                Vector3 oldSpawnPos = newSpawnPos;
                newSpawnPos += 2 * new Vector3(boxDimensions.x * factor.x, 0, boxDimensions.z * factor.z);

                //if updated spawn position is within spawner's acceptable spawn boundaries, try spawning again
                Vector3 testPos = newSpawnPos - gameObject.transform.position;
                if ((testPos.x >= SpawnBoundary.x && testPos.x <= SpawnBoundary.z) &&
                    (testPos.z >= SpawnBoundary.y && testPos.z <= SpawnBoundary.w))
                {
                    Debug.Log("Hi");
                    break;
                }

                //otherwise, reset to old position and try again.
                newSpawnPos = oldSpawnPos;

                repositionTries++;
            }

            attempts++;
        }

        if (attempts >= spawnRetryAttemptLimit)
        {
            Debug.LogWarning("Failed to find spawn location for unit - too obstructed.");
            return false;
        }

        return true;
    }

    //bind the unit spawn callback
    public void ConfigureUnitSpawnCallback(UnityAction<GameObject> unitSpawnCallback)
    {
        _entitySpawnEvent.AddListener(unitSpawnCallback);
    }

   
}

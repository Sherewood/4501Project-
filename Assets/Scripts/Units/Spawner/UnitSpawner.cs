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

    //terrain, use for determining height of terrain
    private Terrain _terrain;

    /* Unit spawn configuration */

    [Tooltip("types of units to be periodically spawned")]
    public List<string> PeriodicUnitSpawnTypes;

    [Tooltip("intervals for units to be periodically spawned in seconds")]
    public List<float> PeriodicUnitSpawnIntervals;
    private List<float> currentUnitSpawnIntervals;

    [Tooltip("offset from the spawner's location where the unit will be spawned")]
    public Vector3 SpawnPositionFixedOffset;

    [Tooltip("the maximum range in which units can be spawned around the spawner. Format (x1,y1,x2,y2)")]
    //also the maximum range in which units can be spawned
    //Ex: If SpawnBox is [0,0,10,10], units will be randomly placed in range ([pos.x,pos.x+10]+fixedOffset.x,pos.y+fixedOffset.y,[pos.z,pos.z+10]+fixedOffset.z)
    public Vector4 SpawnBoundary;

    [Tooltip("number of times the spawning of a unit should be re-attempted due to obstruction at a prior location")]
    public int SpawnRetryAttemptLimit;

    [Tooltip("If enabled, spawner will attempt to relocate unit if spawning region is occupied.")]
    public bool SpawnRetryMode;

    [Tooltip("If enabled, spawner will stop spawning if unit is obstructed")]
    public bool StopObstructionMode = true;

    [Tooltip("The particle effect used when a unit is spawned, if applicable")]
    public GameObject SpawnEffect;

    private System.Random _random;

    void Awake()
    {
        _entitySpawnEvent = new EntitySpawnEvent();
        _unitDb = FindObjectOfType<UnitDatabase>();
        currentUnitSpawnIntervals = new List<float>(PeriodicUnitSpawnIntervals);
        _random = new System.Random();
        _terrain = FindObjectOfType<Terrain>();
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
    public GameObject SpawnUnit(string unitType)
    {
        Debug.Log("Spawning unit of type - " + unitType);
        //fetch prefab from unit db
        GameObject prefab = _unitDb.GetUnitPrefab(unitType);

        if(prefab == null)
        {
            Debug.LogWarning("Error: attempted to spawn unit of type " + unitType + " which has no associated prefab asset.");
            return null;
        }

        //calculate spawn coordinates
        Vector3 spawnPos = PickSpawnCoordinates();

        //determine if spawn position is blocked, and adjust position if it is
        Vector3 finalSpawnPos = new Vector3();

        if (StopObstructionMode)
        {
            if (!AdjustSpawnCoordinates(prefab, spawnPos, out finalSpawnPos))
            {
                Debug.LogWarning("Unable to spawn unit due to obstruction");
                return null;
            }
        }
        else
        {
            finalSpawnPos = spawnPos;
        }

        //instantiate new unit
        GameObject newUnit = Instantiate(prefab, finalSpawnPos, new Quaternion());
        
        if (SpawnEffect != null)
        {
            //add spawn effect on terrain
            GameObject spawnEffect = Instantiate(SpawnEffect, new Vector3(finalSpawnPos.x, _terrain.SampleHeight(finalSpawnPos), finalSpawnPos.z), Quaternion.identity);
        }

        //trigger callback to controller
        _entitySpawnEvent.Invoke(newUnit);

        return newUnit;
    }

    //sets the fixed spawn offset
    public void SetSpawnOffset(Vector3 newOffset)
    {
        SpawnPositionFixedOffset = newOffset;
    }

    //chooses spawn coordinates for a given unit
    private Vector3 PickSpawnCoordinates()
    {
        Vector3 spawnPos = transform.position + SpawnPositionFixedOffset;

        return spawnPos;
    }

    //check if spawn position is blocked, and adjust spawn coordinates if it is
    private bool AdjustSpawnCoordinates(GameObject prefab, Vector3 spawnPos, out Vector3 newSpawnPos)
    {
        bool possiblyBlocked = true;
        int attempts = 0;

        //start with initially determined spawn position
        newSpawnPos = spawnPos;

        //get unit dimensions / 2
        Vector3 boxDimensions = new Vector3(prefab.transform.localScale.x/2, prefab.transform.localScale.y/2, prefab.transform.localScale.z/2);

        //first, test if spawn obstructed
        if(!IsUnitSpawnObstructed(newSpawnPos, boxDimensions))
        {
            return true;
        }

        //do not attempt repositioning if spawn retry mode is off
        if (!SpawnRetryMode)
        {
            Debug.LogWarning("Spawn location is obstructed. Consider turning on spawn retry mode?");
            return false;
        }

        //attempt to reposition 
        while (possiblyBlocked && attempts < SpawnRetryAttemptLimit)
        {
            //adjust position based on random factor and previously calculated dimensions
            //todo: come up with better repositioning algorithm
            int repositionTries = 0;
            while (repositionTries < SpawnRetryAttemptLimit) {
                Vector3 factor = new Vector3(_random.Next(3) - 1, 0, _random.Next(3) - 1);
                Vector3 oldSpawnPos = newSpawnPos;
                newSpawnPos += 2 * new Vector3(boxDimensions.x * factor.x, 0, boxDimensions.z * factor.z);

                //if updated spawn position is within spawner's acceptable spawn boundaries, try spawning again
                Vector3 testPos = newSpawnPos - gameObject.transform.position;
                if ((testPos.x >= SpawnBoundary.x && testPos.x <= SpawnBoundary.z) &&
                    (testPos.z >= SpawnBoundary.y && testPos.z <= SpawnBoundary.w))
                {
                    break;
                }

                //otherwise, reset to old position and try again.
                newSpawnPos = oldSpawnPos;

                repositionTries++;
            }
            
            //if the new spawn position is not obstructed, stop and accept this position as the valid one.
            if(!IsUnitSpawnObstructed(newSpawnPos, boxDimensions))
            {
                break;
            }

            attempts++;
        }

        if (attempts >= SpawnRetryAttemptLimit)
        {
            Debug.LogWarning("Failed to find spawn location for unit - too obstructed.");
            return false;
        }

        return true;
    }

    private bool IsUnitSpawnObstructed(Vector3 spawnPos, Vector3 dimensions)
    {
        //area checked should be slightly larger than the unit
        Collider[] overlappedUnits = Physics.OverlapBox(spawnPos, dimensions * 1.05f);

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
        return unitsInCollider;
    }

    //bind the unit spawn callback
    public void ConfigureUnitSpawnCallback(UnityAction<GameObject> unitSpawnCallback)
    {
        _entitySpawnEvent.AddListener(unitSpawnCallback);
    }

   
}

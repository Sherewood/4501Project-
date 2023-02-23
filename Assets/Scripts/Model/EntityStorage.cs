using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
/* Model Class */
//Purpose: Handle tracking of units in the game for the game controllers, and support queries by those controllers

public class EntityStorage : MonoBehaviour
{
    //the hash table storing all entities, mapped to their instance ids
    private Dictionary<int, GameObject> _entityStorage;

    void Start()
    {
        _entityStorage = new Dictionary<int, GameObject>();
    }

    //adds entity to entity storage
    public void AddEntity(GameObject entity)
    {
        if (!_entityStorage.ContainsKey(entity.GetInstanceID()))
        {
            _entityStorage.Add(entity.GetInstanceID(), entity);
        }
    }

    //removes and returns entity's GameObject if it exists, or returns null otherwise
    public GameObject removeEntity(int instanceID)
    {
        GameObject entityToRemove = null;

        if (_entityStorage.ContainsKey(instanceID))
        {
            _entityStorage.Remove(instanceID,out entityToRemove);
        }

        return entityToRemove;
    }

    //returns entity's GameObject if it exists, or null otherwise
    public GameObject findEntity(int instanceID)
    {
        GameObject entityToReturn = null;
        
        if (_entityStorage.ContainsKey(instanceID))
        {
            entityToReturn = _entityStorage[instanceID];
        }

        return entityToReturn;
    }

    //return all entities whose coordinates are within the box formed by bottomLeftPos and topRightPos
    public List<GameObject> findEntitiesInRange(Vector3 bottomLeftPos, Vector3 topRightPos)
    {
        List<GameObject> entitiesInRange = new List<GameObject>();

        foreach (GameObject entity in _entityStorage.Values)
        {
            Vector3 entityPos = entity.transform.position;

            //check if within boundaries
            if(((entityPos.x >= bottomLeftPos.x) && (entityPos.z >= bottomLeftPos.z)) &&
                ((entityPos.x <= topRightPos.x) && (entityPos.z <= topRightPos.z)))
            {
                entitiesInRange.Add(entity);
            }
        }

        return entitiesInRange;
    }

    //returns all entities with a matching unit type
    public List<GameObject> GetPlayerUnitsOfType(string type)
    {
        List<GameObject> entitiesWithType = new List<GameObject>();

        foreach (GameObject entity in _entityStorage.Values)
        {
            //will cause crash if entity with no unit info component is added to storage
            UnitInfo entityInfo = entity.GetComponent<UnitInfo>();

            //compare types up to the length of the specified type (so for example a given type 'player' will include all player units and buildings)
            if (String.Compare(type, 0, entityInfo.GetUnitType(), 0, type.Length) == 0)
            {
                entitiesWithType.Add(entity);
            }
        }

        return entitiesWithType;
    }
}

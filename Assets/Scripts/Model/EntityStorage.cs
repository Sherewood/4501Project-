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
    public int count;
    void Awake()
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
        count++;
    }

    //removes and returns entity's GameObject if it exists, or returns null otherwise
    public GameObject RemoveEntity(int instanceID)
    {
        GameObject entityToRemove = null;

        if (_entityStorage.ContainsKey(instanceID))
        {
            _entityStorage.Remove(instanceID,out entityToRemove);
        }
        count--;
        return entityToRemove;
    }

    //returns entity's GameObject if it exists, or null otherwise
    public GameObject FindEntity(int instanceID)
    {
        GameObject entityToReturn = null;
        
        if (_entityStorage.ContainsKey(instanceID))
        {
            entityToReturn = _entityStorage[instanceID];
        }

        return entityToReturn;
    }

    //return all entities whose coordinates are within the box formed by topLeftPos and bottomRightPos
    public List<GameObject> FindEntitiesInRange(Vector3 topLeftPos, Vector3 bottomRightPos)
    {
        List<GameObject> entitiesInRange = new List<GameObject>();

        //due to the camera orientation, need an OBB test instead of an AABB test

        //temporary hard-coded 45 degree y-axis rotation for normal vectors
        //obviously, this will cause area selection to break if the camera is oriented differently.
        Quaternion testRot = Quaternion.AngleAxis(45, new Vector3(0, 1, 0));

        //thinking: forward vector of camera should work as normal vector for testing x-axis, and so on?
        //kinda lost the thread here
        Vector3 xAxisNormal = testRot * Vector3.forward;
        Vector3 zAxisNormal = testRot * -Vector3.right;

        //project points onto normals
        //probably horrendous to do it this way instead of calculating scalar projection normally
        //blame unity for not including a Vector3 scalar projection function I guess...
        //todo: try manually inserting scalar projection formula before submission because this is vomit inducing
        //update: left this in (for advanced prototype) in case you want a laugh
        float topLeftProjectionXAxis = Vector3.Dot(topLeftPos, xAxisNormal);
        float bottomRightProjectionXAxis = Vector3.Dot(bottomRightPos, xAxisNormal);

        if(topLeftProjectionXAxis < bottomRightProjectionXAxis)
        {
            float temp = bottomRightProjectionXAxis;
            bottomRightProjectionXAxis = topLeftProjectionXAxis;
            topLeftProjectionXAxis = temp;
        }

        float topLeftProjectionZAxis = Vector3.Dot(topLeftPos, zAxisNormal);
        float bottomRightProjectionZAxis = Vector3.Dot(bottomRightPos, zAxisNormal);

        if (topLeftProjectionZAxis < bottomRightProjectionZAxis)
        {
            float temp = bottomRightProjectionZAxis;
            bottomRightProjectionZAxis = topLeftProjectionZAxis;
            topLeftProjectionZAxis = temp;
        }

        Debug.Log("Projection range: (" + topLeftProjectionXAxis + "," + topLeftProjectionZAxis + ") - (" + bottomRightProjectionXAxis + "," + bottomRightProjectionZAxis + ")");

        foreach (GameObject entity in _entityStorage.Values)
        {
            Vector3 entityPos = entity.transform.position;

            //only checking if center point lies within the box for now....
            //this worked out so I'm not touching this ever again.
            float entityProjectionXAxis = Vector3.Dot(entityPos, xAxisNormal);

            float entityProjectionZAxis = Vector3.Dot(entityPos, zAxisNormal);

            //check if within boundaries
            if((bottomRightProjectionXAxis <= entityProjectionXAxis && entityProjectionXAxis <= topLeftProjectionXAxis) &&
                (bottomRightProjectionZAxis <= entityProjectionZAxis && entityProjectionZAxis <= topLeftProjectionZAxis))
            {
                entitiesInRange.Add(entity);
            }
        }

        Debug.Log("Found " + entitiesInRange.Count + " entities.");

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
    //returns all entities 
    public List<GameObject> getAllUnits()
    {
        List<GameObject> all = new List<GameObject>();

        foreach (GameObject entity in _entityStorage.Values)
        {
            //will cause crash if entity with no unit info component is added to storage
            UnitInfo entityInfo = entity.GetComponent<UnitInfo>();

            all.Add(entity);
        }

        return all;
    }
}

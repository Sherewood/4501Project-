using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/* Internal Controller Class */
//Purpose: Managing newly created/deleted units

public class UnitCreationController : MonoBehaviour
{

    private EntityStorage _entityStorage;


    void Start()
    {
        _entityStorage = FindObjectOfType<EntityStorage>();
    }

    //store newly created entities in the Entity Storage
    public void StoreCreatedEntity(GameObject unit)
    {
        if(unit.GetComponent<UnitInfo>() == null)
        {
            Debug.LogError("Error: Unit Creation Controller given unit with no Unit Info component");
            return;
        }

        _entityStorage.AddEntity(unit);
    }

    //remove dead entities from the Entity Storage, then delete them.
    public void DeleteDeadEntity(GameObject unit)
    {
        if(_entityStorage.RemoveEntity(unit.GetInstanceID()) == null)
        {
            Debug.LogError("Error: Unit Creation Controller tried to delete unit which was already deleted, or never tracked.");
            return;
        }

        Destroy(unit);
    }
}

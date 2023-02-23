using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/* Internal Controller Class */
//Purpose: To handle game-state related matters such as player resources, elapsed time and win conditions

public class GameStateController : MonoBehaviour
{

    private EntityStorage _entityStorage;

    void Start()
    {
        Debug.Log("Game State Controller - Begin game initialization");
        _entityStorage = FindObjectOfType<EntityStorage>();

        //get all units (each unit must have a Unit Info component)
        UnitInfo[] allUnits = FindObjectsOfType<UnitInfo>();
        
        foreach (UnitInfo unit in allUnits)
        {
            Debug.Log("Adding unit of type '" + unit.GetUnitType() + "' to entity storage");
            _entityStorage.AddEntity(unit.gameObject);
        }

        Debug.Log("Initialized entity storage with " + allUnits.Length + " units.");
    }

    void Update()
    {
    }
}

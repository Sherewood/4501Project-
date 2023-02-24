using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
/* Internal Controller Class */
//Purpose: To handle game-state related matters such as player resources, elapsed time and win conditions

public class GameStateController : MonoBehaviour
{

    private EntityStorage _entityStorage;

    //need access to bind callbacks
    private InternalControllerEventHandler _eventHandler;

    void Start()
    {
        Debug.Log("Game State Controller - Begin game initialization");
        _entityStorage = FindObjectOfType<EntityStorage>();
        _eventHandler = GetComponent<InternalControllerEventHandler>();
        //get all units (each unit must have a Unit Info component)
        UnitInfo[] allUnits = FindObjectsOfType<UnitInfo>();
        
        foreach (UnitInfo unit in allUnits)
        {
            //bind callbacks
            BindUnitCallbacks(unit);

            //add to entity storage
            Debug.Log("Adding unit of type '" + unit.GetUnitType() + "' to entity storage");
            _entityStorage.AddEntity(unit.gameObject);
        }

        Debug.Log("Initialized entity storage with " + allUnits.Length + " units.");
    }

    //update loop functionality goes here
    void Update()
    {
    }

    //other helpers go here

    //bind callbacks for new unit (cannot do directly in the prefab)
    //all unit components that send callbacks to the internal controller will need some configuration here
    private void BindUnitCallbacks(UnitInfo unit)
    {
        //if unit spawner -> bind EntitySpawnEvent callback
        if (unit.DoesUnitHaveComponent("unitSpawner"))
        {
            Debug.Log("Binding callback for unit spawner - instance ID " + unit.gameObject.GetInstanceID());
            UnitSpawner spawner = unit.gameObject.GetComponent<UnitSpawner>();
            spawner.ConfigureUnitSpawnCallback((UnityAction<GameObject>)_eventHandler.HandleUnitSpawnEvent);
        }
    }
}

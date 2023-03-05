using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
/* Internal Controller Class */
//Purpose: Managing newly created/deleted units

public class UnitCreationController : MonoBehaviour
{

    private EntityStorage _entityStorage;

    private UnitDatabase _unitDatabase;

    //need access to bind callbacks
    private InternalControllerEventHandler _eventHandler;

    void Awake()
    {
        _entityStorage = FindObjectOfType<EntityStorage>();
        _unitDatabase = FindObjectOfType<UnitDatabase>();
        _eventHandler = GetComponent<InternalControllerEventHandler>();
    }

    //store newly created entities in the Entity Storage
    public void StoreCreatedEntity(GameObject unit)
    {

        UnitInfo unitInfo = unit.GetComponent<UnitInfo>();

        if(unitInfo == null)
        {
            Debug.LogError("Error: Unit Creation Controller given unit with no Unit Info component");
            return;
        }

        //configure the displayed supported components based on the unit db
        List<string> unitSupportedComponents = _unitDatabase.GetComponentsForUnitType(unitInfo.GetUnitType());

        unitInfo.SetSupportedComponents(unitSupportedComponents);

        //setup callbacks
        BindUnitCallbacks(unitInfo);

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

        //if health component -> bind EntityDeathEvent callback
        if (unit.DoesUnitHaveComponent("health"))
        {
            Health health = unit.gameObject.GetComponent<Health>();
            health.ConfigureEntityDeathCallback((UnityAction<GameObject>)_eventHandler.HandleUnitDeadEvent);
        }

        //if harvesting component -> bind ResourceHarvestEvent callback
        if (unit.DoesUnitHaveComponent("harvester"))
        {
            Harvesting harvester = unit.gameObject.GetComponent<Harvesting>();
            harvester.ConfigureResourceHarvestCallback((UnityAction<string, int>)_eventHandler.HandleResourceHarvestEvent);
        }

        //if civilian component -> bind CivilianEvacEvent callback
        if (unit.DoesUnitHaveComponent("civilian"))
        {
            Civilian civilianComp = unit.gameObject.GetComponent<Civilian>();
            civilianComp.ConfigureCivilianEvacCallback((UnityAction<int>)_eventHandler.HandleCivilianEvacEvent);
        }

    }
}

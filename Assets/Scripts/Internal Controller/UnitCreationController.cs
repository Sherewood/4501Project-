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

    private UnitController _unitController;

    //prefab used for health bar on unit
    public HealthBarControl HealthBarPrefab;

    void Awake()
    {
        _entityStorage = FindObjectOfType<EntityStorage>();
        _unitDatabase = FindObjectOfType<UnitDatabase>();
        _eventHandler = GetComponent<InternalControllerEventHandler>();
        _unitController = GetComponent<UnitController>();
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

        //add UI elements
        AddUnitUIElements(unitInfo);

        _entityStorage.AddEntity(unit);
    }

    //remove dead entities from the Entity Storage, then delete them.
    public void DeleteDeadEntity(GameObject unit)
    {
        if(_entityStorage.RemoveEntity(unit.GetInstanceID()) == null)
        {
            Debug.LogError("Error: Unit Creation Controller tried to delete unit which was already deleted, or never tracked. Unit name: " + unit.name);
            return;
        }

        _unitController.DeleteUnitFromFlock(unit);

        /* delayed death system */
        //Needed in order to give the death animation time to play without interfering with the game
        //1) Set the unit's state to dead, disable relevant components
        //2) Disable all collision detection on the unit, disable relevant components
        //3) Start coroutine that destroys unit after a certain amount of time

        //steps 1-2
        UnitState state = unit.GetComponent<UnitState>();
        state.DisableUnit();

        //step 3
        StartCoroutine(DestroyUnitAfterSetTime(unit, 1.6f));
    }

    //coroutine to destroy the unit after a given interval has elapsed
    IEnumerator DestroyUnitAfterSetTime(GameObject unit, float time)
    {
        while(time > 0)
        {
            time -= Time.deltaTime;
            yield return null;
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

        //if movement component -> bind DestinationReachedEvent callback
        if (unit.DoesUnitHaveComponent("movement"))
        {
            Movement movement = unit.gameObject.GetComponent<Movement>();
            movement.ConfigureDestinationReachedCallback((UnityAction<GameObject>)_eventHandler.HandleDestinationReachedEvent);
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

        //if research generator component.... ALSO bind ResourceHarvestEvent callback because the deadline is in less than 2 hours
        if (unit.DoesUnitHaveComponent("researchGenerator"))
        {
            ResearchGenerator researchGenerator = unit.gameObject.GetComponent<ResearchGenerator>();
            researchGenerator.ConfigureResearchGenCallback((UnityAction<string, int>)_eventHandler.HandleResourceHarvestEvent);
        }

        //if civilian component -> bind CivilianEvacEvent callback
        if (unit.DoesUnitHaveComponent("civilian"))
        {
            Civilian civilianComp = unit.gameObject.GetComponent<Civilian>();
            civilianComp.ConfigureCivilianEvacCallback((UnityAction<int>)_eventHandler.HandleCivilianEvacEvent);
        }

        //if planetary evacuation component -> bind EndOfGameEvent callback
        if (unit.DoesUnitHaveComponent("planetaryEvac"))
        {
            PlanetaryEvacuation planetaryEvac = unit.gameObject.GetComponent<PlanetaryEvacuation>();
            planetaryEvac.ConfigureEndOfGameCallback((UnityAction<bool>)_eventHandler.HandleEndOfGameEvent);
        }

        //if AI Control component -> bind PositionRequestEvent callback
        if (unit.DoesUnitHaveComponent("AIControl"))
        {
            AIControl unitAI = unit.gameObject.GetComponent<AIControl>();
            unitAI.ConfigurePositionRequestCallback((UnityAction<string, GameObject>)_eventHandler.HandlePositionRequestEvent);

            //only start the AI after callback is set up
            unitAI.StartAI();
        }

    }

    //add UI elements for units
    private void AddUnitUIElements(UnitInfo unit)
    {
        //add health bar to track unit's health
        if (unit.DoesUnitHaveComponent("health")){
            HealthBarControl newHealthBar = Instantiate(HealthBarPrefab, new Vector3(0,0,0), Quaternion.identity);
            newHealthBar.SetTarget(unit.gameObject);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
/* Internal Controller Class */
//Purpose: To handle game-state related matters such as player resources, elapsed time and win conditions

public class GameStateController : MonoBehaviour
{

    //callbacks
    public EndOfGameEvent EndOfGameEvent;

    //controller/model classes

    private EntityStorage _entityStorage;

    private UnitDatabase _unitDb;

    private UnitCreationController _unitCreationController;

    private GameStateModel _gameStateModel;

    private ResearchModel _researchModel;

    //holds list of enemy spawners

    private List<EnemySpawner> _enemySpawners;


    //main base

    private PlanetaryEvacuation _playerMainBase;

    void Start()
    {
        Debug.Log("Game State Controller - Begin game initialization");
        _unitCreationController = GetComponent<UnitCreationController>();
        _gameStateModel = FindObjectOfType<GameStateModel>();
        _researchModel = FindObjectOfType<ResearchModel>();
        _entityStorage = FindObjectOfType<EntityStorage>();
        _unitDb = FindObjectOfType<UnitDatabase>();
        //get all units (each unit must have a Unit Info component)
        UnitInfo[] allUnits = FindObjectsOfType<UnitInfo>();

        //find the main base
        foreach (UnitInfo unit in allUnits)
        {
            if (unit.GetUnitType().Equals("player-static-mainbase"))
            {
                Debug.Log("Game State Controller - Found and tracking player main base, instance id " + unit.gameObject.GetInstanceID());
                _playerMainBase = unit.gameObject.GetComponent<PlanetaryEvacuation>();
                break;
            }
        }

        if (_playerMainBase == null)
        {
            Debug.LogError("no player main base, cannot start the game");
            return;
        }

        //then, store all units
        foreach (UnitInfo unit in allUnits)
        {
            //add to entity storage using unit creation controller
            Debug.Log("Game State Controller - Adding unit of type '" + unit.GetUnitType() + "' to entity storage, entity name: " + unit.gameObject.name);
            _unitCreationController.StoreCreatedEntity(unit.gameObject);
        }

        Debug.Log("Game State Controller - Tracking all enemy spawners");
        _enemySpawners = new List<EnemySpawner>();
        //get the list of enemy spawners
        List<GameObject> enemySpawnerObjects = _entityStorage.GetPlayerUnitsOfType("enemy-static-spawner");
        foreach(GameObject enemySpawnerObject in enemySpawnerObjects)
        {
            EnemySpawner enemySpawner = enemySpawnerObject.GetComponent<EnemySpawner>();

            //try activating spawners with heat level 0
            enemySpawner.AttemptActivation(0);

            _enemySpawners.Add(enemySpawner);
        }
        Debug.Log("Game State Controller - Found " + _enemySpawners.Count + " spawners.");

        Debug.Log("Game State Controller - Initialized entity storage with " + allUnits.Length + " units.");
    }

    //update loop functionality goes here
    void Update()
    {
        //if player main base was destroyed
        if (_playerMainBase == null)
        {
            //inform that the player sucks and should lose
            EndOfGameEvent.Invoke(false);
        }
        if (FindObjectOfType<Timetracker>().CurTime % GetComponent<Sun>().GetDamage() ==1)
        {
            _entityStorage.deal_damage();
           
        }
    }

    //other helpers go here

    //for handling a request for a location by a specific unit
    public void HandlePositionRequest(string requestType, GameObject requestingUnit)
    {
        AIControl requestingAI = requestingUnit.GetComponent<AIControl>();

        Vector3 requestedPosition = new Vector3();

        switch (requestType)
        {
            case "mainBase":
                //just get the main base position if it exists (it should)
                if(_playerMainBase == null)
                {
                    return;
                }

                requestedPosition = _playerMainBase.gameObject.transform.position;

                break;
            default:
                Debug.LogError("Invalid position request type: " + requestType);
                return;
        }

        //notify the unit's AI of the position it wanted
        requestingAI.SendPositionNotification(requestedPosition);
    }

    //move evacuated civilians to main base
    public void EvacuateCivilians(int numCivilians)
    {
        _playerMainBase.AddCivies(numCivilians);
    }

    //store newly harvested resources
    public void StoreHarvestedResource(string type, int amount)
    {
        switch (type)
        {
            case "minerals":
                _gameStateModel.AddPlayerMinerals(amount);
                break;
            case "fuel":
                _gameStateModel.AddPlayerFuel(amount);
                break;
            case "research points":
                _gameStateModel.AddPlayerRP(amount);
                break;
            default:
                Debug.LogError("Unknown resource type: " + type + " cannot add.");
                break;
        }
    }

    //determine if the player can afford to build a unit of the specified type
    public bool CanAffordUnit(string unitType)
    {
        //should move getting available resources to model class...
        List<string> availableResources = new List<string>() { "minerals", "fuel" };

        //check each supported resource to see if the player has enough of it to purchase the unit.
        foreach (string resource in availableResources)
        {
            int playerStockpile = GetPlayerResource(resource);
            int unitCost = _unitDb.GetUnitCost(unitType, resource);

            if (playerStockpile < unitCost)
            {
                GetComponent<DisplayInfoController>().AddDialogue("Player has insufficient: " + resource + ", they have " + playerStockpile + " and the unit requires " + unitCost);
                return false;
            }
        }
        return true;
    }

    //deduct resources for the cost of some unit from the player's storage
    public void PurchaseUnit(string unitType)
    {
        /*Debug.Log*/
        GetComponent<DisplayInfoController>().AddDialogue("mineral cost deducted: " + _unitDb.GetUnitCost(unitType, "minerals") + "fuel deducted: " + _unitDb.GetUnitCost(unitType, "fuel"));
        _gameStateModel.SubtractPlayerMinerals(_unitDb.GetUnitCost(unitType, "minerals"));
        _gameStateModel.SubtractPlayerFuel(_unitDb.GetUnitCost(unitType, "fuel"));
    }

    //get player resources
    public int GetPlayerResource(string type)
    {
        switch (type)
        {
            case "minerals":
                return _gameStateModel.GetPlayerMinerals();
            case "fuel":
                return _gameStateModel.GetPlayerFuel();
            case "research points":
                return _gameStateModel.GetPlayerRP();
            default:
                Debug.LogError("Unknown resource type: " + type + " cannot get.");
                break;
        }

        return -1;
    }

    /* technologies */

    //return true if tech is researchable
    public bool IsTechResearchable(string techId)
    {
        int rp = GetPlayerResource("research points");

        return _researchModel.CanTechBeResearched(techId, rp);
    }

    //unlock technology with given technology ID
    public bool ResearchTechnology(string techId)
    {
        //this check was not here when we did the demo. good lord I'm stupid.
        if (!IsTechResearchable(techId))
        {
            return false;
        }

        //deduct the research point cost
        _gameStateModel.SubtractPlayerRP(_researchModel.GetTechnologyCost(techId));

        return _researchModel.CompleteTechnology(techId);
    }

    /* other helpers */

    //returns the player's main base
    public GameObject GetMainBase()
    {
        return _playerMainBase.gameObject;
    }

    public int GetEvacuatedCivs()
    {
        return _playerMainBase.GetNumCivies();
    }

}

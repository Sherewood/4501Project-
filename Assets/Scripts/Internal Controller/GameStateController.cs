using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
/* Internal Controller Class */
//Purpose: To handle game-state related matters such as player resources, elapsed time and win conditions

public class GameStateController : MonoBehaviour
{

    private EntityStorage _entityStorage;

    private UnitDatabase _unitDb;

    private UnitCreationController _unitCreationController;

    private GameStateModel _gameStateModel;

    private PlanetaryEvacuation _playerMainBase;

    void Start()
    {
        Debug.Log("Game State Controller - Begin game initialization");
        _unitCreationController = GetComponent<UnitCreationController>();
        _gameStateModel = FindObjectOfType<GameStateModel>();
        _entityStorage = FindObjectOfType<EntityStorage>();
        _unitDb = FindObjectOfType<UnitDatabase>();
        //get all units (each unit must have a Unit Info component)
        UnitInfo[] allUnits = FindObjectsOfType<UnitInfo>();

        foreach (UnitInfo unit in allUnits)
        {
            //add to entity storage using unit creation controller
            Debug.Log("Adding unit of type '" + unit.GetUnitType() + "' to entity storage");
            _unitCreationController.StoreCreatedEntity(unit.gameObject);
        }

        //get the player main base
        //could 100% do in the previous for loop but the efficiency doesn't really matter since its during initialization...
        if (_playerMainBase == null)
        {
            GameObject mainBaseRef = _entityStorage.GetPlayerUnitsOfType("player-static-mainbase")[0];
            Debug.Log("Found and tracking player main base, instance id " + mainBaseRef.GetInstanceID());
            _playerMainBase = mainBaseRef.GetComponent<PlanetaryEvacuation>();
        }

        Debug.Log("Initialized entity storage with " + allUnits.Length + " units.");
    }

    //update loop functionality goes here
    void Update()
    {
    }

    //other helpers go here

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
                Debug.Log("Player has insufficient: " + resource + ", they have " + playerStockpile + " and the unit requires " + unitCost);
                return false;
            }
        }
        return true;
    }

    //deduct resources for the cost of some unit from the player's storage
    public void PurchaseUnit(string unitType)
    {
        Debug.Log("mineral cost deducted: " + _unitDb.GetUnitCost(unitType, "minerals") + "fuel deducted: " + _unitDb.GetUnitCost(unitType, "fuel"));
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

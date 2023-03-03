using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
/* Internal Controller Class */
//Purpose: To handle game-state related matters such as player resources, elapsed time and win conditions

public class GameStateController : MonoBehaviour
{

    private EntityStorage _entityStorage;

    private UnitCreationController _unitCreationController;

    private GameStateModel _gameStateModel;

    void Start()
    {
        Debug.Log("Game State Controller - Begin game initialization");
        _unitCreationController = GetComponent<UnitCreationController>();
        _gameStateModel = FindObjectOfType<GameStateModel>();
        //get all units (each unit must have a Unit Info component)
        UnitInfo[] allUnits = FindObjectsOfType<UnitInfo>();
        
        foreach (UnitInfo unit in allUnits)
        {
            //add to entity storage using unit creation controller
            Debug.Log("Adding unit of type '" + unit.GetUnitType() + "' to entity storage");
            _unitCreationController.StoreCreatedEntity(unit.gameObject);
        }

        Debug.Log("Initialized entity storage with " + allUnits.Length + " units.");
    }

    //update loop functionality goes here
    void Update()
    {
    }

    //other helpers go here

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
            default:
                Debug.LogError("Unknown resource type: " + type + " cannot add.");
                break;
        }
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
            default:
                Debug.LogError("Unknown resource type: " + type + " cannot get.");
                break;
        }

        return -1;
    }

}

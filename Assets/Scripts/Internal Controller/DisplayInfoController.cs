using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/* Internal Controller */

//Purpose: Providing information to the UI upon request
//Originally intended to push information to the UI controller, but will instead provide interfaces for the UI controller to access

public class DisplayInfoController : MonoBehaviour
{

    private SelectionController _selectionController;

    private GameStateController _gameStateController;

    private UnitDatabase _unitDatabase;

    // Start is called before the first frame update
    void Start()
    {
        _selectionController = GetComponent<SelectionController>();
        _gameStateController = GetComponent<GameStateController>();

        _unitDatabase = FindObjectOfType<UnitDatabase>();
    }

    /* Helpers (to be called directly by the UI) */

    //return the list of selected units
    public List<GameObject> GetSelectedUnits()
    {
        return _selectionController.GetSelectedUnits();
    }

    //return the list of actions available to the selected units
    public List<string> GetSelectedUnitActions()
    {
        List<Capability> capabilities = _selectionController.GetSelectedUnitCapabilities();

        List<string> actions = new List<string>();

        foreach(Capability c in capabilities)
        {
            actions.Add(c.ActionName);
        }

        return actions;
    }

    //given a list of resource types (minerals, fuel), returns a dictionary mapping resource types to the number of resources the player has
    public Dictionary<string, int> GetPlayerResources(List<string> resourceTypes)
    {
        Dictionary<string, int> resourceAmounts = new Dictionary<string, int>();

        foreach (string resourceType in resourceTypes)
        {
            int resourceAmount = _gameStateController.GetPlayerResource(resourceType);
            resourceAmounts.Add(resourceType, resourceAmount);
        }

        return resourceAmounts;
    }

    //given a unit type, returns the sprite associated with that unit type
    public Sprite GetUnitIcon(string unitType)
    {
        return _unitDatabase.GetUnitIcon(unitType);
    }

    //future todo: for MenuSelectionEvent, will need helper method that only returns a list of values when a certain state is set
    //for example: if build unit button clicked, this list of values would become the list of unit types that can be built at the selected barracks/factory


}

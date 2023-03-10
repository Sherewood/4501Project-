using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/* Data type */
//Purpose: To represent an action a unit can take

public class Capability
{
    //name of the action
    public string ActionName;
    //list of actions that make this action unavailable when they are available
    public List<string> IncompatibleActions;
    //description of the capability (for UI purposes)
    public string Description;
    //whether the capability is available when multiple units are selected
    public bool MultiUnit;
    //tech requirements for the capability
    public List<string> TechRequirements;

    public Capability(string actionName, List<string> incompatibleActions, string description,
        bool multiUnit, List<string> techRequirements)
    {
        ActionName = actionName;
        IncompatibleActions = incompatibleActions;
        Description = description;
        MultiUnit = multiUnit;
        TechRequirements = techRequirements;
    }
}

/* Model class */
//Purpose: To store mappings between components of game objects and capabilities

public class CapabilityModel : MonoBehaviour
{

    //(componentName, Capability) mappings
    private Dictionary<string, List<Capability>> _capabilityMappings;

    void Awake()
    {
        _capabilityMappings = new Dictionary<string, List<Capability>>();
        //define mappings

        //movement component -> move capability
        CreateCapabilityMapping("movement", "move", new List<string>(), "Unit can move!", true, new List<string>());
        //attack component -> attack, guard capabilities
        CreateCapabilityMapping("attack", "attack", new List<string>(), "Unit can attack!", true, new List<string>());
        CreateCapabilityMapping("attack", "guard", new List<string>(), "Unit can guard its position!", true, new List<string>());
        //movement + health component -> fortify capability
        CreateCapabilityMapping("movement-health", "fortify", new List<string>(), "Unit can move!", true, new List<string>());
        //harvester component -> harvest capability
        CreateCapabilityMapping("harvester", "harvest", new List<string>(), "Unit can harvest a resource!", true, new List<string>());
        //construction component -> construct capability
        CreateCapabilityMapping("construction", "construct", new List<string>(), "Unit can construct a building!", true, new List<string>());
        //civilian component -> evacuation capability
        CreateCapabilityMapping("civilian", "evacuateCivies", new List<string>(), "Unit can evacuate civilians!", false, new List<string>());
        //planetary evacuation component -> evacuate main base capability
        CreateCapabilityMapping("planetaryEvac", "evacuateMainBase", new List<string>(), "Unit can evacuate planet!", false, new List<string>());
    }

    void CreateCapabilityMapping(string componentName, string actionName, List<string> incompatibleActions, string description, bool multiUnit, List<string> techRequirements)
    {
        Capability capability = new Capability(actionName, incompatibleActions, description, multiUnit, techRequirements);

        if (_capabilityMappings.ContainsKey(componentName))
        {
            _capabilityMappings[componentName].Add(capability);
        }
        else
        {
            List<Capability> newCapList = new List<Capability>();
            newCapList.Add(capability);
            _capabilityMappings.Add(componentName, newCapList);
        }
    }

    //returns the capabilities available to a specific unit
    public List<Capability> GetCapabilitiesOfUnit(GameObject entity)
    {
        List<Capability> unitCapabilities = new List<Capability>();
        //get the unit info component
        UnitInfo unitInfo = entity.GetComponent<UnitInfo>();

        foreach(string componentKey in _capabilityMappings.Keys)
        {
            //split by '-' to account for capabilities that require multiple components
            bool meetsRequirements = true;
            string[] compNameSplit = componentKey.Split('-');
            foreach (string componentName in compNameSplit)
            {
                if (!unitInfo.DoesUnitHaveComponent(componentName))
                {
                    meetsRequirements = false;
                    break;
                }
            }

            //if all component prereqs are met, include the associated capabilities
            if (meetsRequirements)
            {
                foreach (Capability capability in _capabilityMappings[componentKey])
                {
                    Debug.Log(capability.ActionName);
                    unitCapabilities.Add(capability);
                }
            }
        }

        return unitCapabilities;
    }
}

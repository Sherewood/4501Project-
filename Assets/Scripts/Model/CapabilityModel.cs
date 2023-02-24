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
    private Dictionary<string, Capability> _capabilityMappings;

    void Awake()
    {
        _capabilityMappings = new Dictionary<string, Capability>();

        //create capabilities to be mapped
        Capability testCapability = new Capability("test", new List<string>(), "this is a test", true, new List<string>());

        //define mappings
        
        //temporary mapping for testing the capability controller
        _capabilityMappings.Add("unitInfo", testCapability);
    }

    //returns the capabilities available to a specific unit
    public List<Capability> GetCapabilitiesOfObject(GameObject entity)
    {
        List<Capability> unitCapabilities = new List<Capability>();
        //get the unit info component
        UnitInfo unitInfo = entity.GetComponent<UnitInfo>();

        foreach(string componentName in _capabilityMappings.Keys)
        {
            if (unitInfo.DoesUnitHaveComponent(componentName))
            {
                unitCapabilities.Add(_capabilityMappings[componentName]);
            }
        }

        return unitCapabilities;
    }
}

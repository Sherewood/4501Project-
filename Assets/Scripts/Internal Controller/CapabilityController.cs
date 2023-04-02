using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/* Internal Controller Class */
//Purpose: To deal with capabilities of units

public class CapabilityController : MonoBehaviour
{

    private CapabilityModel _capabilityModel;

    private ResearchModel _researchModel;

    // Start is called before the first frame update
    void Start()
    {
        _capabilityModel = FindObjectOfType<CapabilityModel>();

        _researchModel = FindObjectOfType<ResearchModel>();
    }

    public List<Capability> GetCapabilitiesOfUnit(GameObject unit)
    {
        List<Capability> possibleCapabilities = _capabilityModel.GetCapabilitiesOfUnit(unit);

        List<Capability> actualCapabilities = new List<Capability>();

        //check tech requirements
        actualCapabilities = CheckTechRequirements(possibleCapabilities);

        //check if any capabilities are incompatible due to other capabilities
        actualCapabilities = ResolveCapabilityConflicts(actualCapabilities);

        return actualCapabilities;
    }

    public List<Capability> GetCapabilitiesOfUnits(List<GameObject> units)
    {
        //get all the unique capabilities available to any units in the selection
        List<Capability> totalCapabilities = new List<Capability>();

        //track if multiple units are selected, in order to hide single unit only capabilities
        bool multipleUnitsSelected = units.Count > 1;

        foreach (GameObject unit in units)
        {
            //todo - add parameter to indicate multi-unit capabilities should not be included if more than 1 unit selected
            List<Capability> possibleCapabilities = _capabilityModel.GetCapabilitiesOfUnit(unit);
        
            foreach(Capability capability in possibleCapabilities)
            {
                //skip if multiple units are selected and the capability is not available when multiple units are selected
                if(!capability.MultiUnit && multipleUnitsSelected)
                {
                    continue;
                }

                //only add if not already included in capability list
                if (!totalCapabilities.Contains(capability))
                {
                    totalCapabilities.Add(capability);
                }
            }
        }
        List<Capability> actualCapabilities = new List<Capability>();

        //check tech requirements
        actualCapabilities = CheckTechRequirements(totalCapabilities);

        //check if any capabilities are incompatible due to other capabilities
        actualCapabilities = ResolveCapabilityConflicts(actualCapabilities);

        return actualCapabilities;
    }

    private List<Capability> CheckTechRequirements(List<Capability> possibleCapabilities)
    {
        List<Capability> availableCapabilities = new List<Capability>();

        //check if all of the techs required for the capability are researched
        foreach (Capability capability in possibleCapabilities)
        {
            bool hasTechPrereqs = true;

            foreach(string techRequirement in capability.TechRequirements)
            {
                if (!_researchModel.IsTechResearched(techRequirement))
                {
                    Debug.Log("TEST " + techRequirement);
                    hasTechPrereqs = false;
                    break;
                }
            }

            if (hasTechPrereqs)
            {
                availableCapabilities.Add(capability);
            }
        }

        return availableCapabilities;
    }

    private List<Capability> ResolveCapabilityConflicts(List<Capability> possibleCapabilities)
    {
        List<Capability> compatibleCapabilities = new List<Capability>();

        //check if any capabilities are incompatible due to other capabilities
        foreach (Capability capability in possibleCapabilities)
        {
            bool validAction = true;
            foreach (Capability comparedCapability in possibleCapabilities)
            {
                if (comparedCapability == capability)
                {
                    continue;
                }

                if (capability.IncompatibleActions.Contains(comparedCapability.ActionName))
                {
                    validAction = false;
                }
            }

            if (validAction)
            {
                compatibleCapabilities.Add(capability);
            }
        }

        return compatibleCapabilities;
    }


}

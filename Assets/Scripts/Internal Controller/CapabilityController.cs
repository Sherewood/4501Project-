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
        //only consider capabilities of player units
        if (!unit.GetComponent<UnitInfo>().GetAllegiance().Equals("player"))
        {
            return new List<Capability>();
        }

        List<Capability> possibleCapabilities = _capabilityModel.GetCapabilitiesOfUnit(unit);

        List<Capability> actualCapabilities = new List<Capability>();

        //check tech requirements
        actualCapabilities = CheckTechRequirements(possibleCapabilities);

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
            //if not a player unit, ignore its capabilities
            if (!unit.GetComponent<UnitInfo>().GetAllegiance().Equals("player"))
            {
                continue;
            }

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
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/* Internal Controller Class */
//Purpose: To deal with capabilities of units

public class CapabilityController : MonoBehaviour
{

    private CapabilityModel _capabilityModel;

    // Start is called before the first frame update
    void Start()
    {
        _capabilityModel = FindObjectOfType<CapabilityModel>();
    }

    public List<Capability> GetCapabilitiesOfUnit(GameObject unit)
    {
        List<Capability> possibleCapabilities = _capabilityModel.GetCapabilitiesOfUnit(unit);

        List<Capability> actualCapabilities = new List<Capability>();

        //check tech requirements (to be implemented later)

        //check if any capabilities are incompatible due to other capabilities
        foreach(Capability capability in possibleCapabilities)
        {
            bool validAction = true;
            foreach(Capability comparedCapability in possibleCapabilities)
            {
                if(comparedCapability == capability)
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
                actualCapabilities.Add(capability);
            }
        }

        return actualCapabilities;
    }


}

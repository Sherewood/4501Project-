using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/* Unit Component */

//Purpose: Manage decision making for the unit
//fulfills the "think-act" portion of "sense-think-act", will leave sensing to the Targeting component
public class AIControl : MonoBehaviour
{

    //potential issue: adding the asset here will lead to the contents of the text asset being read out every
    //time a unit is created. If this provides significant overhead, should move parsing to Unit Database
    [Tooltip("The rule data to be used by the AI's rule-based system.")]
    public TextAsset RuleFile;

    //provides the list of command-based rules (only followed if command was issued)
    private List<int> _commandBasedRules;

    //provides the list of rules followed when the unit isn't following a command.
    private List<int> _autoRules;

    //stores prerequisites for each rule
    private Dictionary<int, List<List<string>>> _prereqs;

    //stores actions to be taken for each rule.
    private Dictionary<int, List<string>> _actions;
     
    void Awake()
    {
        _commandBasedRules = new List<int>();
        _autoRules = new List<int>();
        _prereqs = new Dictionary<int, List<List<string>>>();
        _actions = new Dictionary<int, List<string>>();
    }
}

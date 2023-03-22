using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/* Unit Component */

//Purpose: Manage decision making for the unit
//fulfills the "think-act" portion of "sense-think-act", will leave sensing to the Targeting component
public class AIControl : MonoBehaviour
{
    [Tooltip("Enable for extra troubleshooting logs. Recommended to only do for single unit at a time to prevent log flooding.")]
    public bool DebugMode = false;

    //potential issue: adding the asset here will lead to the contents of the text asset being read out every
    //time a unit is created. If this provides significant overhead, should move parsing to Unit Database
    [Tooltip("The rule data to be used by the AI's rule-based system.")]
    public TextAsset RuleFile;

    //provides the list of command-based rules (only followed if command was issued)
    private List<int> _commandBasedRules;

    //provides the list of rules followed when the unit isn't following a command.
    private List<int> _autoRules;

    //stores prerequisites for each rule
    private Dictionary<int, List<string[]>> _prereqs;

    //stores actions to be taken for each rule.
    private Dictionary<int, List<string>> _actions;
     
    void Awake()
    {
        _commandBasedRules = new List<int>();
        _autoRules = new List<int>();
        _prereqs = new Dictionary<int, List<string[]>>();
        _actions = new Dictionary<int, List<string>>();

        if (!InitRBS())
        {
            Debug.LogError("Failed to initialize rule-based system. File name: " + RuleFile.name);
            return;
        }
    }

    //initialize the rule based system
    private bool InitRBS()
    {
        string[] rbsText = RuleFile.ToString().Split('\n');

        int ruleId = 0;

        //parse each line
        foreach(string rbsLine in rbsText)
        {
            bool commandRule = false;
            //local debug mode - debugging for the rule on this line alone
            bool localDebugMode = false;

            string parsedRbsLine = rbsLine;
            //skip comments/empty lines
            if (rbsLine.StartsWith('#') || rbsLine.Length == 0 || rbsLine.StartsWith(' '))
            {
                if (DebugMode)
                {
                    Debug.Log("Skipped comment: " + rbsLine);
                }
                continue;
            }

            string[] splitRbsLine = new string[] {};
            //parse flags if they exist
            if (parsedRbsLine.Contains("|"))
            {
                //yeah this is going to be inefficient lmao
                splitRbsLine = parsedRbsLine.Split("|");
                string[] flags = splitRbsLine[0].Split(",");
                //right side of | is the rest of the rule
                parsedRbsLine = splitRbsLine[1];

                foreach(string flag in flags)
                {
                    if (DebugMode || localDebugMode)
                    {
                        Debug.Log("Flag for rule " + ruleId + ": " + flag);
                    }
                    if(flag == "CM")
                    {
                        commandRule = true;
                    }
                    else if(flag == "D")
                    {
                        Debug.Log("Local debug mode enabled for rule " + ruleId + ". Note this only applies to parsing for now.");
                        localDebugMode = true;
                    }
                }
            }

            //split rule into prereqs and actions
            splitRbsLine = parsedRbsLine.Split(" then ");
            if(splitRbsLine.Length != 2)
            {
                Debug.LogError("Error in RBS parsing -> prereqs/actions not split by ' then '");
                Debug.LogError("Affected line: " + splitRbsLine);
                return false;
            }
            string prereqs = splitRbsLine[0];
            string actions = splitRbsLine[1];
            _prereqs.Add(ruleId, new List<string[]>());
            _actions.Add(ruleId, new List<string>());

            if (commandRule)
            {
                _commandBasedRules.Add(ruleId);
            }
            else
            {
                _autoRules.Add(ruleId);
            }

            //parse prereqs

            //remove 'If ' at start
            prereqs = prereqs.Substring(3);

            //todo: more robust system for ands and ors
            //for now: split by or, then by and
            string[] prereqsSplitByOr = prereqs.Split(" or ");
            if(prereqsSplitByOr.Length == 0)
            {
                Debug.LogError("Error in RBS parsing -> no prereqs given for rule.");
                Debug.LogError("Affected line: " + rbsLine);
                return false;
            }
            foreach(string prereqSplitByOr in prereqsSplitByOr)
            {
                if (DebugMode || localDebugMode)
                {
                    Debug.Log("Prereq split by or for rule " + ruleId + ": " + prereqSplitByOr);
                }
                string[] prereqsSplitByAnd = prereqSplitByOr.Split(" and ");
                _prereqs[ruleId].Add(prereqsSplitByAnd);
            }

            //parse actions
            //only and statements allowed
            if(actions.Contains(" or "))
            {
                Debug.LogError("Error in RBS parsing -> no or statements allowed for actions");
                Debug.LogError("Affected line: " + rbsLine);
                return false;
            }
            string[] actionsSplitByAnd = actions.Split(" and ");
            if(actionsSplitByAnd.Length == 0)
            {
                Debug.LogError("Error in RBS parsing -> no actions given for rule.");
                Debug.LogError("Affected line: " + rbsLine);
                return false;
            }
            foreach(string actionSplitByAnd in actionsSplitByAnd)
            {
                if (DebugMode || localDebugMode)
                {
                    Debug.Log("Action for rule " + ruleId + ": " + actionSplitByAnd);
                }
                _actions[ruleId].Add(actionSplitByAnd);
            }

            //increment rule id for next rule
            ruleId += 1;
        }

        if (DebugMode)
        {
            Debug.Log("Successfully parsed rule file: " + RuleFile.name);
        }

        return true;
    }
}

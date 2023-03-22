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

    //command given to the unit by higher authority
    private string _command;
    //variables to hold information necessary for initial handling of the command
    private Vector3 _commandTargetPosition;
    private GameObject _commandTarget;

    /* unit components */
    private UnitState _unitState;
     
    void Awake()
    {
        _commandBasedRules = new List<int>();
        _autoRules = new List<int>();
        _prereqs = new Dictionary<int, List<string[]>>();
        _actions = new Dictionary<int, List<string>>();

        _command = "";
        _commandTargetPosition = transform.position;
        _commandTarget = null;

        if (!InitRBS())
        {
            Debug.LogError("Failed to initialize rule-based system. File name: " + RuleFile.name);
            return;
        }

        _unitState = GetComponent<UnitState>();
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

    /* command handling */
    /* multiple methods needed to support multiple different types of command
    would prefer to set the handling for these commands using the rule-based system alone,
    but need to support different types of input... 

    Solution: do 'pre-processing' for some commands based on input, then let rule handle the rest?
    */
    //no-input command
    public void SendCommand(string command)
    {
        if (DebugMode)
        {
            Debug.Log("Received command: " + command);
        }
        _command = command;
        //handle 'new command' AI event
        HandleAIEvent("newCommand");
    }

    public void SendCommand(string command, Vector3 targetPos)
    {
        if (DebugMode)
        {
            Debug.Log("Received command: " + command);
        }
        _command = command;
        //set "target position" to be used for initial command handling
        _commandTargetPosition = targetPos;

        HandleAIEvent("newCommand");
    }

    public void SendCommand(string command, GameObject target)
    {
        if (DebugMode)
        {
            Debug.Log("Received command: " + command);
        }
        _command = command;
        _commandTarget = target;

        HandleAIEvent("newCommand");
    }

    /* event handling */
    //handle callback meant to influence AI decision making
    public void HandleAIEvent(string aiEvent)
    {
        if (DebugMode)
        {
            Debug.Log("Handling AI event: " + aiEvent);
        }

        //choose the set of rules to check based on whether a command is being processed
        List<int> chosenRules = (!_command.Equals("")) ? _commandBasedRules : _autoRules;

        List<int> validRules = new List<int>();

        //determine all valid rules
        foreach (int ruleId in chosenRules)
        {
            if(CheckIfRuleValid(ruleId, aiEvent))
            {
                validRules.Add(ruleId);
            }
        }

        //lazy tiebreaker : take the first one lmao
        //may or may not need to revisit later
        int chosenRule = validRules[0];

        //perform the rule's actions.
        PerformActionsForRule(chosenRule);
    }

    /* prerequisite handling */

    //check if the rule's prerequisites are valid
    private bool CheckIfRuleValid(int ruleId, string aiEvent)
    {
        if (DebugMode)
        {
            Debug.Log("Checking if rule: " + ruleId + " is valid for aiEvent: " + aiEvent);
        }

        bool ruleSatisfied = false;

        foreach (string[] prereqSet in _prereqs[ruleId])
        {
            //each prereq set is an and statement for all prereqs in the set
            //therefore, set is only satisfied if all prereqs in it are true
            bool prereqSetSatisfied = true;
            
            foreach(string prereq in prereqSet)
            {
                if (!IsPrereqSatisfied(prereq, aiEvent))
                {
                    prereqSetSatisfied = false;
                    break;
                }
            }

            if (prereqSetSatisfied)
            {
                ruleSatisfied = true;
                break;
            }
        }

        return ruleSatisfied;
    }



    //check if prereq satisfied
    //use the indicated AI event aswell as certain prereqs are satisfied immediately if they match it
    private bool IsPrereqSatisfied(string prereq, string aiEvent)
    {
        if(prereq.Contains("==") || prereq.Contains("!="))
        {
            return IsEqualityPrereqSatisfied(prereq);
        }

        if (DebugMode)
        {
            Debug.Log("Checking if prereq: " + prereq + " is satisfied for aiEvent: " + aiEvent);
        }

        switch (prereq)
        {
            case "targetChanged":
                return (prereq.Equals(aiEvent));
            case "targetLost":
                return (prereq.Equals(aiEvent));
            case "reachedDestination":
                return (prereq.Equals(aiEvent));
            //todo: range checking prereqs should ask attack component if aiEvent doesn't match
            //this is because some rules will check range in addition to target being changed/etc.
            case "targetNotInRange":
                return (prereq.Equals(aiEvent));
            case "targetInRange":
                return (prereq.Equals(aiEvent));
            //todo: add support for other prereqs
            default:
                Debug.LogError("Unsupported prereq: " + prereq);
                return false;
        }
    }

    //check if prereq that relies on checking if something is equal to x is true
    private bool IsEqualityPrereqSatisfied(string equalityPrereq)
    {
        //get the comparison type (could add more later if needed)
        string equalityCheckType = equalityPrereq.Contains("==") ? "==" : "!=";

        //split into type and value
        string[] splitPrereq = equalityPrereq.Split(equalityCheckType);
        string type = splitPrereq[0];
        string value = splitPrereq[1];

        if (DebugMode)
        {
            Debug.Log("Checking if type: " + type + " is " + (equalityPrereq.Equals("==") ? "equal to" : "not equal to") + " value: " + value);
        }

        switch (type)
        {
            case "state":
                //check if unit state matches or doesn't match
                bool doesUnitStateMatch = _unitState.IsState(_unitState.StringToUState(value));
                return equalityCheckType.Equals("==") ? doesUnitStateMatch : !doesUnitStateMatch;
            case "command":
                //check if command matches the given value
                return _command.Equals(value);
            default:
                Debug.LogError("Unsupported equality check type: " + type);
                return false;
        }
    }

    /* action handling */
    private void PerformActionsForRule(int ruleId)
    {
        if (DebugMode)
        {
            Debug.Log("Performing actions for rule: " + ruleId);
        }

        List<string> actionList = _actions[ruleId];

        if (actionList == null)
        {
            foreach(string action in actionList)
            {
                PerformAction(action);
            }
        }
    }

    private void PerformAction(string action)
    {
        //handle equality actions separately
        if (action.Contains("="))
        {
            PerformSetAction(action);
            return;
        }

        if (DebugMode)
        {
            Debug.Log("Performing action: " + action);
        }

        //pretty much all actions are todo...
        switch (action)
        {
            case "doNothing":
                //yes
                break;
            case "moveTarget":
                //move towards the target
                break;
            case "attackTarget":
                //rotate towards the target while firing at it
                break;
            case "breakCommand":
                //clear command, re-enabling use of automatic rules + actions
                _command = "";
                break;
            case "initReturn":
                //move towards the return point
                break;
            default:
                Debug.LogError("Unsupported rule-based action: " + action);
                return;
        }
    }

    //perform action which involves setting a value.
    private void PerformSetAction(string setAction)
    {
        //split into type and value
        string[] splitAction = setAction.Split("=");
        string type = splitAction[0];
        string value = splitAction[1];

        if (DebugMode)
        {
            Debug.Log("Performing action to set type: " + type + " to value: " + value);
        }

        switch (type)
        {
            case "setState":
                _unitState.SetState(_unitState.StringToUState(value));
                break;
            default:
                Debug.LogError("Unsupported rule-based setting action: " + type);
                return;
        }
    }
}

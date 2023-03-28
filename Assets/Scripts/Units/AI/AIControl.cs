using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
/* Event callback */

//AI event - used to notify the AI control of things that should prompt a reaction from it.
[System.Serializable]
public class AIEvent : UnityEvent<string> { }



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
    public List<TextAsset> RuleFiles;

    //provides the list of command-based rules (only followed if command was issued)
    private List<int> _commandBasedRules;

    //provides the list of rules followed when the unit isn't following a command.
    private List<int> _autoRules;

    //stores prerequisites for each rule
    private Dictionary<int, List<string[]>> _prereqs;

    //stores actions to be taken for each rule.
    private Dictionary<int, List<string>> _actions;

    //command given to the unit by higher authority
    protected string _command;
    //variables to hold information necessary for initial handling of the command
    protected Vector3 _commandTargetPosition;
    protected GameObject _commandTarget;
    protected float _commandValue;

    /* unit components */
    protected UnitState _unitState;

    protected Movement _movement;

    protected Targeting _targeting;

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
            Debug.LogError("Failed to initialize rule-based system.");
            return;
        }

        GetComponents();
    }

    protected virtual void GetComponents()
    {
        _unitState = GetComponent<UnitState>();
        if(_unitState == null)
        {
            Debug.LogError("AI Control cannot find Unit State component");
        }
        _movement = GetComponent<Movement>();
        if(_movement == null)
        {
            Debug.LogError("AI Control cannot find Movement component");
        }

        //no null checking here because worker doesn't have targeting component
        //todo: consider moving targeting to CombatAIControl?
        _targeting = GetComponent<Targeting>();
    }

    //initialize the rule based system
    protected bool InitRBS()
    {
        List<string> ruleText = new List<string>();
        //note about parsing multiple files
        //1) might be more inefficient due to adding everything into list, but do not expect significant slowdown
        //2) No duplicate checking necessary, in the odd chance that a rule is duplicated, tiebreaking will handle it....
        foreach (TextAsset ruleFile in RuleFiles)
        {
            string[] rbsText = ruleFile.ToString().Split('\n');
            ruleText.AddRange(rbsText);
            if (DebugMode)
            {
                Debug.Log("Loaded in rule file: " + ruleFile.name);
            }
        }
        int ruleId = 0;

        //parse each line
        foreach(string rbsLine in ruleText)
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
                
                //hack: because actions are at end of the line, need to also remove this newline character...
                //should filter it earlier instead but whatever
                _actions[ruleId].Add(actionSplitByAnd.Replace("\r",""));
            }

            //increment rule id for next rule
            ruleId += 1;
        }

        if (DebugMode)
        {
            Debug.Log("Successfully parsed rule files");
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

    //included this to keep some of the returnToBase handling in UnitController (for getting the main base)
    //might decide to remove later? for now though its staying...
    public void SendCommand(string command, Vector3 targetPos, float value)
    {
        if (DebugMode)
        {
            Debug.Log("Received command: " + command);
        }
        _command = command;
        //set "target position" to be used for initial command handling
        _commandTargetPosition = targetPos;
        //set value to be used for initial command handling
        _commandValue = value;

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

    /* order a command to end */
    public void StopCommand(string command)
    {
        if (DebugMode)
        {
            Debug.Log("Received order to terminate command: " + command);
        }

        if (!_command.Equals(command))
        {
            Debug.LogWarning("Tried to stop command '" + command + "', but unit is currently following command '" + _command + "'!");
            return;
        }

        HandleAIEvent("stopCommand");
    }

    /* event handling */
    //handle callback meant to influence AI decision making
    public virtual void HandleAIEvent(string aiEvent)
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

        //cancel handling if no rules are valid
        if(validRules.Count == 0)
        {
            if (DebugMode)
            {
                Debug.Log("No valid rules for event, so take no action");
            }
            return;
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
    protected virtual bool IsPrereqSatisfied(string prereq, string aiEvent)
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
            case "reachedDestination":
                return (prereq.Equals(aiEvent));
            case "newCommand":
                return (prereq.Equals(aiEvent));
            case "stopCommand":
                return (prereq.Equals(aiEvent));
            default:
                Debug.LogError("Unsupported prereq: " + prereq);
                return false;
        }
    }

    //check if prereq that relies on checking if something is equal to x is true
    protected bool IsEqualityPrereqSatisfied(string equalityPrereq)
    {
        //get the comparison type (could add more later if needed)
        string equalityCheckType = equalityPrereq.Contains("==") ? "==" : "!=";
        //split into type and value
        string[] splitPrereq = equalityPrereq.Split(equalityCheckType);
        string type = splitPrereq[0];
        string value = splitPrereq[1];

        if (DebugMode)
        {
            Debug.Log("Checking if type: " + type + " is " + (equalityCheckType.Equals("==") ? "equal to" : "not equal to") + " value: " + value);
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

        foreach(string action in actionList)
        {
            PerformAction(action);
        }
    }

    protected virtual void PerformAction(string action)
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

        GameObject target = DetermineTarget();

        //pretty much all actions are todo...
        switch (action)
        {
            case "doNothing":
                //yes
                break;
            case "stopMovement":
                _movement.StopMovement();
                break;
            case "moveToDestination":
                //self explanatory
                _movement.MoveToDestination(_commandTargetPosition, MovementMode.MODE_PATHFINDING);
                //todo: refactor into separate method for setting moving state
                if (_unitState.GetState() != UState.STATE_ATTACKING && _unitState.GetState() != UState.STATE_GUARDING)
                {
                    _unitState.SetState(UState.STATE_MOVING);
                }
                break;
            case "breakCommand":
                //clear command, re-enabling use of automatic rules + actions
                _command = "";
                //if targeting is still focusing on a target, it should stop.
                if (_targeting != null)
                {
                    _targeting.StopTargetFocus();
                }
                break;
            case "setFlockLeader":
                _movement.SetFlockLeader(_commandTarget);
                break;
            case "moveInFlock":
                //destination is dummy value so unit does not stop moving unless given external command
                _movement.MoveToDestination(new Vector3(-1,-1,-1), MovementMode.MODE_PHYSICAL);
                break;
            case "initReturn":
                //move towards the return point
                break;
            case "returnToBase":
                //move towards the specified return point (base position), stop at the specified offset (command value)
                _movement.SetReturnPoint(_commandTargetPosition);
                _movement.MoveToReturnPoint(_commandValue, MovementMode.MODE_PATHFINDING);
                break;
            default:
                Debug.LogError("Unsupported rule-based action: " + action);
                return;
        }
    }

    protected GameObject DetermineTarget()
    {
        GameObject target = null;
        if (_command.Equals(""))
        {
            if(_targeting == null)
            {
                Debug.LogWarning("Unit's AI using an automatic behaviour that needs a target, but no targeting component is supported!");
                return null;
            }
            target = _targeting.GetTarget();
        }
        else if (_command.Equals("attack"))
        {
            target = _commandTarget;
        }

        return target;
    }

    //perform action which involves setting a value.
    protected void PerformSetAction(string setAction)
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

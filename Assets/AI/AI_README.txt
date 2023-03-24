#### AI README ####

The AI in this game uses a rule-based system to make decisions, with the rules provided by 1 of the specified files.

Rules have the following format:

If the rule uses one or more flags
<flags>| If <prerequisities> then <actions>

If the rule doesn't use flags
If <prerequisites> then <actions>

Flags:

CM - command rule: Indicates the rule is only considered when the unit is carrying out a command
D - debug mode: Indicates that debug prints should be made for anything which involves this rule. Currently only works for initialization (parsing)

Prerequisite Types:

single word: format '<prereq>' - will map to a single prerequisite check in the AI Control class used by the unit.
condition checker: format '<type><==|!=><value>' - will check if the type is equal or not equal to a value, according to the method defined in the AI control class used by the unit.

Action Types:

single word: Format '<action>' - will map to a single action in the AI Control class used by the unit.
setter action: Format '<type>=<value>' - will set the given type to the given value, according to the method defined in the AI Control class used by the unit.



Examples:

If targetChanged and targetInRange then setTarget and attackTarget

CM|If command==move and newCommand then moveToDestination

-If the prerequisites targetChanged and targetInRange are true, then the actions setTarget, followed by attackTarget are carried out.

List of AI Control classes:

-AIControl -> base class, currently only one available, will add more to pertain to specific types of units.


Extra notes:

If more than 1 rule is satisfied, the first rule in top-down order is the one chosen.

For command rules, the following is required

-Use the CM flag
-One of the prereqs should be command=<value>

For each unique command, the following rules should be included

-A rule with the prereqs command=<yourCommandValue> and newCommand, with newCommand denoting that this rule holds the first decision to take after a command is given. (Optional, but you'll probably want it)

-A rule including the prereq command=<yourCommandValue>, and the action breakCommand, to denote the rule which completes the command and resumes normal AI functionality. 

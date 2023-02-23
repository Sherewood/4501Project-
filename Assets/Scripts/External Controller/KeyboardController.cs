using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardController : MonoBehaviour
{

    //possible direction keys
    private List<string> _directionKeys;

    //keys that are pressed on a specific frame
    private List<string> _pressedKeys;

    //event for when key presses indicate a direction
    public DirectionKeyEvent DirectionKeyEvent;

    // Start is called before the first frame update
    void Start()
    {
        _directionKeys = new List<string>();

        _directionKeys.Add("up");
        _directionKeys.Add("left");
        _directionKeys.Add("right");
        _directionKeys.Add("down");

        _pressedKeys = new List<string>();
    }

    // Update is called once per frame
    void Update()
    {
        //check which keys are pressed
        foreach (string directionKey in _directionKeys)
        {
            if (Input.GetKey(directionKey))
            {
                _pressedKeys.Add(directionKey);
            }
        }

        //if any keys pressed, determine if a direction is being indicated
        if(_pressedKeys.Count > 0)
        {
            string direction = DetermineDirection();

            //if direction indicated, invoke event
            if(direction != "")
            {
                DirectionKeyEvent.Invoke(direction);
            }
        }

        _pressedKeys.Clear();
    }

    // determines if keys pressed indicate a direction, and returns them
    private string DetermineDirection()
    {
        string direction = "";

        //incredibly cursed series of if statements, can probably be refactored later....
        if (_pressedKeys.Contains("up"))
        {
            if (_pressedKeys.Contains("left"))
            {
                direction = "north-west";
            }
            else if (_pressedKeys.Contains("right"))
            {
                direction = "north-east";
            }
            else if (!_pressedKeys.Contains("down"))
            {
                direction = "north";
            }
        }
        else if (_pressedKeys.Contains("down"))
        {
            if (_pressedKeys.Contains("left"))
            {
                direction = "south-west";
            }
            else if (_pressedKeys.Contains("right"))
            {
                direction = "south-east";
            }
            else if (!_pressedKeys.Contains("up"))
            {
                direction = "south";
            }
        }
        else if (_pressedKeys.Contains("right"))
        {
            direction = "east";
        }
        else if (_pressedKeys.Contains("left"))
        {
            direction = "west";
        }

        return direction;
    }
}

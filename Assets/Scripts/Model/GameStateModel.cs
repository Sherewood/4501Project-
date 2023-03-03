using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateModel : MonoBehaviour
{

    /* Configuration */

    public int StartingMinerals;
    public int StartingFuel;

    /* private vars */

    private int _currentMinerals;
    private int _currentFuel;
    private int _currentScore;

    private int _heatLevel;

    void Awake()
    {
        _currentMinerals = StartingMinerals;
        _currentFuel = StartingFuel;
        _currentScore = 0;
        _heatLevel = 0;
    }

    /* player resources */

    //boilerplate crap
    public int GetPlayerMinerals()
    {
        return _currentMinerals;
    }
    public void AddPlayerMinerals(int minerals)
    {
        _currentMinerals += minerals;
    }
    public void SubtractPlayerMinerals(int minerals)
    {
        _currentMinerals -= minerals;
    }
    public int GetPlayerFuel()
    {
        return _currentFuel;
    }
    public void AddPlayerFuel(int fuel)
    {
        _currentFuel += fuel;
    }
    public void SubtractPlayerFuel(int fuel)
    {
        _currentFuel -= fuel;
    }
    //can player afford to pay the specified amount of resources?
    public bool CanPlayerAffordCost(int mineralCost, int fuelCost)
    {
        return (_currentMinerals >= mineralCost) && (_currentFuel >= fuelCost);
    }

    /* player score */

    //more boilerplate crap
    public int GetPlayerScore()
    {
        return _currentScore;
    }

    public void AddPlayerScore(int score)
    {
        _currentScore += score;
    }
    public void DeductPlayerScore(int score)
    {
        _currentScore -= score;
    }

    /* heat level */
    public int GetHeatLevel()
    {
        return _heatLevel;
    }
    public void IncrementHeatLevel()
    {
        _heatLevel++;
    }


    //todo: research

}

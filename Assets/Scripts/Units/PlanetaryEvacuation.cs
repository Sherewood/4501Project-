using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
/* Unit Component */
//Purpose: Facilitate planetary evacuation handling


public class PlanetaryEvacuation : MonoBehaviour
{
    /* Callbacks */
    EndOfGameEvent _endGameEvent;

    /* Configuration */
    [Tooltip("The minimum number of civilians required to carry out evacuation")]
    public int CivilianThreshold;

    [Tooltip("The minumum amount of fuel required to carry out evacuation")]
    public int FuelThreshold;

    [Tooltip("The amount of time it takes for evacuation to launch")]
    public float EvacuationLaunchTime;

    /* private vars */

    //number of civilians present
    private int _numCivies;

    private float _evacCountdown;

    private bool _evacStarted;


    // Start is called before the first frame update
    void Start()
    {
        _numCivies = 0;

        _evacCountdown = EvacuationLaunchTime;

        _evacStarted = false;

        _endGameEvent = new EndOfGameEvent();
    }

    public void AddCivies(int numCivies)
    {
        _numCivies += numCivies;
    }

    //boilerplate crap
    public int GetNumCivies()
    {
        return _numCivies;
    }

    public bool IsEvacInProgress()
    {
        return _evacStarted;
    }

    public float GetEvacuationTimeLeft()
    {
        return _evacCountdown;
    }

    //start planetary evacuation if able to
    public bool InitPlanetaryEvac(int fuelAmount)
    {
        if (!CanEvac(fuelAmount))
        {
            return false;
        }

        _evacStarted = true;

        Debug.Log("Planetary evacuation begun. Will complete in " + _evacCountdown + " seconds.");

        return true;
    }

    // Update is called once per frame
    void Update()
    {
        //count down to planetary evacuation if evac started.
        if (_evacStarted)
        {
            _evacCountdown -= Time.deltaTime;

            if(_evacCountdown < 0)
            {
                TriggerEvac();
            }
        }
    }

    //can only begin evacuation sequence if enough civies present and enough fuel stored.
    private bool CanEvac(int fuelAmount)
    {
        return (_numCivies >= CivilianThreshold) && (fuelAmount >= FuelThreshold);
    }

    //should trigger end of game
    private void TriggerEvac()
    {
        Debug.Log("Planetary evacuation initiated, trigger end of game event");
        Debug.Log("Civilians evacuated - " + _numCivies);
        _endGameEvent.Invoke(true);
    }

    public void ConfigureEndOfGameCallback(UnityAction<bool> endOfGameAction)
    {
        _endGameEvent.AddListener(endOfGameAction);
    }

}

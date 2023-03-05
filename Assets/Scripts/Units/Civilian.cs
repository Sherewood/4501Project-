using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
/* Unit Component */
//Purpose: Handles tracking of civilians in a building.

public class Civilian : MonoBehaviour
{
    /* Callbacks */
    private CivilianEvacEvent _civEvacEvent;


    /* Configuration */
    [Tooltip("The number of civilians stored here")]
    public int NumCivilians;

    [Tooltip("The evacuation rate of civilians, per second")]
    public float EvacRate;

    [Tooltip("The build rate of workers, per second")]
    public float WorkerBuildRate;

    /* private vars */
    private bool _evacActive;

    private int _workerCap;

    private UnitSpawner _spawner;

    private float _workerBuildCooldown;

    private float _evacCooldown;

    //going to set this rate for now
    private const int CIVIES_PER_WORKER = 40;

    //getting tired of seeing this constant redefined yet?
    private const float BASE_COOLDOWN = 1;

    List<GameObject> _workerList;

    // Start is called before the first frame update
    void Start()
    {
        _workerCap = NumCivilians / CIVIES_PER_WORKER;
        _evacActive = false;
        _workerList = new List<GameObject>();

        _evacCooldown = BASE_COOLDOWN / EvacRate;
        _workerBuildCooldown = BASE_COOLDOWN / WorkerBuildRate;

        _spawner = GetComponent<UnitSpawner>();

        _civEvacEvent = new CivilianEvacEvent();
    }

    //order beginning of evacuation
    public void TriggerEvacuation()
    {
        _evacActive = true;
    }

    //return true if civilian evacuation is in progress.
    public bool IsEvacuationInProgress()
    {
        return _evacActive;
    }

    // Update is called once per frame
    void Update()
    {
        //clear dead workers
        ClearDeletedWorkers();

        //update the worker cap
        CalcWorkerCap();
        
        //if under the worker spawn cap, try to spawn more workers
        if(_workerCap > _workerList.Count)
        {
            _workerBuildCooldown -= Time.deltaTime;

            //if ready, spawn worker
            if(_workerBuildCooldown <= 0)
            {
                GameObject worker = _spawner.SpawnUnit("player-dynamic-worker");

                _workerList.Add(worker);
                _workerBuildCooldown = BASE_COOLDOWN / WorkerBuildRate;
            }
        }

        //if evacuation is active, try to evacuate civilians.
        if (_evacActive)
        {
            _evacCooldown -= Time.deltaTime;

            //if ready, evacuate civilian
            if(_evacCooldown <= 0)
            {
                NumCivilians -= 1;

                _civEvacEvent.Invoke(1);

                _evacCooldown = BASE_COOLDOWN / EvacRate;
            }
        }
    }

    //deals with cleaning out workers that are dead
    //obviously not optimal efficiency, will have to revisit if causes performance issues
    private void ClearDeletedWorkers()
    {
        for(int i = 0; i < _workerList.Count; i++)
        {
            if (_workerList[i] == null)
            {
                _workerList.RemoveAt(i);
                i--;
            }
        }
    }

    private void CalcWorkerCap()
    {
        _workerCap = NumCivilians / CIVIES_PER_WORKER;
    }

    public void ConfigureCivilianEvacCallback(UnityAction<int> civEvacCallback)
    {
        _civEvacEvent.AddListener(civEvacCallback);
    }
}

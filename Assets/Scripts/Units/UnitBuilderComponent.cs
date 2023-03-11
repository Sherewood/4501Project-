using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitBuilderComponent : MonoBehaviour
{

    public List<string> _buildQueue;
    public List<float> _queueTimers;
    public List<string> _supportedUnitTypes;

    private UnitSpawner _spawner;

    // Start is called before the first frame update
    void Start()
    {
        _spawner = GetComponent<UnitSpawner>();
        _spawner.SetSpawnOffset(new Vector3(1,0,0));
        //_buildQueue.Add("player-dynamic-military-infantry");
        //_queueTimers.Add(10);
    }

    // Update is called once per frame
    void Update()
    {
        if (_buildQueue.Count > 0)
        {
            Debug.Log("stuff in queue");
            _queueTimers[0] -= Time.deltaTime;
            if(_queueTimers[0] < 0)
            {
                _spawner.SpawnUnit(_buildQueue[0]);
                _buildQueue.RemoveAt(0);
                _queueTimers.RemoveAt(0);
            }
        }
    }

    void addUnitToQueue(string unit, float buildTime)
    {
        _buildQueue.Add(unit);
        _queueTimers.Add(buildTime);
    }
}

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
        //going to rely on fixed offset instead.
        //_spawner.SetSpawnOffset(new Vector3(1,0,0));
    }

    // Update is called once per frame
    void Update()
    {
        if (_buildQueue.Count > 0)
        {
            _queueTimers[0] -= Time.deltaTime;
            if(_queueTimers[0] < 0)
            {
                GameObject result = _spawner.SpawnUnit(_buildQueue[0]);
                //if failed to spawn unit, then just wait and try again
                if(result == null)
                {
                    Debug.LogWarning("Purchased unit spawn is obstructed, trying again :(");
                    _queueTimers[0] = 2.0f;
                }
                
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/* Unit Component */

//Purpose: Add extra heat-level based control to spawner component for enemy spawners
public class EnemySpawner : UnitSpawner
{
    //threshold for heat level activation
    public int HeatLevelThreshold;

    private MeshRenderer _editorViewMesh;

    void Start()
    {
        _editorViewMesh = gameObject.GetComponentInChildren<MeshRenderer>();
        _editorViewMesh.enabled = false;
    }

    //activate the spawner if heat level is high enough
    public void AttemptActivation(int heatLevel)
    {
        if(heatLevel >= HeatLevelThreshold)
        {
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}

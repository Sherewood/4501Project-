using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//represents the game
public class SelectionIndicator : MonoBehaviour
{
    private GameObject _target;

    void Awake()
    {
        _target = null;
    }

    // Update is called once per frame
    void Update()
    {
        //just terminate if the target vanishes
        if(_target == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = _target.transform.position;
    }

    public void SetTarget(GameObject target)
    {
        _target = target;
    }

    public void ClearTarget()
    {
        _target = null;
    }
}

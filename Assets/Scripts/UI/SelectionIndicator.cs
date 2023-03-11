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
        //lazy ik...
        if(_target == null)
        {
            transform.position = new Vector3(0, -100, 0);
        }
        else
        {
            transform.position = _target.transform.position;
        }
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

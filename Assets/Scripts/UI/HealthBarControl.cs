using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Purpose: Control the unit's health bar
public class HealthBarControl : MonoBehaviour
{

    //the target unit
    private GameObject _target;
    //target unit's health component
    private Health _targetHealthComp;
    //mesh which contains health bar shader
    private MeshRenderer _healthBarMesh;

    void Awake()
    {
        _target = null;
        _healthBarMesh = gameObject.GetComponentInChildren<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        //should be assigned a target before it begins operating
        //if target is null, assume the assigned unit died so its time to dip
        if(_target == null)
        {
            Destroy(gameObject);
        }
        else
        {
            //keep health bar above its target unit
            Vector3 targetPos = _target.transform.position;
            float yOffset = _target.transform.localScale.y * 2.0f;
            if(yOffset > 2.5f)
            {
                yOffset = 2.5f;
            }
            targetPos.y += yOffset;
            transform.position = targetPos;

            //get health percentage
            float healthPct = _targetHealthComp.GetUnitHealth() / _targetHealthComp.MaxHealth;

            //update health percentage in shader
            _healthBarMesh.material.SetFloat("_HealthPct", healthPct);
        }
    }

    public void SetTarget(GameObject target)
    {
        _target = target;
        _targetHealthComp = _target.GetComponent<Health>();
        if(_targetHealthComp == null)
        {
            Debug.LogError("Assigned health bar to unit with no health.");
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//represents the selected unit
public class SelectionIndicator : MonoBehaviour
{
    private GameObject _target;

    private MeshRenderer _effectMesh;

    void Awake()
    {
        _target = null;

        _effectMesh = gameObject.GetComponentInChildren<MeshRenderer>();
    }

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

    public void SetColor(Vector3 color)
    {
        _effectMesh.material.SetVector("_Color",new Vector4(color.x,color.y,color.z,1.0f));
    }

    public void ClearTarget()
    {
        _target = null;
    }
}

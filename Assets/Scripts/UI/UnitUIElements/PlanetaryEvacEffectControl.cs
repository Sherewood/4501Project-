using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//for planetary evacuation countdown effect
public class PlanetaryEvacEffectControl : MonoBehaviour
{

    private MeshRenderer _effectMesh;

    //timer variables
    private float _currentTime;

    private float _completionTime;
    
    void Awake()
    {
        _effectMesh = gameObject.GetComponentInChildren<MeshRenderer>();

        _currentTime = 0.0f;
        _completionTime = 0.0f;
    }

    void Update()
    {
        if (_effectMesh.enabled)
        {
            _currentTime += Time.deltaTime;

            if (_currentTime > _completionTime)
            {
                _currentTime = _completionTime;
            }

            _effectMesh.material.SetFloat("_EffectTime", _currentTime);
        }
    }

    public void StartEffect(float completionTime)
    {
        _completionTime = completionTime;

        _effectMesh.enabled = true;

        _effectMesh.material.SetFloat("_EffectTime", 0.0f);
        _effectMesh.material.SetFloat("_CompletionTime", _completionTime);
    }
}

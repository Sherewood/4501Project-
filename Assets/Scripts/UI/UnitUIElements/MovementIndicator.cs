using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementIndicator : MonoBehaviour
{

    private float _time;

    private Terrain _terrain;

    private MeshRenderer _effectMesh;

    // Start is called before the first frame update
    void Start()
    {
        _time = 0.0f;
        _terrain = FindObjectOfType<Terrain>();

        transform.position = new Vector3(transform.position.x, transform.position.y + _terrain.SampleHeight(transform.position)+0.05f, transform.position.z);

        _effectMesh = gameObject.GetComponentInChildren<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        _time += Time.deltaTime;

        //update time
        _effectMesh.material.SetFloat("_EffectTime", _time);

        //destroy when effect finished after 1.1 seconds
        if(_time >= 1.1f)
        {
            Destroy(gameObject);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;


//Purpose: To adjust the intensity of the directional light source over time.
public class Sun : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject Time;
    public float DayLength;
    private float _rotationSpeed;
    public GameObject light;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        _rotationSpeed = Time.GetComponent<Timetracker>().CurTime / DayLength;
        
        //transform.Rotate(0, _rotationSpeed, 0);
        transform.Translate(0, Mathf.Sin(.5f), -Mathf.Cos(.5f));
        light.GetComponent<Light>().intensity += _rotationSpeed/1000;
        
    }
}

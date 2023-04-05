using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;


//Purpose: To adjust the intensity of the directional light source over time.
public class Sun : MonoBehaviour
{
    // Start is called before the first frame update
    // public Timetracker Time;
    public float updateInterval = 0.5F;
    public float intensity;
    public float Newintensity;
    private float damage;
    private double lastInterval;
    private int frames;
    private float fps;
    public float CurTime;
    private float modifier = 0.01f;
    private float _rotationSpeed;
    public GameObject light;
    public GameObject SolarObject;
  
    void Start()
    {
       
        intensity = 0;
        Newintensity = 0;
    }

    // Update is called once per frame
    void Update()
    {

        if (SolarObject != null)
        {
            SolarObject.transform.Translate(0, Mathf.Sin(.5f), -Mathf.Cos(.5f));
        }
        if ( FindObjectOfType<Timetracker>().CurTime %30.0f == 0)
        {
            intensify();
        }


        
    }
    public void intensify()
    {
        
        light.GetComponent<Light>().intensity+= modifier;
       
        damage = intensity;
        Newintensity = light.GetComponent<Light>().intensity;
    }
    public float GetDamage()
    {
        return damage;
    }
    public bool HeatRises()
    {
        if (Newintensity> intensity   )
        {
            
            intensity = Newintensity;
            return true;
        }
        return false;
    }
}

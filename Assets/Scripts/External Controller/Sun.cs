using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;


//Purpose: To adjust the intensity of the directional light source over time.
public class Sun : MonoBehaviour
{
    // Start is called before the first frame update
    public Timetracker Time;
    public float intensity;
    public float Newintensity;
    private float damage;
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
        
        SolarObject.transform.Translate(0, Mathf.Sin(.5f), -Mathf.Cos(.5f));
        if (Time.CurTime %20 == 0)
        {
            intensify();
        }
        
    }
    public void intensify()
    {
        light.GetComponent<Light>().intensity+= modifier;
       
        intensity= light.GetComponent<Light>().intensity - modifier;
        Newintensity = light.GetComponent<Light>().intensity;
    }
    public float GetDamage()
    {
        return damage;
    }
    public bool HeatRises()
    {
        Debug.Log("ASD");
        if (Newintensity > intensity   )
        {
            
            intensity = Newintensity;

            
            return true;
        }
        
        return false;
    }
}

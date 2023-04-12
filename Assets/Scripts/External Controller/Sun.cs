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
    private float damage_interval;
    private double lastInterval;
    private int frames;
    private float fps;
    public float CurTime;
    private float modifier = 0.01f;
    private float _rotationSpeed;
    public GameObject light;
    public GameObject SolarObject;

    private GameStateController _gameStateController;
  
    void Start()
    {
       
        intensity = 0;
        Newintensity = 0;

        _gameStateController = FindObjectOfType<GameStateController>();
    }

    // Update is called once per frame
    void Update()
    {

        if (SolarObject != null)
        {
            SolarObject.transform.Translate(0, Mathf.Sin(.5f), -Mathf.Cos(.5f));
        }
        if ( FindObjectOfType<Timetracker>().CurTime %30.0f == 1)
        {
            intensify();
        }   
    }
    public void intensify()
    {
        
        light.GetComponent<Light>().intensity+= modifier;

        damage_interval = intensity;
        Newintensity = light.GetComponent<Light>().intensity;

        //notify game state controller of intensity change
        _gameStateController.HandleSunIntensityUpdate(Newintensity);
    }
    public float GetDamage()
    {
        
        return 45f-damage_interval;
    }
    public bool HeatRises()
    {
        if (Newintensity> intensity)
        {
            
            intensity = Newintensity;
            return true;
        }
        return false;
    }
}

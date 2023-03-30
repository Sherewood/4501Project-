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
    private float damage;
    private float modifier = 0.01f;
    private float _rotationSpeed;
    public GameObject light;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        transform.Translate(0, Mathf.Sin(.5f), -Mathf.Cos(.5f));
        if (Time.CurTime %30 == 0)
        {
            intensify();
        }
        
    }
    public void intensify()
    {
        light.GetComponent<Light>().intensity+= modifier;
       
        damage = intensity;
    }
    public float GetDamage()
    {
        return damage;
    }
}

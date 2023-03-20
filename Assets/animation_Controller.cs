using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class animation_Controller : MonoBehaviour
{
    public Animator animator;
    private AnimatorControllerParameter[] parameter;
    public GameObject weapon;
    //differenct animation functions 
    public bool gun=false;
    public bool heavy_gun=false;
    public bool rocket = false;
    // Start is called before the first frame update
    void Start()
    {
       parameter= new AnimatorControllerParameter[animator.parameterCount];
       for (int i = 0; i < parameter.Length; i++)
        {
            parameter[i] = animator.GetParameter(i);
            Debug.Log(parameter[i].name);
        }
       if (weapon == null) { weapon = new GameObject(); }
    }

    // Update is called once per frame
    void Update()
    {
        if(animator.GetBool("FIRE")==false) weapon.SetActive(false);
    }
   public  void SetAnim(String str)
    {
        if (str.Equals("FIRE") && gun) weapon.SetActive(true);
        for (int i = 0; i < parameter.Length; i++)
        {
            animator.SetBool(parameter[i].name,false);
            Debug.Log(parameter[i].name);
        }
        animator.SetBool(str, true);
       

    }
}

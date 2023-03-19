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
    // Start is called before the first frame update
    void Start()
    {
       parameter= new AnimatorControllerParameter[animator.parameterCount];
       for (int i = 0; i < parameter.Length; i++)
        {
            parameter[i] = animator.GetParameter(i);
            Debug.Log(parameter[i].name);
        }

    }

    // Update is called once per frame
    void Update()
    {
        if(animator.GetBool("FIRE")==false) weapon.SetActive(false);
    }
   public  void SetAnim(String str)
    {
        if (str.Equals("FIRE")) weapon.SetActive(true);
        for (int i = 0; i < parameter.Length; i++)
        {
            animator.SetBool(parameter[i].name,false);
            Debug.Log(parameter[i].name);
        }
        animator.SetBool(str, true);
       

    }
}

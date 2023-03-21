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
    public GameObject tool;
    //differenct animation functions 
    public bool gun = false; // If the unit holds a gun
    public bool drill = false; // If the unit has a drill
    // Start is called before the first frame update
    void Start()
    {
        parameter = new AnimatorControllerParameter[animator.parameterCount];
        for (int i = 0; i < parameter.Length; i++)
        {
            parameter[i] = animator.GetParameter(i);
            Debug.Log(parameter[i].name);
        }
        if (weapon == null) { weapon = new GameObject(); }
        if (tool == null) { tool = new GameObject(); }

    }

    // Update is called once per frame
    void Update()
    {
        if (animator.GetBool("FIRE") == false && gun) weapon.SetActive(false);
        if (animator.GetBool("HARVEST") == true && drill)
        {
            tool.transform.GetChild(2).gameObject.SetActive(true);
            tool.transform.GetChild(0).transform.Rotate(0, 20.0f, 0);
        }
        else if (animator.GetBool("HARVEST") == false && drill)
        {
            tool.transform.GetChild(0).transform.Rotate(0, 0.0f, 0);
            tool.transform.GetChild(2).gameObject.SetActive(false);
        }
    }
    public void SetAnim(String str)
    {
        if (str.Equals("FIRE") && gun) weapon.SetActive(true);
        for (int i = 0; i < parameter.Length; i++)
        {
            animator.SetBool(parameter[i].name, false);
            // Debug.Log(parameter[i].name);
        }
        animator.SetBool(str, true);


    }
    public void UnSetAnim(String str)
    {

        animator.SetBool(str, false);
    }
    public Boolean IsIdle()
    {
        for (int i = 0; i < parameter.Length; i++)
        {
            if(animator.GetBool(parameter[i].name))
            {
                return false;
            }
        }
        return true;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animation_test_script : MonoBehaviour
{
    public animation_Controller a;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.A))
        {
            Debug.Log("FIRE");
            a.SetAnim("FIRE");
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            Debug.Log("RUN");
            a.SetAnim("RUN");
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log("DIE");
            a.SetAnim("DIE");
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            Debug.Log("WALK");
            a.SetAnim("WALK");
        }
    }
}

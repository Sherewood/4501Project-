using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserInterfaceController : MonoBehaviour
{
    private GameObject InternalController;
    // Start is called before the first frame update
    void Start()
    {
        InternalController = GameObject.Find("InternalController");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

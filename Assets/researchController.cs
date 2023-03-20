using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class researchController : MonoBehaviour
{
    public GameObject prefab;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void setActive()
    {

        if (prefab.active) prefab.SetActive(false);
        else prefab.SetActive(true);
    }
}

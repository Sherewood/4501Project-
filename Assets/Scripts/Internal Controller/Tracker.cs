using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tracker : MonoBehaviour
{
    public GameObject trackerGameObject;
    // Start is called before the first frame update
    void Start()
    {
        if (trackerGameObject != null) this.transform.position = trackerGameObject.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.position = trackerGameObject.transform.position;
    }
    void setGameObject(GameObject newObj)
    {
        trackerGameObject=newObj;
    }
}

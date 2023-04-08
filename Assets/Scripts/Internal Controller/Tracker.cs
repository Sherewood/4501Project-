using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tracker : MonoBehaviour
{
    public GameObject trackerGameObject;
    public GameObject followingObject;
    // Start is called before the first frame update
    void Start()
    {
      
    }

    // Update is called once per frame
    void Update()
    {
        trackerGameObject.transform.position.Set(followingObject.transform.position.x,0, followingObject.transform.position.z);
    }
    void setGameObject(GameObject newObj)
    {
       // trackerGameObject=newObj;
    }
}

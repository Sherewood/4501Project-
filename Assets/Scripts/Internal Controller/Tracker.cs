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
        trackerGameObject.transform.position = new Vector3 (followingObject.transform.position.x, 700, followingObject.transform.position.z+100);
        trackerGameObject.transform.localScale = new Vector3(20 / trackerGameObject.transform.parent.lossyScale.x, 20/trackerGameObject.transform.parent.lossyScale.y, 20/ trackerGameObject.transform.parent.lossyScale.z);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 pos= new Vector3(followingObject.transform.position.x, 60, followingObject.transform.position.z+20);
        trackerGameObject.transform.position = pos;
        
    }
    void setGameObject(GameObject newObj)
    {
       // trackerGameObject=newObj;
    }
}

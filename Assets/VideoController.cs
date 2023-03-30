using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoController : MonoBehaviour
{
    public GameObject DisplayMat;
    public GameObject onAnim;
    public GameObject offAnim;
    public GameObject Homie;
    public GameObject Text;
    
    
    // Start is called before the first frame update
    void Start()
    {
        onAnim.SetActive(true);
        offAnim.SetActive(false);
        Homie.SetActive(false);
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            if (onAnim.activeInHierarchy)
            {
               //DisplayMat
                offAnim.SetActive(true);
                onAnim.SetActive(false);
                Text.SetActive(false);
            }
            else if (offAnim.activeInHierarchy)
            {
                onAnim.SetActive(true);
                offAnim.SetActive(false);
                Text.SetActive(true);
            }
            
        }
        
    }

}

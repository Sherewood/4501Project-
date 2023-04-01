using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine.Video;
using static System.Net.Mime.MediaTypeNames;

public class VideoController : MonoBehaviour
{
    public GameObject DisplayMat;
    public GameObject onAnim;
    public GameObject offAnim;
   // public TextMeshProUGUI Text;
    public typewriter_effect typewriter_Effect;
    //displayinfo controlelr
    private DisplayInfoController _displayInfoController;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Report());
        _displayInfoController = FindObjectOfType<DisplayInfoController>();
       typewriter_Effect.SendMessage("Alright Commander. We don't have much time. Build up our defensese so we can get enough fule to evacuate. Sun's going to be up soon...");
       // StartCoroutine(SignOff());
        
    }

    // Update is called once per frame
    void Update()
    {
       // Text.GetComponent<TextMeshProUGUI>().text.Equals("AS");
        List<string> dialogue = _displayInfoController.CheckEvents();
        if (dialogue.Count > 0)
        {
            foreach(string s in dialogue)
            {
                EventHandle(s);
            }
        }
    /*    if (Input.GetKeyDown(KeyCode.K))
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
            
        }*/
        
    }
    public void EventHandle(string message)
    {
        StartCoroutine(Report());
        typewriter_Effect.SendMessage(message);
    }
    IEnumerator Report()
    {
        onAnim.SetActive(true);
        offAnim.SetActive(false);
        yield return new WaitForSeconds(15f);
        StartCoroutine(SignOff());

    }
    IEnumerator SignOff()
    {
        offAnim.SetActive(true);
        onAnim.SetActive(false);
        yield return new WaitForSeconds(1f);

    }

}

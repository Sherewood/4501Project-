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
    public List<string> dialogue;
    public TextMeshProUGUI Text;
    public typewriter_effect typewriter_Effect;
    //displayinfo controller
    private DisplayInfoController _displayInfoController;
    

    // Start is called before the first frame update
    void Start()
    {
        dialogue = new List<string>();
        StartCoroutine(Report());
        _displayInfoController = FindObjectOfType<DisplayInfoController>();
       StartCoroutine(EventHandle(("Alright Commander. We don't have much time. Build up our defenses so we can get enough fuel to evacuate. Sun's going to be up soon...")));
       // StartCoroutine(SignOff());
        
    }

    // Update is called once per frame
    void Update()
    {
        
        // Text.GetComponent<TextMeshProUGUI>().text.Equals("AS");
        dialogue = _displayInfoController.CheckEvents();
        if (dialogue.Count > 0 && Text != null && Text.text.Equals("")) 
        {
            StartCoroutine(StringQueue(dialogue));
            
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
    IEnumerator StringQueue(List<string> lines)
    {

        foreach (string s in dialogue)
        {
            StartCoroutine(EventHandle(s));
            yield return new WaitForSeconds(5f);
            dialogue.Remove(s);
        }
        
        
    }
    IEnumerator EventHandle(string message)
    {
        
        StartCoroutine(Report());
        if (typewriter_Effect != null)
        {
            typewriter_Effect.SendMessage(message);
        }
        yield return new WaitForSeconds(15f);

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

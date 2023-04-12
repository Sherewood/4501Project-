using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class typewriter_effect : MonoBehaviour
{
    private string final_string;
    public TextMeshProUGUI text;
    public GameObject audio;
    public float delay=0.1f;
    // Start is called before the first frame update
    void Start()
    {
       // audio.SetActive(false);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SendMessage(string message)
    {
        if (message != text.text)
        {
            final_string = message;
            audio.GetComponent<AudioSource>().Play(0);
            StartCoroutine(typeWriter(delay));
            
        }
       // 
    }
        IEnumerator typeWriter(float delay)
    {
        foreach (char c in final_string)
        {
            text.text += c;
            yield return new WaitForSeconds(delay);
        }
        yield return new WaitForSeconds(2f);
        text.text = "";
        audio.GetComponent<AudioSource>().Stop();
    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class typewriter_effect : MonoBehaviour
{
    private string final_string;
    public GameObject text;
    public float delay=0.2f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SendMessage(string message)
    {
        final_string = message;
        StartCoroutine(typeWriter(delay));
    }
    IEnumerator typeWriter(float delay)
    {
        foreach (char c in final_string)
        {
            text.GetComponent<TextMeshProUGUI>().text += c;
            yield return new WaitForSeconds(delay);
        }
        
    }
}

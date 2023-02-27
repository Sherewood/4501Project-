using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class Timetracker : MonoBehaviour
{
    

    public float updateInterval = 0.5F;
    private double lastInterval;
    private int frames;
    private float fps;
    public GameObject m_Text;
    // Start is called before the first frame update
    void Start()
    {
        lastInterval = Time.realtimeSinceStartup;
        frames = 0;
    }

    // Update is called once per frame
    void Update()
    {
        ++frames;
        float timeNow = Time.realtimeSinceStartup;
        if (timeNow > lastInterval + updateInterval)
        {
            fps = (float)(frames / (timeNow - lastInterval));
            frames = 0;
            lastInterval = timeNow;
        }
        timeNow = (int)timeNow;
       m_Text.GetComponent<TMPro.TextMeshProUGUI>().text= timeNow.ToString();
    }
}

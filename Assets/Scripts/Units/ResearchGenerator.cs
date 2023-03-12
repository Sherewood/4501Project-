using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ResearchGenerator : MonoBehaviour
{

    //re-using this callback since it serves the desired purpose
    ResourceHarvestEvent _researchGenEvent;

    [Tooltip("The amount of research points gained per second")]
    public int ResearchGainRate;

    private float _cooldown;

    private const float BASE_COOLDOWN = 1.0f;

    //init cooldown and callback
    void Awake()
    {
        _cooldown = BASE_COOLDOWN;

        _researchGenEvent = new ResourceHarvestEvent();
    }

    void Update()
    {
        _cooldown -= Time.deltaTime;
        //fairly self explanatory
        if(_cooldown <= 0)
        {
            _cooldown = BASE_COOLDOWN;

            //inform internal controller a certain amount of research points have been generated.
            _researchGenEvent.Invoke("research points", ResearchGainRate);
        }
    }

    public void ConfigureResearchGenCallback(UnityAction<string, int> researchGenCallback)
    {
        _researchGenEvent.AddListener(researchGenCallback);
    }

}

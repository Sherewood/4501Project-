using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/* Unit component */

//purpose: Handle special abilities that can be activated for a period of time
public class TimedSpecialAbility : MonoBehaviour
{

    //callbacks
    public AIEvent AICallback;

    // properties
    public float Duration;
    public float Cooldown;

    //true if ability available to activate
    private bool _canActivate;
    //true if ability activated
    private bool _active;
    //time remaining for ability
    private float _timeRemaining;
    //time remaining for cooldown
    private float _cooldownTime;

    // Start is called before the first frame update
    void Start()
    {
        _canActivate = true;
        _active = false;
        _timeRemaining = 0.0f;
        _cooldownTime = 0.0f;
    }

    // Handle cooldowns/activations....
    void Update()
    {
        if (_active)
        {
            _timeRemaining -= Time.deltaTime;
            //if time on ability runs out, disable and set cooldown
            if(_timeRemaining <= 0.0f)
            {
                _active = false;
                _cooldownTime = Cooldown;
                AICallback.Invoke("specialAbilityEnded");
            }
        }
        else if(!_canActivate)
        {
            _cooldownTime -= Time.deltaTime;
            
            if (_cooldownTime <= 0.0f)
            {
                _canActivate = true;
                AICallback.Invoke("specialAbilityReady");
            }
        }
    }

    public bool CanActivate()
    {
        return _canActivate;
    }

    public bool IsActive()
    {
        return _active;
    }

    //activate the ability if possible
    public bool Activate()
    {
        if (_canActivate)
        {
            _active = true;
            _canActivate = false;
            _timeRemaining = Duration;
            return true;
        }
        else
        {
            return false;
        }
    }
}

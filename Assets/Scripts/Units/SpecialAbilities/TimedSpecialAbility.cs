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

    [Tooltip("Used to display powerup status (cooldown, active time, etc)")]
    public ProgressBarControl PowerupStatusBar;

    // special effect for the ability when it's active
    public GameObject specialEffect;

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

        if(specialEffect != null)
        {
            specialEffect.SetActive(false);
        }

        //if powerup status bar exists, set its colors to the defaults
        //this is really stupid but whatever deadlines tomorrow
        if(PowerupStatusBar != null)
        {
            PowerupStatusBar.SetColors(0,2);
        }
    }

    // Handle cooldowns/activations....
    void Update()
    {
        if (_active)
        {
            _timeRemaining -= Time.deltaTime;

            //if powerup status bar exists, track active time here
            if(PowerupStatusBar != null)
            {
                PowerupStatusBar.SetPercentage(_timeRemaining / Duration);
            }

            //if time on ability runs out, disable and set cooldown
            if(_timeRemaining <= 0.0f)
            {
                _active = false;
                _cooldownTime = Cooldown;

                //return to default (on cooldown) color
                if (PowerupStatusBar != null)
                {
                    PowerupStatusBar.SetColors(0, 2);
                }

                if (specialEffect != null)
                {
                    specialEffect.SetActive(false);
                }

                AICallback.Invoke("specialAbilityEnded");
            }
        }
        else if(!_canActivate)
        {
            _cooldownTime -= Time.deltaTime;

            //if powerup status bar exists, track cooldown here
            if (PowerupStatusBar != null)
            {
                //invert because we want the bar to move from left to right to indicate the powerup "charging"
                PowerupStatusBar.SetPercentage(1.0f - _cooldownTime / Cooldown);
            }

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

            if (specialEffect != null)
            {
                specialEffect.SetActive(true);
            }

            //use "active" color in progress bar
            if (PowerupStatusBar != null)
            {
                PowerupStatusBar.SetColors(1, 2);
            }

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

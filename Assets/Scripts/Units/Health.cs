using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/* Unit Component class */
//Purpose: To monitor unit's health/defense stats, deal with taking damage and report death.

public class Health : MonoBehaviour
{

    private UnitState _unitState;

    [Tooltip("The maximum health of the unit.")]
    public float MaxHealth;

    private float _actualHealth;

    [Tooltip("The base defense of the unit. Maximum defense is 100, formula for now for dmg is 'dmg*(100-def)")]
    public float BaseDefense;

    [Tooltip("The bonus this unit gets to its defense from fortifying.")]
    public float FortifyDefenseBonus;

    private float _actualDefense;

    private const float MAX_DEFENSE = 100;

    // Start is called before the first frame update
    void Start()
    {
        _actualHealth = MaxHealth;
        _actualDefense = BaseDefense;
        _unitState = GetComponent<UnitState>();
    }

    // Update is called once per frame
    void Update()
    {
        UState curState = _unitState.GetState();
    
        //set defense based on whether unit is fortified or not
        if (curState == UState.STATE_FORTIFIED)
        {
            _actualDefense = BaseDefense + FortifyDefenseBonus;
        }
        else
        {
            _actualDefense = BaseDefense;
        }
        
        //check if dead (todo)
    }


    void OnCollisionEnter(Collision collision)
    {
        GameObject impactObject = collision.rigidbody.gameObject;

        //if projectile did not impact the unit, skip for now.
        if (!ImpactFromProjectile(impactObject))
        {
            return;
        }

        //replace 0 with damage value of projectile
        TakeDamage(0);

        //check if dead (todo)
    }

    //check if impact came from projectile
    //TODO: Implement once projectiles are ready
    private bool ImpactFromProjectile(GameObject impactObject)
    {
        return false;
    }

    //self explanatory
    private void TakeDamage(float damage)
    {
        _actualHealth -= damage * (100 - _actualDefense);
    }

}

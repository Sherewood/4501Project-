using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
/* Unit Component class */
//Purpose: To monitor unit's health/defense stats, deal with taking damage and report death.

public class Health : MonoBehaviour
{

    //used to report unit death
    private EntityDeadEvent _entityDeathEvent;
    private bool _reportedDeath;

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

    void Awake()
    {
        _actualHealth = MaxHealth;
        _actualDefense = BaseDefense;
        _unitState = GetComponent<UnitState>();
        _entityDeathEvent = new EntityDeadEvent();
        _reportedDeath = false;
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

        
        //check if dead
        HandleDeathIfNeeded();
    }


    void OnCollisionEnter(Collision collision)
    {
        //avoid crash from colliding with element that has no rigidbody
        if(collision.rigidbody == null)
        {
            return;
        }

        GameObject impactObject = collision.rigidbody.gameObject;

        //if projectile did not impact the unit, skip for now.
        if (!ImpactFromProjectile(impactObject))
        {
            return;
        }

        //replace 0 with damage value of projectile
        TakeDamage(0);

        //check if dead
        HandleDeathIfNeeded();
    }

    //check if impact came from projectile
    //TODO: Implement once projectiles are ready
    private bool ImpactFromProjectile(GameObject impactObject)
    {
        return false;
    }

    public float GetUnitHealth()
    {
        return _actualHealth;
    }

    //self explanatory
    public void TakeDamage(float damage)
    {
        _actualHealth -= damage * ((100 - _actualDefense)/100);
    }

    //check if unit is dead, and if so report death to listeners
    private void HandleDeathIfNeeded()
    {
        if (!_reportedDeath && _actualHealth <= 0)
        {
            //this.GetComponent<animation_Controller>().SetAnim("DEAD");
            Debug.Log("Unit with instance ID " + gameObject.GetInstanceID() + " reporting death.");
            _entityDeathEvent.Invoke(gameObject);
            _reportedDeath = true;
        }
    }

    //set up callback for entity death handling
    public void ConfigureEntityDeathCallback(UnityAction<GameObject> entityDeathCallback)
    {
        _entityDeathEvent.AddListener(entityDeathCallback);
    }
    IEnumerator Death()
    {
        
        yield return new WaitForSeconds(.03f);
    }
}

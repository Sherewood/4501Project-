using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/* Unit Component */
//Purpose: Represent the weapon held by the unit


public class Weapon : MonoBehaviour
{

    //Configuration
    [Tooltip("The type of weapon (melee, ranged, artillery)")]
    public string WeaponType;

    [Tooltip("The damage per hit of the weapon")]
    public float Damage;

    [Tooltip("The fire rate of the weapon (attacks per second)")]
    public float FireRate;

    [Tooltip("The minimum distance from the target required to use the weapon.")]
    public float MinRange;

    [Tooltip("The maximum range of the weapon.")]
    public float MaxRange;

    [Tooltip("The offset from the character's position where the weapon is fired")]
    public Vector3 FiringOffset;

    [Tooltip("The arc in which the weapon can be fired in degrees")]
    public float FiringArc;

    //TODO: Add property for projectile used by the weapon when projectiles are ready
    public GameObject projectilePrefab;


    //parameters
    private const float BASE_COOLDOWN = 1;
    private float _cooldown;



    // Start is called before the first frame update
    void Start()
    {
        ResetCooldown();
    }

    // Update is called once per frame
    void Update()
    {
        if(_cooldown > 0)
        {
            _cooldown -= Time.deltaTime;
        }      
    }

    //return true if weapon is in range, false otherwise
    public bool IsWeaponInRange(float distance)
    {
        return (distance <= MaxRange);
    }

    //return true if weapon is able to fire, false otherwise
    public bool IsWeaponReadyToFire(float distance, Vector3 direction)
    {
        return (_cooldown < 0) && (distance >= MinRange) && IsWithinArc(direction);
    }

    //handles firing the weapon
    public void FireWeapon(GameObject target)
    {

        //TODO: spawn projectile and direct it towards target
        Vector3 offset = transform.rotation * FiringOffset;
        GameObject newProjectile = Instantiate(projectilePrefab, gameObject.transform.position + offset, Quaternion.identity);
        newProjectile.GetComponent<Projectile>()._target = target;
        newProjectile.GetComponent<Projectile>()._weaponType = WeaponType;
        newProjectile.GetComponent<Projectile>()._unitAllegiance = GetComponent<UnitInfo>().UnitType.Split("-")[0];
        newProjectile.GetComponent<Projectile>()._damage = Damage;
        //reset cooldown of weapon
        ResetCooldown();
    }

    private void ResetCooldown()
    {
        if (FireRate == 0)
        {
            _cooldown = BASE_COOLDOWN * 100000;
        }
        else
        {
            _cooldown = BASE_COOLDOWN / FireRate;
        }
    }

    //check if target is within firing arc;
    private bool IsWithinArc(Vector3 direction)
    {
        //get the quaternion between the direction the unit is facing and the direction to the target
        Quaternion angle = Quaternion.FromToRotation(transform.forward, direction);

        //might run into issues relying on this when unit is on a slope
        float angleBetween = angle.eulerAngles.y;

        //wrap around so angle is in range [-180,180]
        if(angleBetween > 180)
        {
            angleBetween -= 360;
        }

        //if angle in range [-FiringArc/2,FiringArc/2], then target is within firing arc.
        return Mathf.Abs(angleBetween) <= FiringArc/2;
    }
}
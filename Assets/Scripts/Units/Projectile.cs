using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public GameObject _target;
    public string _weaponType;
    public string _unitAllegiance;
    public float _damage;

    private int speed;
    private float initialDistance;
    private float creationTime;
    // Start is called before the first frame update
    void Start()
    {
        speed = 25;
        initialDistance = Vector3.Distance(transform.position, _target.transform.position);
        creationTime = Time.time;
        //offset so that melee swing begins left of the attacker and ~1 unit out
        if(string.Compare(_weaponType, "melee") == 0)
        {
            transform.position += Vector3.Cross(_target.transform.position - transform.position, Vector3.up).normalized;
        }
    }

    // Update is called once per frame
    void Update()
    {
        float currentDistance = Vector3.Distance(transform.position, _target.transform.position);
        var step = speed * Time.deltaTime;

        
        if (string.Compare(_weaponType, "melee") == 0)
        {
            //melee swings from left of the attacker as a semi-circle passing through the front to the right side
            transform.position += new Vector3(Mathf.Sin((Time.time - creationTime - Mathf.PI/2) * speed) * speed * Time.deltaTime, 0, Mathf.Cos((Time.time - creationTime - Mathf.PI / 2) * speed) * speed * Time.deltaTime);
            //could be changed to be based on position, will mess up if speed changes
            if (Time.time - creationTime > 0.13)
            {
                _target.GetComponent<Health>().TakeDamage(_damage);
                Destroy(gameObject);
            }
        }

        else if (string.Compare(_weaponType, "ranged") == 0)
        {
            //move bullet towards target and once "close enough" deal damage and destroy self
            transform.position = Vector3.MoveTowards(transform.position, _target.transform.position, step);
            if (currentDistance < 0.01f)
            {
                _target.GetComponent<Health>().TakeDamage(_damage);
                Destroy(gameObject);
            }
        }

        else if (string.Compare(_weaponType, "artillery") == 0)
        {
            //same as bullet but increases height to "arc" like a shell would, probably will redo later if have time to make look pretty
            transform.position = Vector3.MoveTowards(transform.position, _target.transform.position, step);
            if (currentDistance > initialDistance / 1.7)
            {
                transform.position += new Vector3(0, step, 0);
            }
            if (currentDistance < 0.01f)
            {
                //TODO: add explosion to deal area of effect damage
                _target.GetComponent<Health>().TakeDamage(_damage);
                Destroy(gameObject);
            }
        }
    }
}

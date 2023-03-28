using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public GameObject _target;
    public string _weaponType;
    public string _unitAllegiance;
    public float _damage;

    private float speed;
    private float initialDistance;
    private float creationTime;
    private float enemyAngle;
    private Vector3 originalPos;
    private Vector3 _lastRecordedPos;
    // Start is called before the first frame update
    void Start()
    {
        speed = 25.0f;
        _lastRecordedPos = _target.transform.position;
        initialDistance = Vector3.Distance(transform.position, _lastRecordedPos);
        creationTime = Time.time;
        //offset so that melee swing begins left of the attacker and ~1 unit out
        if(string.Compare(_weaponType, "melee") == 0)
        {
            speed = speed * 4.0f / 5.0f;
            enemyAngle = Mathf.Atan2(transform.position.x - _target.transform.position.x, transform.position.z - _target.transform.position.z);
            transform.position += Vector3.Scale(Vector3.Cross(_target.transform.position - transform.position, Vector3.up).normalized, new Vector3(1.3f, 1.3f, 1.3f));
        }
    }

    // Update is called once per frame
    void Update()
    {

        //if target is still alive, projectile should move towards it.
        //Otherwise, projectile should move to the target's last known position;
        Vector3 targetPos;
        if(_target == null)
        {
            targetPos = _lastRecordedPos;
        }
        else
        {
            targetPos = _target.transform.position;
            _lastRecordedPos = _target.transform.position;
        }

        float currentDistance = Vector3.Distance(transform.position, targetPos);
        var step = speed * Time.deltaTime;

        
        if (string.Compare(_weaponType, "melee") == 0)
        {
            //melee swings from left of the attacker as a semi-circle passing through the front to the right side
            transform.position += new Vector3(Mathf.Cos(-((Time.time - creationTime) * speed + Mathf.PI / 2 + enemyAngle)) * Time.deltaTime * speed * 1.3f, 0, Mathf.Sin(-((Time.time - creationTime) * speed + Mathf.PI / 2 + enemyAngle)) * Time.deltaTime * speed * 1.3f);
            //could be changed to be based on position
            if (Time.time - creationTime > 3.0f / speed)
            {
                DealDamage(_target);
                Destroy(gameObject);
            }
        }

        else if (string.Compare(_weaponType, "ranged") == 0)
        {
            //move bullet towards target and once "close enough" deal damage and destroy self
            transform.position = Vector3.MoveTowards(transform.position, targetPos, step);
            if (currentDistance < 0.05f)
            {
                DealDamage(_target);
                Destroy(gameObject);
            }
        }

        else if (string.Compare(_weaponType, "artillery") == 0)
        {
            //same as bullet but increases height to "arc" like a shell would, probably will redo later if have time to make look pretty
            transform.position = Vector3.MoveTowards(transform.position, targetPos, step);
            if (currentDistance > initialDistance / 1.7)
            {
                transform.position += new Vector3(0, step, 0);
            }
            if (currentDistance < 0.05f)
            {
                Collider[] hitColliders = Physics.OverlapSphere(transform.position, 3);
                foreach (var hitCollider in hitColliders)
                {
                    GameObject currentObject = hitCollider.gameObject;
                    if (currentObject.GetComponent<Health>() != null)
                    {
                        UnitInfo unitInfo = currentObject.GetComponent<UnitInfo>();
                        if (unitInfo != null && string.Compare(unitInfo.GetAllegiance(), _unitAllegiance) != 0)
                        {
                            DealDamage(currentObject);
                        }
                    }
                }
                Destroy(gameObject);
            }
        }
    }

    private void DealDamage(GameObject target)
    {
        //if target already dead, can't damage it
        if(target == null)
        {
            return;
        }

        Health healthComponent = target.GetComponent<Health>();

        if (healthComponent == null)
        {
            Debug.LogWarning("Projectile hit target with no health component. Should not happen in normal gameplay!");
            return;
        }

        healthComponent.TakeDamage(_damage);
    }
}

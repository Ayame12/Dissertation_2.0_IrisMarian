using Unity.Netcode;
using UnityEngine;

public class TowerProjectileScript : NetworkBehaviour
{
    public float speed;
    public float damage;
    private GameObject target;

    bool alreadyHit = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("projectile start func");
    }

    // Update is called once per frame
    void Update()
    {
        if (target == null)
        {
            if(IsServer)
            {
                Debug.Log("destroy on lost target");
                Destroy(gameObject);
            }
            return;
        }

        Vector3 direction = target.transform.position - gameObject.transform.position;
        float distance = speed * Time.deltaTime;

        if (direction.magnitude < distance)
        {
            hitTarget();
            return;
        }

        gameObject.transform.Translate(direction.normalized * distance, Space.World);
    }

    public void setTarget(GameObject newTarget)
    {
        target = newTarget;
    }

    private void hitTarget()
    {
        if (!alreadyHit)
        {
            if(IsServer)
            {
                Debug.Log("destroy on hit");
                Destroy(gameObject); 
            }

            if (target.GetComponent<AgentStats>())
            {
                target.GetComponent<AgentStats>().takeDamageRpc(damage, 3);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == target)
        {
            hitTarget();
        }
    }
}

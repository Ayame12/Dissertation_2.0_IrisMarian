using TMPro;
using Unity.Netcode;
using UnityEngine;

public class RootProjectile : NetworkBehaviour
{
    public int enemyLayer;
    public string enemyTowerTag;

    public float speed;
    public float damage;
    public float range;
    public float rootDuration;
    public int targetNumber;

    private GameObject hitTarget = null;
    private int targetsHit = 0;

    private Vector3 targetPosition;
    private Vector3 initialPosition;

    private float radius;

    public void initialize(Vector3 initialPos, Vector3 targetPos)
    {
        initialPosition = initialPos;
        targetPosition = targetPos;

        Vector3 direction = (targetPosition - initialPosition).normalized;
        float rot = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

        transform.position = initialPosition;
        transform.rotation = Quaternion.Euler(0, rot, 0);

        radius = GetComponent<SphereCollider>().radius;
    }

    // Update is called once per frame
    new void Update()
    {
        transform.Translate(0, 0, speed * Time.deltaTime);

        checkAndStunTarget();

        if (targetsHit >= targetNumber)
        {
            Destroy(gameObject);
        }

        float distanceTraveled = Vector3.Distance(initialPosition, gameObject.transform.position);

        if (distanceTraveled > range)
        {
            Destroy(gameObject);
        }

    }

    private void checkAndStunTarget()
    {
        if(!IsOwner)
        {
            return;
        }

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius);

        foreach (Collider collider in hitColliders)
        {
            GameObject parentHit = collider.gameObject.transform.root.gameObject;

            if (parentHit != gameObject)
            {
                if (parentHit.layer == enemyLayer)
                {
                    if (parentHit.tag != enemyTowerTag)
                    {
                        if (parentHit != hitTarget)
                        {
                            if (collider.gameObject.GetComponentInParent<AgentStats>())
                            {
                                collider.gameObject.GetComponentInParent<AgentStats>().takeDamage(damage, 1);
                                collider.gameObject.GetComponentInParent<AgentStats>().applyStun(rootDuration);

                                hitTarget = parentHit;

                                targetsHit++;
                            }
                        }
                    }
                }
            }
        }
    }
}

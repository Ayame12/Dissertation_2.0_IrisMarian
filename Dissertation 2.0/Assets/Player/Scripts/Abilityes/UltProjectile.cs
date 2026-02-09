using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class UltProjectile : NetworkBehaviour
{
    public int enemyLayer;
    public string enemyTowerTag;

    public float speed;
    public float damage;
    public float range;
    public float slowPercentage;
    public float lingerDuration;

    private bool recast = false;
    private bool isMoving = true;

    private Vector3 targetPosition;
    private Vector3 initialPosition;
    private float distanceToTravel;

    private float radius;

    [Rpc(SendTo.Owner)]
    public void initializeRpc(Vector3 initialPos, Vector3 targetPos, float yRot)
    {
        initialPosition = initialPos;
        targetPosition = targetPos;
        distanceToTravel = Vector3.Distance(initialPosition, targetPosition);
        if(distanceToTravel > range)
        {
            distanceToTravel = range;
        }

        transform.position = initialPosition;
        transform.rotation = Quaternion.Euler(0, yRot, 0);

        radius = GetComponentInChildren<SphereCollider>().radius;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        if (Vector3.Distance(initialPosition, gameObject.transform.position) >= distanceToTravel)
        {
            gameObject.transform.position = targetPosition;
            isMoving = false;
        }
        if(recast)
        {
            isMoving = false;
        }

        if (isMoving)
        {
            transform.Translate(0, 0, speed * Time.deltaTime);
        }
        else
        {
            lingerDuration -= Time.deltaTime;
        }

        if (lingerDuration <= 0 || recast)
        {
            checkAndSlowTarget(true);
            destroyProjectileRpc(/*IsHost, true, initialPosition, targetPosition*/);
        }
        else
        {
            checkAndSlowTarget(false);
        }
    }

    private void checkAndSlowTarget(bool dealDamage)
    {
        if (!IsOwner || isMoving)
        {
            return;
        }

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius);

        List<GameObject> hitObjects = new List<GameObject>();

        foreach (Collider collider in hitColliders)
        {
            GameObject parentHit = collider.gameObject.transform.root.gameObject;

            if (parentHit != gameObject)
            {
                bool alreadyHit = false;
                foreach (GameObject obj in hitObjects)
                {
                    if (obj == parentHit)
                    {
                        alreadyHit = true;
                        break;
                    }
                }
                if(alreadyHit)
                {
                    continue;
                }
                
                if (parentHit.layer == enemyLayer)
                {
                    if (parentHit.tag != enemyTowerTag)
                    {
                        if (collider.gameObject.GetComponentInParent<AgentStats>())
                        {
                            if(dealDamage)
                            {
                                collider.gameObject.GetComponentInParent<AgentStats>().takeDamageRpc(damage,1);
                            }
                            else
                            {
                                collider.gameObject.GetComponentInParent<AgentStats>().applySlowRpc(slowPercentage);
                            }

                        }
                    }
                }
            }
        }
    }

    [Rpc(SendTo.Server)]
    private void destroyProjectileRpc(/*bool hostCalled, bool rangeFinish, Vector3 initialPos, Vector3 targetPos*/)
    {
        Destroy(gameObject);
    }

    [Rpc(SendTo.Everyone)]
    public void RecastRpc()
    {
        recast = true;
    }
}


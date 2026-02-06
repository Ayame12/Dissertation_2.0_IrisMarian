using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class RootScript : AbilityScript
{
    public float damage;
    public float speed = 20;
    public float range;
    public float rootDuration;
    public int targetNumber = 2;

    private int targetsHit = 0;

    private GameObject alreadyHitTarget;

    private float radius;

    //public GameObject sphere;

    new void Update()
    {
        if (!IsOwner)
        { return; }

        base.Update();

        if (isActive)
        {
            if (gameObject.activeInHierarchy)
            {
                gameObject.transform.Translate(0, 0, speed * Time.deltaTime);
            }

            checkAndStunTargets();

            if (targetsHit >= targetNumber)
            {
                isActive = false;

                if (gameObject.GetComponent<MeshRenderer>())
                {
                    gameObject.GetComponent<MeshRenderer>().enabled = false;
                }
                if (gameObject.GetComponent<SphereCollider>())
                {
                    gameObject.GetComponent<SphereCollider>().enabled = false;
                }

                //sphere.SetActive(false);
                alreadyHitTarget = null;
            }

            float distanceTraveled = Vector3.Distance(initialPosition, gameObject.transform.position);

            if (distanceTraveled > range)
            {
                isActive = false;

                if (gameObject.GetComponent<MeshRenderer>())
                {
                    gameObject.GetComponent<MeshRenderer>().enabled = false;
                }
                if (gameObject.GetComponent<SphereCollider>())
                {
                    gameObject.GetComponent<SphereCollider>().enabled = false;
                }
                //sphere.SetActive(false);
                alreadyHitTarget = null;
            }
        }
    }

    public override void initialize(GameObject playerObj)
    {
        base.initialize(playerObj);

        //sphere.SetActive(false);
        if (gameObject.GetComponent<MeshRenderer>())
        {
            gameObject.GetComponent<MeshRenderer>().enabled = false;
            radius = GetComponent<SphereCollider>().radius;
        }
        if (gameObject.GetComponent<SphereCollider>())
        {
            gameObject.GetComponent<SphereCollider>().enabled = false;
        }

        //radius = sphere.GetComponent<SphereCollider>().radius;
    }

    public override void action()
    {
        cooldownTimer = cooldown;
        targetsHit = 0;
        isActive = true;
        //sphere.SetActive(true);

        if (gameObject.GetComponent<MeshRenderer>())
        {
            gameObject.GetComponent<MeshRenderer>().enabled = true;
        }
        if (gameObject.GetComponent<SphereCollider>())
        {
            gameObject.GetComponent<SphereCollider>().enabled = true;
        }

        initialPosition = player.GetComponent<Transform>().position;
        targetPosition = getMousePos();

        Vector3 direction = (targetPosition - initialPosition).normalized;

        float rot = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

        gameObject.SetActive(true);
        gameObject.transform.position = initialPosition;
        gameObject.transform.rotation = Quaternion.Euler(0, rot, 0);
    }

    private void checkAndStunTargets()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius);

        foreach (Collider collider in hitColliders)
        {
            if (collider.gameObject != gameObject)
            {
                if (collider.gameObject.layer == enemyLayer)
                {
                    if (collider.gameObject.tag != enemyTowerTag)
                    {
                        GameObject parentHit = collider.gameObject.transform.root.gameObject;

                        if (parentHit != alreadyHitTarget)
                        {
                            if (collider.gameObject.GetComponentInParent<AgentStats>())
                            {
                                collider.gameObject.GetComponentInParent<AgentStats>().takeDamage(damage, 1);
                                collider.gameObject.GetComponentInParent<AgentStats>().applyStun(rootDuration);

                                alreadyHitTarget = parentHit;

                                targetsHit++;
                            }
                        }
                    }
                }
            }
        }
    }
}

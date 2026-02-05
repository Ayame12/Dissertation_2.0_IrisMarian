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

    GameObject[] alreadyHitTargets = { null };

    private float radius;

    public GameObject sphere;

    new void Start()
    {
        base.Start();

        radius = GetComponentInChildren<SphereCollider>().radius;
    }

    new void Update()
    {
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

                //if (gameObject.GetComponentInChildren<MeshRenderer>())
                //{
                //    gameObject.GetComponentInChildren<MeshRenderer>().enabled = false;
                //}
                //if (gameObject.GetComponentInChildren<SphereCollider>())
                //{
                //    gameObject.GetComponentInChildren<SphereCollider>().enabled = false;
                //}

                sphere.SetActive(false);
            }

            float distanceTraveled = Vector3.Distance(initialPosition, gameObject.transform.position);

            if (distanceTraveled > range)
            {
                isActive = false;

                //if (gameObject.GetComponentInChildren<MeshRenderer>())
                //{
                //    gameObject.GetComponentInChildren<MeshRenderer>().enabled = false;
                //}
                //if (gameObject.GetComponentInChildren<SphereCollider>())
                //{
                //    gameObject.GetComponentInChildren<SphereCollider>().enabled = false;
                //}
                sphere.SetActive(false);
            }
        }
    }

    public override void action()
    {
        cooldownTimer = cooldown;
        targetsHit = 0;
        isActive = true;
        gameObject.GetComponentInChildren<MeshRenderer>().enabled = true;

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
            if (collider.gameObject != gameObject && collider.gameObject != alreadyHitTargets[0])
            {
                if (collider.gameObject.layer == enemyLayer && collider.gameObject.tag != enemyTowerTag)
                {
                    if (collider.gameObject.GetComponent<AgentStats>())
                    {
                        collider.gameObject.GetComponent<AgentStats>().takeDamage(damage, 1);
                        collider.gameObject.GetComponent<AgentStats>().applyStun(rootDuration);

                        alreadyHitTargets[0] = collider.gameObject;

                        targetsHit++;
                    }
                }
            }
        }
    }
}

using Unity.Netcode;
using UnityEngine;

public class TowerScript : NetworkBehaviour
{
    public float range;
    public float cooldown;
    public GameObject projectileprefab;
    public Transform spawnPoint;
    //public LineRenderer lineRenderer;

    public int enemyLayer;
    public int allyLayer;

    private GameObject enemyPlayer;
    private string enemyMinionTag;

    private float attackTimer;
    public GameObject currentTarget;

    private bool done = false;

    private void Start()
    {
        enemyMinionTag = GetComponent<AgentStats>().enemyMinionTag;
    }

    // Update is called once per frame
    void Update()
    {
        if (!done)
        {
            return;
        }
        if (currentTarget != null && Vector3.Distance(transform.position, currentTarget.transform.position) > range)
        {
            currentTarget = null;
        }

        if (currentTarget == null)
        {
            findAndSetTarget();
        }

        updateLine();

        if (currentTarget != null)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0)
            {
                spawnAttackRpc();
                attackTimer = cooldown;
            }
        }
    }

    private void findAndSetTarget()
    {
        GameObject[] enemyMinions = GameObject.FindGameObjectsWithTag(enemyMinionTag);

        GameObject closestEnemyMinion = getClosestObjectInRadius(enemyMinions, range);

        if (closestEnemyMinion != null)
        {
            currentTarget = closestEnemyMinion;
            attackTimer = cooldown;
        }
        else
        {
            float distance = Vector3.Distance(gameObject.transform.position, enemyPlayer.transform.position);

            if (distance <= range)
            {
                currentTarget = enemyPlayer;
                attackTimer = cooldown;
            }
        }
    }

    private GameObject getClosestObjectInRadius(GameObject[] objects, float radius)
    {
        float closestDistance = Mathf.Infinity;
        GameObject closestEnemy = null;

        foreach (GameObject obj in objects)
        {
            float distance = Vector3.Distance(gameObject.transform.position, obj.transform.position);

            if (distance < closestDistance && distance <= range)
            {
                closestDistance = distance;
                closestEnemy = obj;
            }
        }

        return closestEnemy;
    }

    private void updateLine()
    {
        //if (currentTarget != null)
        //{
        //    lineRenderer.enabled = true;
        //    lineRenderer.SetPosition(0, spawnPoint.transform.position);
        //    lineRenderer.SetPosition(1, currentTarget.transform.position);
        //}
        //else
        //{
        //    lineRenderer.enabled = false;
        //}
    }

    [Rpc(SendTo.Server)]
    private void spawnAttackRpc()
    {
        Debug.Log("spawning projectile");
        GameObject projectile = Instantiate(projectileprefab, spawnPoint.transform.position, Quaternion.identity);
        projectile.GetComponent<NetworkObject>().Spawn(true);
        projectile.GetComponent<TowerProjectileScript>().setTarget(currentTarget);
    }

    public void setupPlayerRef(GameObject player)
    {
        enemyPlayer = player;
        done = true;
    }
}

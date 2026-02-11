using Unity.Netcode;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class MinionRangedCombat : NetworkBehaviour
{
    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;

    private float attackRange = 10;
    private float attackDamage = 10;
    public float attackCooldown = 2;

    public float attackTimer = 0;
    private bool isAttacking = false;

    private AgentStats stats;
    private MinionManager minionScript;

    private float targetSwitchTimer;
    private float targetSwitchInterval;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (NetworkManager.IsHost)
        {
            stats = GetComponent<AgentStats>();
            minionScript = GetComponent<MinionManager>();

            targetSwitchTimer = minionScript.targetSwitchTimer;
            targetSwitchInterval = minionScript.targetSwitchInterval;

            attackRange = minionScript.stopDistange;
            attackDamage = stats.damage;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (NetworkManager.IsHost)
        {
            attackTimer -= Time.deltaTime;

            if (isAttacking)
            {
                if (attackTimer <= 0)
                {
                    isAttacking = false;
                }
            }

            if ((canAttack()))
            {
                Attack();
            }
        }
    }

    private bool canAttack()
    {
        if (!isAttacking && minionScript.currentTarget != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, minionScript.currentTarget.transform.position);
            return distanceToTarget <= attackRange;
        }
        return false;
    }

    private void Attack()
    {
        attackTimer = attackCooldown;
        isAttacking = true;

        GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
        projectile.GetComponent<BasicProjectile>().setTarget(minionScript.currentTarget, attackDamage);
        projectile.GetComponent<NetworkObject>().Spawn(true);

        targetSwitchTimer = targetSwitchInterval;
    }
}

using Unity.Netcode;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class MinionProjectile : NetworkBehaviour
{
    private GameObject target;
    private float damage;
    public float speed;

    public int damageType;

    // Update is called once per frame
    void Update()
    {
        if (NetworkManager.IsHost)
        {
            if (target != null)
            {
                Vector3 dir = target.transform.position - transform.position;
                float distanceThisFrame = speed * Time.deltaTime;
                transform.Translate(dir.normalized * distanceThisFrame, Space.World);

                if (dir.magnitude <= distanceThisFrame)
                {
                    damageTarget();
                    Destroy(gameObject);
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    public void setTarget(GameObject newTarget, float attackDamage)
    {
        target = newTarget;
        damage = attackDamage;
    }

    private void damageTarget()
    {
        if (target.GetComponent<AgentStats>())
        {
            target.GetComponent<AgentStats>().takeDamageRpc(damage, damageType);
        }
    }
}

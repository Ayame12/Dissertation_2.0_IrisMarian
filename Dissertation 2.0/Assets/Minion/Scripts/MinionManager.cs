using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

[Serializable]
public class MinionSerializationData
{
    public string objectType = "minion";
    public bool isBlue = false;
    public float health;
    public bool isTargetingPlayer = false;
    public Vector3 position = new Vector3(0, 0, 0);
}

public class MinionManager : NetworkBehaviour
{
    private NavMeshAgent agent;
    public GameObject currentTarget;

    [Header("Object Identifiers")]
    //private int enemyLayer;
    //private int friendlyLayer;
    private string enemyTowerTag;
    private string enemyMinionTag;
    private string enemyPlayerTag;
    //private GameObject enemyTower;
    //private GameObject enemyPlayer;

    [Header("Combat and Movement")]
    public float stopDistange;
    public float aggroDistance;
    public float targetSwitchInterval;
    public float targetSwitchTimer;

    [Space]
    public float CSTimerThreshhold;
    //private float timerSincePlayerDamage;

    private AgentStats stats;

    public int uniqueIdentifier;

    public MinionSerializationData serializedMinion;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    //public override void OnNetworkSpawn()
    void Awake()
    {
        stats = GetComponent<AgentStats>();

        enemyTowerTag = stats.enemyTowerTag;
        enemyMinionTag = stats.enemyMinionTag;
        enemyPlayerTag = stats.enemyPlayerTag;

        if(stats.friendlyLayer == 9)
        {
            serializedMinion.isBlue = true;
        }
        else
        {
            serializedMinion.isBlue = false;
        }
        serializedMinion.health = stats.health;
        serializedMinion.position = transform.position;
        serializedMinion.isTargetingPlayer = false;


        if (NetworkManager.IsHost)
        {
            agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            agent.speed = stats.speed;

            //enemyPlayer = stats.enemyPlayer;
            //enemyTower = stats.enemyTower;
            //enemyLayer = stats.enemyLayer;
            //friendlyLayer = stats.friendlyLayer;

            findAndSetTarget();

            targetSwitchTimer = 0;
        }

        //if (enemy)
        //    serializedMinion.isBlue
    }

    // Update is called once per frame
    //void Update()
    //{
    //    agent.speed = GetComponent<Stats>().currentSpeed;


    //    if(currentTarget == null)
    //    {
    //        findAndSetTarget();
    //    }

    //    if(currentTarget != null)
    //    {
    //        Vector3 directionToTarget = currentTarget.transform.position - gameObject.transform.position;
    //        Vector3 stoppingPosition = currentTarget.transform.position - directionToTarget.normalized * stopDistange;

    //        agent.SetDestination(stoppingPosition);

    //        faceTarget();
    //    }
    //}

    void Update()
    {
        if (NetworkManager.IsHost)
        {
            agent.speed = stats.currentSpeed;

            targetSwitchTimer -= Time.deltaTime;

            if (targetSwitchTimer <= 0 || currentTarget == null)
            {
                findAndSetTarget();
            }

            if (currentTarget != null)
            {
                float distanceToTarget = Vector3.Distance(gameObject.transform.position, currentTarget.transform.position);

                if (distanceToTarget > stopDistange)
                {
                    Vector3 directionToTarget = currentTarget.transform.position - gameObject.transform.position;
                    Vector3 stoppingPosition = currentTarget.transform.position - directionToTarget.normalized * stopDistange;

                    stoppingPosition.y = transform.position.y;

                    agent.SetDestination(stoppingPosition);
                }

                faceTarget();

                if(currentTarget.tag == enemyPlayerTag)
                {
                    serializedMinion.isTargetingPlayer = true;
                }
                else
                {
                    serializedMinion.isTargetingPlayer = false;
                }
            }
            else
            {
                serializedMinion.isTargetingPlayer = false;
            }
        }

        serializedMinion.health = stats.targetHealth;
        serializedMinion.position = gameObject.transform.position;
    }

    private void faceTarget()
    {
        Vector3 directionToTarget = (currentTarget.transform.position - gameObject.transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToTarget.x, 0, directionToTarget.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    private void findAndSetTarget()
    {
        GameObject[] enemyMinions = GameObject.FindGameObjectsWithTag(enemyMinionTag);
        //GameObject[] enemyMinions = GameObject.FindGameObjectsWithTag("RedMinion");


        GameObject closestEnemyMinion = getClosestObjectInRadius(enemyMinions);

        if (closestEnemyMinion != null)
        {
            currentTarget = closestEnemyMinion;
        }
        else
        {
            GameObject enemyTower = GameObject.FindGameObjectWithTag(enemyTowerTag);
            //GameObject enemyTower = GameObject.FindGameObjectWithTag("RedTower");
            float distanceToTower = Vector3.Distance(gameObject.transform.position, enemyTower.transform.position);

            if (distanceToTower < aggroDistance)
            {
                currentTarget = enemyTower;
            }
            else
            {
                GameObject enemyPlayer = GameObject.FindGameObjectWithTag(enemyPlayerTag);
                //GameObject enemyPlayer = GameObject.FindGameObjectWithTag("RedPlayer");
                if (enemyPlayer != null)
                {
                    float distanceToPlayer = Vector3.Distance(gameObject.transform.position, enemyPlayer.transform.position);

                    if (distanceToPlayer < aggroDistance)
                    {
                        currentTarget = enemyPlayer;
                    }
                    else
                    {
                        currentTarget = enemyTower;
                    }
                }
                else
                {
                    currentTarget = enemyTower;
                }
            }
        }
    }

    private GameObject getClosestObjectInRadius(GameObject[] objects)
    {
        float closestDistance = Mathf.Infinity;
        GameObject closestEnemy = null;

        foreach (GameObject obj in objects)
        {
            float distance = Vector3.Distance(gameObject.transform.position, obj.transform.position);

            if (distance < closestDistance && distance < aggroDistance)
            {
                closestDistance = distance;
                closestEnemy = obj;
            }
        }

        return closestEnemy;
    }

    [Rpc(SendTo.Everyone)]
    public void setIdentifierRpc(int identifier)
    {
        uniqueIdentifier = identifier;
    }
}

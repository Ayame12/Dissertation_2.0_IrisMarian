using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private NavMeshAgent agent;

    public float rotateSpeedMovement = 0.05f;
    private float rotateVelocity;
    public float stopDistance;

    public GameObject targetEnemy;

    public GameObject moveIcon;
    public float moveIconTimerMax = 1f;
    private float moveIconTimer;

    public GameObject basicAttackPrefab;
    public float basicAttackCastDuration;
    public float basicAttackCooldown;
    private float basicAttackCooldownTimer;

    public bool hasControl = true;

    private PlayerInputScript playerInput;
    private AgentStats stats;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        stats = GetComponent<AgentStats>();

        agent.speed = stats.speed;

        playerInput = GetComponent<PlayerInputScript>();
    }

    // Update is called once per frame
    public void tickUpdate()
    {
        agent.speed = stats.currentSpeed;

        if (basicAttackCooldownTimer > 0)
        {
            basicAttackCooldownTimer -= Time.deltaTime;
        }

        if (hasControl)
        {
            if (playerInput.move)
            {
                moveToPosition(playerInput.mousePosInGame);

                //move icon
                Vector3 offset = new Vector3(playerInput.mousePosInGame.x, playerInput.mousePosInGame.y + 0.05f, playerInput.mousePosInGame.z);
                moveIcon.SetActive(true);
                moveIcon.transform.position = offset;
                //moveIcon.GetComponent<Animator>().Play("MoveIconAnim", -1, 0f);

                moveIconTimer = moveIconTimerMax;
            }
            if (playerInput.attack)
            {
                targetEnemy = playerInput.target;

                moveToEnemy(targetEnemy);
            }

            if (targetEnemy != null)
            {
                float dis = Vector3.Distance(transform.position, targetEnemy.transform.position);
                if (dis < stopDistance + 1.5f)
                {
                    castBasicAttack();
                    agent.SetDestination(transform.position);
                }
                else
                {
                    agent.SetDestination(targetEnemy.transform.position);
                }

            }
        }

        if (moveIconTimer > 0f)
        {
            moveIconTimer -= Time.deltaTime;

            if (moveIconTimer <= 0f)
            {
                moveIcon.SetActive(false);
            }
        }
    }

    public void moveToPosition(Vector3 position)
    {
        //MOVEMENT
        agent.SetDestination(position);
        agent.stoppingDistance = 0;

        rotateToLookAt(position);

        if (targetEnemy != null)
        {
            //hmScript.deselectHighlight();
            targetEnemy = null;
        }

    }

    public void moveToEnemy(GameObject enemy)
    {
        agent.SetDestination(targetEnemy.transform.position);
        agent.stoppingDistance = stopDistance;

        rotateToLookAt(targetEnemy.transform.position);

        //hmScript.selectedHighlight(enemy);
    }

    public void rotateToLookAt(Vector3 lookAtPosition)
    {
        //ROTATION
        Quaternion rotationToLookAt = Quaternion.LookRotation(lookAtPosition - transform.position);
        float rotationY = Mathf.SmoothDampAngle(transform.eulerAngles.y, rotationToLookAt.eulerAngles.y, ref rotateVelocity, rotateSpeedMovement * (Time.deltaTime * 5));

    }

    public void castBasicAttack()
    {
        //if (basicAttackCooldownTimer <= 0)
        //{
        //    AgentStats stats = GetComponent<AgentStats>();
        //    stats.applyStun(basicAttackCastDuration);

        //    basicAttackCooldownTimer = basicAttackCooldown;

        //    GameObject projectile = Instantiate(basicAttackPrefab, transform.position, Quaternion.identity);
        //    projectile.GetComponent<BasicProjectile>().setTarget(targetEnemy, stats.damage);

        //    if (targetEnemy.GetComponent<PlayerManager>() != null)
        //    {
        //        //gameObject.GetComponent<PlayerManager>().aggroAllInRange();
        //    }
        //}
    }
}

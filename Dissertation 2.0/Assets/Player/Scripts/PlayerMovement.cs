using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI.Table;

public class PlayerMovement : MonoBehaviour
{
    private NavMeshAgent agent;

    //public float rotateSpeedMovement = 0.05f;
    //private float rotateVelocity;
    public float stopDistance;

    private bool isDashing = false;
    private Vector3 initialPosDash;
    private Vector3 targetPosDash;
    private float dashRange;
    private float dashSpeed;

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
        agent.SetDestination(transform.position);

        playerInput = GetComponent<PlayerInputScript>();

        dashRange = GetComponent<PlayerAttackScript>().dashRange;
        dashSpeed = GetComponent<PlayerAttackScript>().dashSpeed;
    }

    // Update is called once per frame
    public void tickUpdate()
    {
        if(!isDashing)
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
                    moveToPosition(playerInput.lastRightClick);

                    //move icon
                    Vector3 offset = new Vector3(playerInput.lastRightClick.x, playerInput.lastRightClick.y + 0.05f, playerInput.lastRightClick.z);
                    moveIcon.SetActive(true);
                    moveIcon.transform.position = offset;
                    //moveIcon.GetComponent<Animator>().Play("MoveIconAnim", -1, 0f);

                    moveIconTimer = moveIconTimerMax;

                    //potentialy remove this?????????????????????????
                    targetEnemy = null;
                }
                if (playerInput.attack)
                {
                    targetEnemy = playerInput.target;

                    if (targetEnemy != null)
                    {
                        moveToEnemy(targetEnemy);
                    }
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
        }
        else
        {
            transform.Translate(0, 0, dashSpeed * Time.deltaTime);

            float distanceDashed = Vector3.Distance(transform.position, initialPosDash);

            if (distanceDashed >= dashRange)
            {
                transform.position = targetPosDash;

                hasControl = true;

                isDashing = false;

                GetComponent<NavMeshAgent>().enabled = true;
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
        //float rotationY = Mathf.SmoothDampAngle(transform.eulerAngles.y, rotationToLookAt.eulerAngles.y, ref rotateVelocity, rotateSpeedMovement * (Time.deltaTime * 5));
        //float rotationY = rotationToLookAt.eulerAngles.y;
        //transform.rotation = Quaternion.Euler(transform.rotation.x, rotationToLookAt.y, transform.rotation.z);
        //transform.rotation = rotationToLookAt;
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

    public void startDash(Vector3 initialPos,Vector3 targetPos, float yRotation)
    {
        hasControl = false;
        isDashing = true;
        initialPosDash = initialPos;
        targetPosDash = targetPos;

        GetComponent<NavMeshAgent>().SetDestination(targetPosDash);
        GetComponent<NavMeshAgent>().enabled = false;

        transform.rotation = Quaternion.Euler(0, yRotation, 0);
    }
}

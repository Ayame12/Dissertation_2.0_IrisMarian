using System;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public class PlayerInputSerializedData
{
    public bool isBlue = false;
    public bool move = false;
    public bool attack = false;
    public bool ability1 = false;
    public bool ability2 = false;
    public bool ability3 = false;
    public PositionData mousePosInGame = new PositionData();
    //public string hoveredObject = "null";
    public bool isHoveringPlayer = false;
    public bool isHoveringTower = false;
    public bool isHoveringMinion = false;
    public bool isHoveringGround = false;
    public int target = 0;
}
public class PlayerInputScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private Camera cam;

    public bool isAI = false;

    private Vector2 mousePos;
    public Vector3 mousePosInGame;
    //public Vector3 lastRightClick;
    public GameObject hoveredObject;

    public int lastDirection = 0;

    public KeyCode ability1Key;
    public KeyCode ability2Key;
    public KeyCode ability3Key;

    public bool move = false;
    public bool attack = false;
    public bool ability1 = false;
    public bool ability2 = false;
    public bool ability3 = false;

    public float rootRange;
    public float ultRange;
    public float rootTargetingDistanceThreshold;
    public float ultTargetingDistanceThreshold;

    public bool newInput = false;

    public GameObject target;

    private GameObject enemyPlayer;

    private AgentStats stats;

    public PlayerInputSerializedData serializedData;

    private PlayerMovement playerMovement;

    void Start()
    {
        mousePos = new Vector2(0, 0);
        mousePosInGame = new Vector3(0, 0, 0);

        cam = Camera.main;

        stats = GetComponent<AgentStats>();
        playerMovement = GetComponent<PlayerMovement>();
        serializedData = new PlayerInputSerializedData();
        mousePosInGame.x = 0;
        mousePosInGame.y = 0;
        mousePosInGame.z = 0;
        if (stats.friendlyLayer == 9)
        {
            serializedData.isBlue = true;
        }
        else
        {
            serializedData.isBlue = false;
        }
    }

    // Update is called once per frame
    public void tickUpdate()
    {
        if (!enemyPlayer)
        {
            enemyPlayer = GameObject.FindGameObjectWithTag(stats.enemyPlayerTag);
            if (!enemyPlayer)
            {
                return;
            }
        }

        move = false;
        attack = false;
        ability1 = false;
        ability2 = false;
        ability3 = false;
        newInput = false;

        if (!isAI)
        {
            serializedData.move = false;
            serializedData.attack = false;
            serializedData.ability1 = false;
            serializedData.ability2 = false;
            serializedData.ability3 = false;
            //serializedData.hoveredObject = "null";
            serializedData.isHoveringPlayer = false;
            serializedData.isHoveringTower = false;
            serializedData.isHoveringMinion = false;
            serializedData.isHoveringGround = false;
            serializedData.target = 0;

            Vector2 currentMousePos = Mouse.current.position.ReadValue();

            GameObject tempTarget = null;

            if (mouseOnScreen(currentMousePos))
            {
                //Debug.Log("mouse on screen");
                lastDirection = 0;

                mousePos = currentMousePos;


                RaycastHit hit;



                if (Physics.Raycast(cam.ScreenPointToRay(mousePos), out hit, Mathf.Infinity))
                {
                    mousePosInGame = hit.point;

                    if (/*Mouse.current.rightButton.isPressed ||*/ Mouse.current.rightButton.wasPressedThisFrame)
                    {
                        if (hit.collider.gameObject.layer == stats.groundLayer || hit.collider.gameObject.layer == stats.friendlyLayer)
                        {
                            move = true;
                            attack = false;
                            newInput = true;
                            //serializedData.hoveredObject = "groundHovered";

                        }
                        else if (hit.collider.gameObject.layer == stats.enemyLayer)
                        {
                            attack = true;
                            move = false;
                            newInput = true;

                            //if (hit.transform.parent)
                            //{
                            //    tempTarget = hit.transform.parent.gameObject;
                            //}
                            //else if (hit.collider.gameObject != null)
                            //{
                            //    tempTarget = hit.transform.gameObject;
                            //}
                            tempTarget = hit.transform.root.gameObject;
                        }

                        //lastRightClick = mousePosInGame;
                        target = tempTarget;
                    }

                    //Debug.Log(mousePos.ToString() + "  :  " + mousePosInGame.ToString());
                }
            }
            else
            {
                float up = Mathf.Abs(Screen.height - 1 - mousePos.y);
                float down = Mathf.Abs(mousePos.y);
                float left = Mathf.Abs(mousePos.x);
                float right = Mathf.Abs(Screen.width - 1 - mousePos.x);

                int smallestDir = 1;
                float smallest = up;

                if (smallest > down)
                {
                    smallestDir = 2;
                    smallest = down;
                }
                if (smallest > left)
                {
                    smallestDir = 3;
                    smallest = left;
                }
                if (smallest > right)
                {
                    smallestDir = 4;
                    smallest = right;
                }

                lastDirection = smallestDir;
            }

            if (Input.GetKeyDown(ability1Key))
            {
                ability1 = true;
                newInput = true;
            }

            if (Input.GetKeyDown(ability2Key) && !stats.isStunned)
            {
                ability2 = true;
                newInput = true;
            }

            if (Input.GetKeyDown(ability3Key))
            {
                ability3 = true;
                newInput = true;
            }

            if (newInput)
            {
                serializedData.move = move;
                serializedData.attack = attack;
                serializedData.ability1 = ability1;
                serializedData.ability2 = ability2;
                serializedData.ability3 = ability3;
                serializedData.mousePosInGame.x = mousePosInGame.x;
                serializedData.mousePosInGame.y = mousePosInGame.y;
                serializedData.mousePosInGame.z = mousePosInGame.z;

                if (target)
                {
                    if (target.GetComponent<PlayerManager>())
                    {
                        serializedData.target = 1;
                    }
                    else if (target.GetComponent<TowerScript>())
                    {
                        serializedData.target = 2;
                    }
                    else
                    {
                        serializedData.target = 3;
                    }
                }
                else
                {
                    if (playerMovement.targetEnemy)
                    {
                        if (playerMovement.targetEnemy.GetComponent<PlayerManager>())
                        {
                            serializedData.target = 1;
                        }
                        else if (playerMovement.targetEnemy.GetComponent<TowerScript>())
                        {
                            serializedData.target = 2;
                        }
                        else 
                        {
                            serializedData.target = 3;
                        }
                    }
                }
                
                if (ability1)
                {
                    Vector2 enemyPos = new Vector2(enemyPlayer.transform.position.x, enemyPlayer.transform.position.z);
                    Vector2 thisPos = new Vector2(transform.position.x, transform.position.z);
                    Vector2 mouse2D = new Vector2(mousePosInGame.x, mousePosInGame.z);

                    if (Vector2.Distance(thisPos, enemyPos) < rootRange + 1)
                    {

                        Vector2 distanceBetweenPlayers = enemyPos - thisPos;
                        Vector2 dir = (mouse2D - thisPos).normalized;

                        float t = Vector3.Dot(distanceBetweenPlayers, dir);

                        float shortestDistance = 0;

                        if (t >= 0)
                        {
                            // Use perpendicular distance
                            shortestDistance = Vector3.Cross(distanceBetweenPlayers, dir).magnitude;

                            if (shortestDistance < rootTargetingDistanceThreshold)
                            {
                                serializedData.target = 1;
                            }
                            else
                            {
                                serializedData.target = 3;
                            }
                        }
                        else
                        {
                            serializedData.target = 3;
                        }
                    }
                }

                if (ability3)
                {
                    Vector2 enemyPos = new Vector2(enemyPlayer.transform.position.x, enemyPlayer.transform.position.z);
                    Vector2 thisPos = new Vector2(transform.position.x, transform.position.z);
                    Vector2 mouse2D = new Vector2(mousePosInGame.x, mousePosInGame.z);

                    if (Vector2.Distance(thisPos, enemyPos) < ultRange + 1)
                    {

                        Vector2 distanceBetweenPlayers = enemyPos - thisPos;
                        Vector2 dir = mouse2D - thisPos;

                        float t = Vector3.Dot(distanceBetweenPlayers, dir);

                        float shortestDistance;

                        if (t >= 0)
                        {
                            // Use perpendicular distance
                            shortestDistance = Vector3.Cross(distanceBetweenPlayers, dir).magnitude;

                            if (shortestDistance < ultTargetingDistanceThreshold)
                            {
                                serializedData.target = 1;
                            }
                            else
                            {
                                serializedData.target = 3;
                            }
                        }
                        else
                        {
                            serializedData.target = 3;
                        }
                    }
                }

                if (tempTarget != null)
                {
                    if (tempTarget.tag == stats.enemyPlayerTag)
                    {
                        serializedData.isHoveringPlayer = true;
                        //serializedData.hoveredObject = "playerHovered";
                    }
                    else if (tempTarget.tag == stats.enemyMinionTag)
                    {
                        serializedData.isHoveringMinion = true;
                        //serializedData.hoveredObject = "minionHovered";
                    }
                    else if (tempTarget.tag == stats.enemyTowerTag)
                    {
                        serializedData.isHoveringTower = true;
                        //serializedData.hoveredObject = "towerHovered";
                    }
                }
                else if (move)
                {
                    serializedData.isHoveringGround = true;
                }
            }
        }
    }

    public bool mouseOnScreen(Vector2 mouse)
    {
#if UNITY_EDITOR
        if (mouse.x <= 0 || mouse.y <= 0 || mouse.x >= Screen.width - 1 || mouse.y >= Screen.height - 1)
        {
            return false;
        }
#else
        if (mouse.x <= 0 || mouse.y <= 0 || mouse.x >= Screen.width - 1 || mouse.y >= Screen.height - 1) {
        return false;
        }
#endif
        else
        {
            return true;
        }
    }

    public void resetComponent()
    {
        move = false;
        attack = false;
        ability1 = false;
        ability2 = false;
        ability3 = false;
    }
}


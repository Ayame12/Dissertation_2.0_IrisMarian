using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputSerializedData
{
    public bool isBlue = false;
    public bool move = false;
    public bool attack = false;
    public bool ability1 = false;
    public bool ability2 = false;
    public bool ability3 = false;
    public Vector3 mousePosInGame = Vector3.zero;
}
public class PlayerInputScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private Camera cam;

    private Vector2 mousePos;
    public Vector3 mousePosInGame;
    public Vector3 lastRightClick;
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

    public bool newInput = false;

    public GameObject target;

    private AgentStats stats;

    //public PlayerInputSerializedData serializedData;

    void Start()
    {
        mousePos = new Vector2(0, 0);
        mousePosInGame = new Vector3(0, 0, 0);

        cam = Camera.main;

        stats = GetComponent<AgentStats>();
        //if(stats.friendlyLayer == 9)
        //{
        //    serializedData.isBlue = true;
        //}
        //else
        //{
        //    serializedData.isBlue = false;
        //}
    }

    // Update is called once per frame
    public void tickUpdate()
    {
        if (ability1)
        {
            ability1 = false;
        }
        if (ability2)
        {
            ability2 = false;
        }
        if (ability3)
        {
            ability3 = false;
        }
        if(newInput)
        {
            newInput = false;
        }

        Vector2 currentMousePos = Mouse.current.position.ReadValue();

        if (mouseOnScreen(currentMousePos))
        {
            //Debug.Log("mouse on screen");
            lastDirection = 0;

            mousePos = currentMousePos;

            
            RaycastHit hit;

            GameObject tempTarget = null;

            if (Physics.Raycast(cam.ScreenPointToRay(mousePos), out hit, Mathf.Infinity))
            {
                mousePosInGame = hit.point;

                if (Mouse.current.rightButton.isPressed || Mouse.current.rightButton.wasPressedThisFrame)
                {
                    if (hit.collider.gameObject.layer == stats.groundLayer)
                    {
                        move = true;
                        attack = false;
                    }
                    else if (hit.collider.gameObject.layer == stats.enemyLayer)
                    {
                        attack = true;
                        move = false;

                        if (hit.transform.parent)
                        {
                            tempTarget = hit.transform.parent.gameObject;
                        }
                        else if (hit.collider.gameObject != null)
                        {
                            tempTarget = hit.transform.gameObject;
                        }
                    }

                    lastRightClick = mousePosInGame;
                    target = tempTarget;
                    newInput = true;
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

        if (Input.GetKeyDown(ability2Key))
        {
            ability2 = true;
            newInput = true;
        }

        if (Input.GetKeyDown(ability3Key))
        {
            ability3 = true;
            newInput = true;
        }

        //if(newInput)
        //{
        //    serializedData.move = move;

        //    serializedData.attack = attack;
        //    serializedData.ability1 = ability1;
        //    serializedData.ability2 = ability2;
        //    serializedData.ability3 = ability3;
        //    serializedData.mousePosInGame = mousePosInGame;
        //}
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


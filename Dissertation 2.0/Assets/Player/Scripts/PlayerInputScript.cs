using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private Camera cam;

    private Vector2 mousePos;
    public Vector3 mousePosInGame;
    public Vector3 lastRightClick;

    public int lastDirection = 0;

    public KeyCode ability1Key;
    public KeyCode ability2Key;
    public KeyCode ability3Key;

    public bool move = false;
    public bool attack = false;
    public bool ability1 = false;
    public bool ability2 = false;
    public bool ability3 = false;

    public GameObject target;

    void Start()
    {
        mousePos = new Vector2(0, 0);
        mousePosInGame = new Vector3(0, 0, 0);

        cam = Camera.main;
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
                if (hit.collider.gameObject.layer == 8)
                {
                    move = true;
                }
                else if (hit.collider.gameObject.layer == 10)
                {
                    attack = true;

                    if (hit.transform.parent)
                    {
                        tempTarget = hit.transform.parent.gameObject;
                    }
                    else if (hit.collider.gameObject != null)
                    {
                        tempTarget = hit.transform.gameObject;
                    }
                }

                mousePosInGame = hit.point;

                if (Mouse.current.rightButton.isPressed || Mouse.current.rightButton.wasPressedThisFrame)
                {
                    lastRightClick = mousePosInGame;
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
        }

        if (Input.GetKeyDown(ability2Key))
        {
            ability2 = true;
        }

        if (Input.GetKeyDown(ability3Key))
        {
            ability3 = true;
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
}


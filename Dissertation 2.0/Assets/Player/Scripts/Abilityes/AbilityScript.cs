using Unity.Netcode;
using UnityEngine;

public class AbilityScript : NetworkBehaviour
{
    [HideInInspector]
    public GameObject player;

    private PlayerInputScript playerInput;

    //PlayerManager playerScript;
    AgentStats stats;

    [HideInInspector]
    public int enemyLayer;
    [HideInInspector]
    public int friendlyLayer;
    [HideInInspector]
    public string enemyTowerTag;
    [HideInInspector]
    public string enemyPlayerTag;
    [HideInInspector]
    public string enemyMinionTag;

    public float cooldown = 5;
    [HideInInspector]
    public float cooldownTimer = 0;
    [HideInInspector]
    public bool isAvailable = true;
    protected bool isActive = false;
    protected Vector3 initialPosition;
    protected Vector3 targetPosition;

    public void Update()
    {
        if (!IsOwner)
        { return; }

        if (cooldownTimer > 0)
        {
            isAvailable = false;
            cooldownTimer -= Time.deltaTime;
        }
        if (cooldownTimer <= 0)
        {
            isAvailable = true;
            cooldownTimer = 0;
        }

        //if(!isActive)
        //{
        //    if(gameObject.GetComponent<MeshRenderer>())
        //    {
        //        gameObject.GetComponent<MeshRenderer>().enabled = false;
        //    }
        //    if(gameObject.GetComponent<SphereCollider>())
        //    {
        //        gameObject.GetComponent<SphereCollider>().enabled = false;
        //    }
        //}
    }

    // Update is called once per frame
    public virtual void action()
    {

    }

    public virtual void initialize(GameObject playerObj)
    {
        player = playerObj;

        stats = player.GetComponent<AgentStats>();
        playerInput = player.GetComponent<PlayerInputScript>();
        //playerScript = player.GetComponent<PlayerManager>();

        enemyLayer = stats.enemyLayer;
        friendlyLayer = stats.friendlyLayer;

        enemyMinionTag = stats.enemyMinionTag;
        enemyPlayerTag = stats.enemyPlayerTag;
        enemyTowerTag = stats.enemyTowerTag;
    }

    public Vector3 getMousePos()
    {
        //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        //if (Physics.Raycast(ray, out RaycastHit hit))
        //{

        //    return hit.point;
        //}

        //return Vector3.zero;

        return playerInput.mousePosInGame;
    }
}

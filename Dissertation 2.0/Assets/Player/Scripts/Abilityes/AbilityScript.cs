using UnityEngine;

public class AbilityScript : MonoBehaviour
{
    public GameObject player;

    //PlayerManager playerScript;
    AgentStats stats;

    public int enemyLayer;
    public int friendlyLayer;
    public string enemyTowerTag;
    public string enemyPlayerTag;
    public string enemyMinionTag;

    public float cooldown = 5;
    public float cooldownTimer = 0;
    [HideInInspector]
    public bool isAvailable = true;
    protected bool isActive = false;

    protected Vector3 initialPosition;
    protected Vector3 targetPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    //public void Start()
    //{
    //    stats = GetComponent<AgentStats>();
    //    //playerScript = player.GetComponent<PlayerManager>();

    //    enemyLayer = stats.enemyLayer;
    //    friendlyLayer = stats.friendlyLayer;

    //    enemyMinionTag = stats.enemyMinionTag;
    //    enemyPlayerTag = stats.enemyPlayerTag;
    //    enemyTowerTag = stats.enemyTowerTag;
    //}

    public void Update()
    {
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

    public void initialize(GameObject playerObj)
    {
        player = playerObj;

        stats = player.GetComponent<AgentStats>();
        //playerScript = player.GetComponent<PlayerManager>();

        enemyLayer = stats.enemyLayer;
        friendlyLayer = stats.friendlyLayer;

        enemyMinionTag = stats.enemyMinionTag;
        enemyPlayerTag = stats.enemyPlayerTag;
        enemyTowerTag = stats.enemyTowerTag;
    }

    public Vector3 getMousePos()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {

            return hit.point;
        }

        return Vector3.zero;
    }
}

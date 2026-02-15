using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[Serializable]
public class PlayerSerializedData
{
    public string objectType = "player";
    public bool isBlue = false;
    public float health;
    public Vector3 position = new Vector3(0, 0, 0);
    public bool ability1 = true;
    public bool ability2 = true;
    public bool ability3 = true;
    public float ability1CD = 0;
    public float ability2CD = 0;
    public float ability3CD = 0;
    public int creepScore = 0;

}

public class PlayerManager : NetworkBehaviour
{
    public PlayerSerializedData serializedPlayer;

    //public GameObject gameInfo;

    public bool isAI = false;
    //public bool isLocal = true;

    //private int groundLayer = 8;
    //private int enemyLayer;
    //private int friendlyLayer;

    //private GameObject enemyPlayer;
    //private GameObject enemyTower;

    //private string enemyTowerTag;
    //private string enemyMinionTag;
    //private string enemyPlayerTag;

    private PlayerInputScript playerInput;
    private PlayerMovement playerMovement;
    private PlayerAttackScript playerAttack;
    private AgentStats stats;

    public int creepScore = 0;

    public Text creepScoreUi;

    public Canvas uiCanvas;

    //public override void OnNetworkSpawn()
    //{
    //    if(!IsOwner) Destroy(this);
    //}

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerInput = GetComponent<PlayerInputScript>();
        playerMovement = GetComponent<PlayerMovement>();
        playerAttack = GetComponent<PlayerAttackScript>();
        stats = GetComponent<AgentStats>();

        if(IsOwner)
        {
            uiCanvas.gameObject.SetActive(true);

            GameObject.FindGameObjectWithTag("HighlightManager").GetComponent<HighlightManager>().setup(stats.enemyLayer);

        }

        if (gameObject.layer == 9)
        {
            serializedPlayer.isBlue = true;
        }
        else
        {
            serializedPlayer.isBlue = false;
        }
        serializedPlayer.position = transform.position;
        serializedPlayer.health = stats.health;

        GameManagerScript.Instance.setupPlayerReferences(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        serializedPlayer.health = stats.targetHealth;
        serializedPlayer.position = transform.position;
        serializedPlayer.ability1 = playerAttack.rootIsAvailable;
        serializedPlayer.ability2 = playerAttack.dashIsAvailable;
        serializedPlayer.ability3 = playerAttack.ultIsAvailable;
        serializedPlayer.ability1CD = playerAttack.rootCooldownTimer;
        serializedPlayer.ability2CD = playerAttack.dashCooldownTimer;
        serializedPlayer.ability3CD = playerAttack.ultCooldownTimer;
        serializedPlayer.creepScore = creepScore;

        //ai input
        if (!IsOwner)
        {
            return;
        }
        playerInput.tickUpdate();
        playerMovement.tickUpdate();
        playerAttack.tickUpdate();

        creepScoreUi.text = "CS: "+creepScore.ToString();
    }

    public void resetPlayerComponents()
    {
        playerInput.resetComponent();
        playerMovement.resetComponent();
    }

    [Rpc(SendTo.Server)]
    public void aggroAllInRangeRpc()
    {
        GameObject[] enemyMinions = GameObject.FindGameObjectsWithTag(stats.enemyMinionTag);

        if (enemyMinions.Length > 0)
        {
            float minionAggroRange = enemyMinions[0].GetComponent<MinionManager>().aggroDistance;

            foreach (GameObject minion in enemyMinions)
            {
                float distance = Vector3.Distance(gameObject.transform.position, minion.transform.position);

                if (distance < minionAggroRange)
                {
                    minion.GetComponent<MinionManager>().currentTarget = gameObject;
                    minion.GetComponent<MinionManager>().targetSwitchTimer = 3;
                }
            }
        }
        GameObject enemyTower = GameObject.FindGameObjectWithTag(stats.enemyTowerTag);

        float distanceToTower = Vector3.Distance(gameObject.transform.position, enemyTower.transform.position);
        if (distanceToTower < enemyTower.GetComponent<TowerScript>().range)
        {
            enemyTower.GetComponent<TowerScript>().currentTarget = gameObject;
        }
    }

    [Rpc(SendTo.Owner)]
    public void incrementCsRpc()
    {
        ++creepScore;
    }
}

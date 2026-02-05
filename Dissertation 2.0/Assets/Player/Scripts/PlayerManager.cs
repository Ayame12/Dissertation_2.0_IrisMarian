using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : NetworkBehaviour
{
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

    public int creepScore = 0;

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

        if(IsOwner)
        {
            uiCanvas.gameObject.SetActive(true);

            playerAttack.initialize();
        }
    }

    // Update is called once per frame
    void Update()
    {
        //ai input
        if(!IsOwner)
        {
            return;
        }
        playerInput.tickUpdate();
        playerMovement.tickUpdate();
        playerAttack.tickUpdate();

    }
}

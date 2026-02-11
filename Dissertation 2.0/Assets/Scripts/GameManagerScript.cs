using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManagerScript : NetworkBehaviour
{
    public enum Scene
    {
        MenuScene,
        GameSetupScene,
        GameScene,
    }


    public string bluePlayerTag;
    public string redPlayerTag;

    public GameObject bluePlayerPrefab;
    public GameObject redPlayerPrefab;

    //public GameObject blueRootPrefab;
    //public GameObject redRootPrefab;

    public CinemachineCamera cineCam;

    private GameObject bluePlayer;
    private GameObject redPlayer;

    private Transform bluePlayerSpawnPoint;
    private Transform redPlayerSpawnPoint;
    public float bluePlayerRespawnCooldown;
    public float redPlayerRespawnCooldown;
    private float bluePlayerRespawnTimer;
    private float redPlayerRespawnTimer;
    public float respawnIncrement;

    //public bool isHost;
    //public bool waitingToLoadGame = true;

    //private bool gameStarted = false;

    //bool playersSpawned = false;
    //bool setupDone = false;

    public static GameManagerScript Instance { get; private set; }

    private void Awake()
    {
        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    public static void LoadNetwork(Scene targetScene)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(targetScene.ToString(), LoadSceneMode.Single);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DontDestroyOnLoad(gameObject);

        //cineCam.Target = 
    }

    // Update is called once per frame
    void Update()
    {

        if (bluePlayerRespawnTimer > 0)
        {
            bluePlayerRespawnTimer -= Time.deltaTime;
            if(bluePlayerRespawnTimer <= 0)
            {
                bluePlayerRespawnTimer = 0;
                handlePlayerRespawnRpc(true);
            }
        }

        if (redPlayerRespawnTimer > 0)
        {
            redPlayerRespawnTimer -= Time.deltaTime;
            if (redPlayerRespawnTimer <= 0)
            {
                redPlayerRespawnTimer = 0;
                handlePlayerRespawnRpc(false);
            }
        }

        //if(playersSpauned && !setupDone)
        //{
        //    cineCam = GameObject.FindGameObjectWithTag("CinemachineCamera").GetComponent<CinemachineCamera>();

        //    if (NetworkManager.Singleton.IsHost)
        //    {
        //        cineCam.Target.TrackingTarget = bluePlayer.transform;
        //    }
        //    else
        //    {
        //        cineCam.Target.TrackingTarget = redPlayer.transform;
        //    }

        //    setupDone = true;
        //}


        //OnLevelWasLoaded(1);
    }

    //void fuc(int level)
    //{
    //    bluePlayer = GameObject.FindGameObjectWithTag(bluePlayerTag);
    //    redPlayer = GameObject.FindGameObjectWithTag(redPlayerTag);

    //    if(isHost)
    //    {
    //        cineCam.Target.TrackingTarget = bluePlayer.transform;
    //    }
    //    else
    //    {
    //        cineCam.Target.TrackingTarget = redPlayer.transform;
    //    }
    //}

    public void StartHost()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
        NetworkManager.Singleton.StartHost();
    }

    private void NetworkManager_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        response.Approved = true;
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }

    public override void OnNetworkSpawn()
    {
        //state.OnValueChanged += State_OnValueChanged;
        //isGamePaused.OnValueChanged += IsGamePaused_OnValueChanged;

        if (IsServer)
        {
            //NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        }
    }

    private void SceneManager_OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode LoadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (SceneManager.GetActiveScene().name != Scene.GameScene.ToString())
        {
            return;
        }
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if(NetworkManager.Singleton.LocalClientId == clientId && NetworkManager.Singleton.IsHost)
            {
                bluePlayerSpawnPoint = GameObject.FindGameObjectWithTag("BluePlayerSpawn").transform;

                bluePlayer = Instantiate(bluePlayerPrefab, bluePlayerSpawnPoint);
                bluePlayer.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
            }
            else
            {
                redPlayerSpawnPoint = GameObject.FindGameObjectWithTag("RedPlayerSpawn").transform;

                redPlayer = Instantiate(redPlayerPrefab, redPlayerSpawnPoint);
                redPlayer.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
            }  
        }

        //playersSpawned = true;
    }

    [Rpc(SendTo.Everyone)]
    public void handlePlayerDeathRpc(bool isBluePlayer)
    {

        if (isBluePlayer)
        {
            bluePlayer.SetActive(false);
            bluePlayerRespawnTimer = bluePlayerRespawnCooldown;
            bluePlayerRespawnCooldown += respawnIncrement;

            bluePlayer.transform.position = bluePlayerSpawnPoint.position;
            bluePlayer.transform.rotation = bluePlayerSpawnPoint.rotation;
        }
        else
        {
            redPlayer.SetActive(false);
            redPlayerRespawnTimer = redPlayerRespawnCooldown;
            redPlayerRespawnCooldown += respawnIncrement;

            redPlayer.transform.position = redPlayerSpawnPoint.position;
            redPlayer.transform.rotation = redPlayerSpawnPoint.rotation;
        }
    }

    [Rpc(SendTo.Everyone)]
    public void handlePlayerRespawnRpc(bool isBluePlayer)
    {
        if(isBluePlayer)
        {
            bluePlayer.SetActive(true);
        }
        else
        {
            redPlayer.SetActive(true);
        }

        //if(IsOwner)
        //{
        //    if (player == bluePlayer)
        //    {
        //        bluePlayerRespawnTimer = bluePlayerRespawnCooldown;
        //        bluePlayerRespawnCooldown += respawnIncrement;

        //        player.transform.position = bluePlayerSpawnPoint.position;
        //        player.transform.rotation = bluePlayerSpawnPoint.rotation;
        //    }
        //    else
        //    {
        //        redPlayerRespawnTimer = redPlayerRespawnCooldown;
        //        redPlayerRespawnCooldown += respawnIncrement;

        //        player.transform.position = redPlayerSpawnPoint.position;
        //        player.transform.rotation = redPlayerSpawnPoint.rotation;
        //    }
        //}
    }

    public void setupPlayerReferences(GameObject player)
    {
        if (player.tag == bluePlayerTag)
        {
            bluePlayer = player;
            GameObject.FindGameObjectWithTag("RedTower").GetComponent<TowerScript>().setupPlayerRef(bluePlayer);
        }
        else
        {
            redPlayer = player;
            GameObject.FindGameObjectWithTag("BlueTower").GetComponent<TowerScript>().setupPlayerRef(redPlayer);
        }
    }

    public void setupPlayerSpawnPointTransforms(bool isBlueSpawn, Transform spawnPoint)
    {
        if (isBlueSpawn)
        {
            bluePlayerSpawnPoint = spawnPoint;
        }
        else
        {
            redPlayerSpawnPoint = spawnPoint;
        }
    }
}

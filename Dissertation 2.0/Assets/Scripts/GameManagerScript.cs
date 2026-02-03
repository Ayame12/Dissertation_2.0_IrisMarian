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

    public CinemachineCamera cineCam;
    private GameObject bluePlayer;
    private GameObject redPlayer;

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
                bluePlayer = Instantiate(bluePlayerPrefab);
                bluePlayer.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
            }
            else
            {
                redPlayer = Instantiate(redPlayerPrefab);
                redPlayer.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
            }  
        }

        //playersSpawned = true;
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.Cinemachine;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;

[Serializable]
class currentTimestamp
{
    public string logType = "log";
    public int minutesElapsed = 0;
    public int secondsElapsed = 0;
    public int milisecondsElapsed = 0;

    public int blueMinionsAlive = 0;
    public int redMinionsAlive = 0;
}

public class GameManagerScript : NetworkBehaviour
{
    public enum Scene
    {
        MenuScene,
        GameSetupScene,
        GameScene,
        GameEnd,
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
    private GameObject blueTower;
    private GameObject redTower;

    private Transform bluePlayerSpawnPoint;
    private Transform redPlayerSpawnPoint;
    public float bluePlayerRespawnCooldown;
    public float redPlayerRespawnCooldown;
    private float bluePlayerRespawnTimer;
    private float redPlayerRespawnTimer;
    public float respawnIncrement;

    public bool blueWins;
    public bool gameDone;
    public Text winnerText;

    bool logGame = true;
    bool logHostOnly = true;
    public string saveFilePath;
    string json;
    public float writeFrequency;
    private float writeTimer;
    public float serializeLogFrequency;
    private float serializeLogTimer;
    private bool serializeAll = true;
    private bool startLogging = false;
    public System.Diagnostics.Stopwatch stopwatch;

    currentTimestamp timestamp;
    

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

        string docPath = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        int m = System.DateTime.Now.Month;
        int d = System.DateTime.Now.Day;
        int h = System.DateTime.Now.Hour;
        int s = System.DateTime.Now.Minute;

        string dateAndTimeOfGame = m.ToString() +"."+ d.ToString() + "_" + h.ToString() + "." + s.ToString();

        saveFilePath = Path.Combine(Application.persistentDataPath, docPath + "\\DissertationData\\iris_gameData_" + dateAndTimeOfGame + ".json");

        stopwatch = new System.Diagnostics.Stopwatch();
        timestamp = new currentTimestamp();
        //saveFilePath = "C:\\Users\\2200147\\Documents\\DissertationData\\iris_playerData.json";
    }

    // Update is called once per frame
    void Update()
    {
        if (bluePlayerRespawnTimer > 0)
        {
            bluePlayerRespawnTimer -= Time.deltaTime;
            if (bluePlayerRespawnTimer <= 0)
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


        if (startLogging)
        {
            if (logGame)
            {
                if ((logHostOnly && IsHost) || !logHostOnly)
                {
                    serializeLogTimer -= Time.deltaTime;
                    writeTimer -= Time.deltaTime;

                    if (serializeLogTimer <= 0)
                    {
                        timestamp.logType = "log";
                        serializeLogTimer = serializeLogFrequency;
                        serializeGameState();
                    }
                    else if (gameDone)
                    {
                        //int seconds = stopwatch.Elapsed.Seconds;

                        timestamp.minutesElapsed = stopwatch.Elapsed.Minutes;
                        timestamp.secondsElapsed = stopwatch.Elapsed.Seconds;
                        timestamp.milisecondsElapsed = stopwatch.Elapsed.Milliseconds;

                        json += JsonUtility.ToJson(timestamp, true);
                        json += JsonUtility.ToJson(bluePlayer.GetComponent<PlayerManager>().serializedPlayer, true);
                        json += JsonUtility.ToJson(redPlayer.GetComponent<PlayerManager>().serializedPlayer, true);

                        writeToFile(json, saveFilePath);
                    }

                }
            }
        }

        if (gameDone)
        {
            if(false)
            {
                GameObject textObj = GameObject.FindGameObjectWithTag("winnerText");
                winnerText = textObj.GetComponent<Text>();

                if (blueWins)
                {
                    winnerText.text = "Blue Wins!";
                }
                else
                {
                    winnerText.text = "Red Wins!";
                }
            }
        }
    }

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
        if (IsServer)
        {
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
    }

    public void setupPlayerReferences(GameObject player)
    {
        if (player.tag == bluePlayerTag)
        {
            bluePlayer = player;
            redTower = GameObject.FindGameObjectWithTag("RedTower");
            redTower.GetComponent<TowerScript>().setupPlayerRef(bluePlayer);
        }
        else
        {
            redPlayer = player;
            blueTower = GameObject.FindGameObjectWithTag("BlueTower");
            blueTower.GetComponent<TowerScript>().setupPlayerRef(redPlayer);
        }

        if(bluePlayer != null && redPlayer != null)
        {
            startLogging = true;
            stopwatch.Start();
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

    private void serializeGameState()
    {
        timestamp.minutesElapsed = stopwatch.Elapsed.Minutes;
        timestamp.secondsElapsed = stopwatch.Elapsed.Seconds;
        timestamp.milisecondsElapsed = stopwatch.Elapsed.Milliseconds;

        GameObject[] blueMinions = GameObject.FindGameObjectsWithTag("BlueMinion");
        GameObject[] redMinions = GameObject.FindGameObjectsWithTag("RedMinion");

        timestamp.blueMinionsAlive = blueMinions.Length;
        timestamp.redMinionsAlive = redMinions.Length;

        json += JsonUtility.ToJson(timestamp, true);
        json += JsonUtility.ToJson(bluePlayer.GetComponent<PlayerManager>().serializedPlayer, true);
        json += JsonUtility.ToJson(redPlayer.GetComponent<PlayerManager>().serializedPlayer, true);

        if(serializeAll)
        {
            json += JsonUtility.ToJson(blueTower.GetComponent<TowerScript>().serializedTower, true);
            json += JsonUtility.ToJson(redTower.GetComponent<TowerScript>().serializedTower, true);

            foreach(GameObject blueMinion in blueMinions)
            {
                json += JsonUtility.ToJson(blueMinion.GetComponent<MinionManager>().serializedMinion, true);
            }

            foreach (GameObject blueMinion in blueMinions)
            {
                json += JsonUtility.ToJson(blueMinion.GetComponent<MinionManager>().serializedMinion, true);
            }
        }
        serializeAll = !serializeAll;

        if (writeTimer <= 0 )
        {
            writeTimer = writeFrequency;
            if(writeToFile(json, saveFilePath))
            {
                json = string.Empty;
            }

        }
    }
    private bool writeToFile(string data, string filePath)
    {
        try
        {
            File.AppendAllText(filePath, data);
            Debug.Log("Players saved to" + filePath);
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError("Error saving player position");
        }
        return false;
    }
}

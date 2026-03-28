using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

public class AI_PickAction : MonoBehaviour
{
    public string statesDataFile;

    private StatesList states;

    public float checkStateFrequency;
    private float checkStateTimer;

    private bool checkState = false;

    //private GameObject redPlayer;
    //private GameObject bluePlayer;

    private GameObject enemyPlayer;
    private GameObject enemyTower;
    private GameObject friendlyTower;
    private Vector2 enemyTowerPos;
    private Vector2 friendlyTowerPos;
    private string friendlyMinionTag;
    private string enemyMinionTag;

    //Vector2 bluePlayerSpawn;
    //Vector2 redPlayerSpawn;

    public float maxMinionClusterDistance = 2;

    private float[] distanceIncrementsPlayerToEnemyTower = { 6, 10, 18, 27, 35, 40 };
    private float[] distanceIncrementsPlayerToPlayer = { 6, 10, 15, 20 };
    private float[] distanceIncrementsPlayerToEnemyWave = { 6, 10, 15, 20 };
    private float[] distanceIncrementsWaveToEnemyTower = { 6, 10, 18, 27, 35, 40 };
    private int[] playerHealthIncrements = { 200, 400 };

    private AI_BehavorTree behaviorTree;

    public Vector2 towerReactionTime = new Vector2(0.1f, 0.2f);
    private float towerReactionTimer = 0.1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        string exeFolder = Directory.GetParent(Application.dataPath).FullName;

        string s = exeFolder + statesDataFile;

        string rawData = File.ReadAllText(s);

        states = JsonUtility.FromJson<StatesList>(rawData);

        behaviorTree = GetComponent<AI_BehavorTree>();
    }

    // Update is called once per frame
    public void tickUpdate()
    {
        if(!enemyPlayer)
        {
            if (gameObject.tag == "RedPlayer")
            {
                enemyPlayer = GameObject.FindGameObjectWithTag("BluePlayer");

                if (!enemyPlayer)
                {
                    return;
                }

                enemyTower = GameObject.FindGameObjectWithTag("BlueTower");
                friendlyTower = GameObject.FindGameObjectWithTag("RedTower");
                enemyTowerPos = new Vector2(-16.5f, -16.5f);
                friendlyTowerPos = new Vector2(16.5f, 16.5f);
                friendlyMinionTag = "RedMinion";
                enemyMinionTag = "BlueMinion";
            }
            else
            {
                enemyPlayer = GameObject.FindGameObjectWithTag("RedPlayer");

                if (!enemyPlayer)
                {
                    return;
                }

                enemyTower = GameObject.FindGameObjectWithTag("RedTower");
                friendlyTower = GameObject.FindGameObjectWithTag("BlueTower");
                enemyTowerPos = new Vector2(16.5f, 16.5f);
                friendlyTowerPos = new Vector2(-16.5f, -16.5f);
                friendlyMinionTag = "BlueMinion";
                enemyMinionTag = "RedMinion";
            }

            if (!enemyPlayer)
            {
                return;
            }
        }

        checkStateTimer -= Time.deltaTime;

        if(checkStateTimer < 0)
        {
            checkState = true;
        }

        if (checkState)
        {
            checkState = false;

            Vector2 friendlyPlayerPos = new Vector2(transform.position.x, transform.position.z);
            Vector2 enemyPlayerPos = new Vector2(enemyPlayer.transform.position.x, enemyPlayer.transform.position.z);

            Vector2 friendlyMinionCentre = new Vector2(0, 0);
            Vector2 enemyMinionCentre = new Vector2(0, 0);

            List<GameObject> friendlyFrontMinionCluster = new List<GameObject>();
            List<GameObject> enemyFrontMinionCluster = new List<GameObject>();

            {
                GameObject[] friendlyMinions = GameObject.FindGameObjectsWithTag(friendlyMinionTag);
                GameObject[] enemyMinions = GameObject.FindGameObjectsWithTag(enemyMinionTag);

                GameObject friendlyFrontMinion;
                GameObject enemyFrontMinion;

                float shortestFriendlyMinionDistanceToTower = Vector2.Distance(friendlyTowerPos, enemyPlayerPos);
                float shortestEnemyMinionDistanceToTower = Vector2.Distance(friendlyTowerPos, enemyPlayerPos);

                //friendly minion wave
                foreach (GameObject minion in friendlyMinions)
                {
                    Vector2 minionPos = new Vector2(minion.transform.position.x, minion.transform.position.z);

                    float distanceToTower = Vector2.Distance(enemyTowerPos, minionPos);
                    if (distanceToTower < shortestFriendlyMinionDistanceToTower)
                    {
                        shortestFriendlyMinionDistanceToTower = distanceToTower;
                        friendlyFrontMinion = minion;
                        friendlyMinionCentre = new Vector2(friendlyFrontMinion.transform.position.x, friendlyFrontMinion.transform.position.z);
                    }
                }

                foreach (GameObject minion in friendlyMinions)
                {
                    Vector2 minionPosition = new Vector2(minion.transform.position.x, minion.transform.position.z);
                    float distanceFromCentre = Vector2.Distance(friendlyMinionCentre, minionPosition);
                    if (distanceFromCentre < maxMinionClusterDistance)
                    {
                        friendlyFrontMinionCluster.Add(minion);

                        float xCentre = 0;
                        float zCentre = 0;

                        foreach (GameObject minionInList in friendlyFrontMinionCluster)
                        {
                            xCentre += minionInList.transform.position.x;
                            zCentre += minionInList.transform.position.z;
                        }

                        xCentre = xCentre / friendlyFrontMinionCluster.Count;
                        zCentre = zCentre / friendlyFrontMinionCluster.Count;

                        friendlyMinionCentre.x = xCentre;
                        friendlyMinionCentre.y = zCentre;
                    }
                }

                //red minion wave
                foreach (GameObject minion in enemyMinions)
                {
                    Vector2 minionPos = new Vector2(minion.transform.position.x, minion.transform.position.z);

                    float distanceToTower = Vector2.Distance(friendlyTowerPos, minionPos);
                    if (distanceToTower < shortestEnemyMinionDistanceToTower)
                    {
                        shortestEnemyMinionDistanceToTower = distanceToTower;
                        enemyFrontMinion = minion;
                        enemyMinionCentre = new Vector2(enemyFrontMinion.transform.position.x, enemyFrontMinion.transform.position.z);
                    }
                }

                foreach (GameObject minion in enemyMinions)
                {
                    Vector2 minionPosition = new Vector2(minion.transform.position.x, minion.transform.position.z);
                    float distanceFromCentre = Vector2.Distance(enemyMinionCentre, minionPosition);
                    if (distanceFromCentre < maxMinionClusterDistance)
                    {
                        enemyFrontMinionCluster.Add(minion);

                        float xCentre = 0;
                        float zCentre = 0;

                        foreach (GameObject minionInList in enemyFrontMinionCluster)
                        {
                            xCentre += minionInList.transform.position.x;
                            zCentre += minionInList.transform.position.z;
                        }

                        xCentre = xCentre / enemyFrontMinionCluster.Count;
                        zCentre = zCentre / enemyFrontMinionCluster.Count;

                        enemyMinionCentre.x = xCentre;
                        enemyMinionCentre.y = zCentre;
                    }
                }
            }

            int playerToEnemyTowerDistanceIndex;
            int playersDistanceIndex;
            int playerToEnemyWaveDistanceIndex;
            int waveToEnemyTowerDistanceIndex;

            float playerToEnemyTowerDistance = Vector2.Distance(friendlyPlayerPos, enemyTowerPos);
            float playersDistance = Vector2.Distance(friendlyPlayerPos, enemyPlayerPos);
            float playerToEnemyWaveDistance = Vector2.Distance(friendlyPlayerPos, enemyMinionCentre);
            float waveToEnemyTowerDistance = Vector2.Distance(friendlyMinionCentre, enemyTowerPos);

            {
                int index = 0;
                for (int i = 0; i < distanceIncrementsPlayerToEnemyTower.Length; ++i)
                {
                    index = i;
                    if (playerToEnemyTowerDistance < distanceIncrementsPlayerToEnemyTower[i])
                    {
                        break;
                    }
                    if (i == distanceIncrementsPlayerToEnemyTower.Length - 1)
                    {
                        ++index;
                    }
                }
                ++index;
                playerToEnemyTowerDistanceIndex = index;
            }

            if (!enemyPlayer.GetComponent<AgentStats>().isAlive)
            {
                playersDistanceIndex = 0;
            }
            else
            {
                int index = 0;
                for (int i = 0; i < distanceIncrementsPlayerToPlayer.Length; ++i)
                {
                    index = i;
                    if (playersDistance < distanceIncrementsPlayerToPlayer[i])
                    {
                        break;
                    }
                    if (i == distanceIncrementsPlayerToPlayer.Length - 1)
                    {
                        ++index;
                    }
                }
                ++index;
                playersDistanceIndex = index;
            }

            if (enemyFrontMinionCluster.Count == 0)
            {
                playerToEnemyWaveDistanceIndex = 0;
            }
            else
            {
                int index = 0;
                for (int i = 0; i < distanceIncrementsPlayerToEnemyWave.Length; ++i)
                {
                    index = i;
                    if (playerToEnemyWaveDistance < distanceIncrementsPlayerToEnemyWave[i])
                    {
                        break;
                    }
                    if (i == distanceIncrementsPlayerToEnemyWave.Length - 1)
                    {
                        ++index;
                    }
                }
                ++index;
                playerToEnemyWaveDistanceIndex = index;
            }

            if (friendlyFrontMinionCluster.Count == 0)
            {
                waveToEnemyTowerDistanceIndex = 0;
            }
            else
            {
                int index = 0;
                for (int i = 0; i < distanceIncrementsWaveToEnemyTower.Length; ++i)
                {
                    index = i;
                    if (waveToEnemyTowerDistance < distanceIncrementsWaveToEnemyTower[i])
                    {
                        break;
                    }
                    if (i == distanceIncrementsWaveToEnemyTower.Length - 1)
                    {
                        ++index;
                    }
                }
                ++index;
                waveToEnemyTowerDistanceIndex = index;
            }

            int bluePlayerHealthIncrementIndex = playerHealthIncrements.Length;
            int redPlayerHealthIncrementIndex = playerHealthIncrements.Length;

            for (int i = 0; i < playerHealthIncrements.Length; ++i)
            {
                if (GetComponent<AgentStats>().isAlive)
                {
                    if (GetComponent<AgentStats>().health < playerHealthIncrements[i])
                    {
                        bluePlayerHealthIncrementIndex = i + 1;
                        break;
                    }
                }
                else
                {
                    bluePlayerHealthIncrementIndex = 0;
                    break;
                }
            }

            for (int i = 0; i < playerHealthIncrements.Length; ++i)
            {
                if (enemyPlayer.GetComponent<AgentStats>().isAlive)
                {
                    if (enemyPlayer.GetComponent<AgentStats>().health < playerHealthIncrements[i])
                    {
                        redPlayerHealthIncrementIndex = i + 1;
                        break;
                    }
                }
                else
                {
                    redPlayerHealthIncrementIndex = 0;
                    break;
                }
            }

            int stateID = 0;
            stateID += playerToEnemyTowerDistanceIndex * 100000;
            stateID += playersDistanceIndex * 10000;
            stateID += playerToEnemyWaveDistanceIndex * 1000;
            stateID += waveToEnemyTowerDistanceIndex * 100;
            stateID += bluePlayerHealthIncrementIndex * 10;
            stateID += redPlayerHealthIncrementIndex * 1;

            float playerActionVal = 0;
            float clearActionVal = 0;
            float towerActionVal = 0;
            float backActionVal = 0;

            bool stateFound = false;
            foreach(State s in states.states)
            {
                if(s.stateID == stateID)
                {
                    playerActionVal = s.trade / (float)s.frequency;
                    clearActionVal = s.wave / (float)s.frequency;
                    towerActionVal = s.tower / (float)s.frequency;
                    backActionVal = s.back / (float)s.frequency;
                    stateFound = true;
                    break;
                }
            }

            if (stateFound)
            {
                

                float r = Random.Range(0.0f, 1.0f);

                if (enemyFrontMinionCluster.Count == 0)
                {
                    if(!enemyPlayer.activeInHierarchy)
                    {
                        if(friendlyFrontMinionCluster.Count == 0 || !isSafeToTower())
                        {
                            playerActionVal = 0;
                            clearActionVal = 0;
                            towerActionVal = 0;
                            backActionVal = 1;
                        }
                        else
                        {
                            float n = playerActionVal + clearActionVal;
                            playerActionVal = 0;
                            clearActionVal = 0;
                            towerActionVal += n / 2;
                            backActionVal += n / 2;
                        }
                    }
                    else
                    {
                        if (friendlyFrontMinionCluster.Count == 0 || !isSafeToTower())
                        {
                            float n = clearActionVal + towerActionVal;
                            playerActionVal += n / 2;
                            clearActionVal = 0;
                            towerActionVal = 0;
                            backActionVal += n / 2;
                        }
                        else
                        {
                            float n = clearActionVal;
                            playerActionVal += n / 3;
                            clearActionVal = 0;
                            towerActionVal += n / 3;
                            backActionVal += n / 3;
                        }
                    }
                }
                else
                {
                    if (!enemyPlayer.activeInHierarchy)
                    {
                        if (friendlyFrontMinionCluster.Count == 0 || !isSafeToTower())
                        {
                            float n = playerActionVal + towerActionVal;
                            playerActionVal = 0;
                            clearActionVal += n / 2;
                            towerActionVal = 0;
                            backActionVal += n / 2;
                        }
                        else
                        {
                            float n = playerActionVal;
                            playerActionVal = 0;
                            clearActionVal += n / 3;
                            towerActionVal += n / 3;
                            backActionVal += n / 3;
                        }
                    }
                    else
                    {
                        if (friendlyFrontMinionCluster.Count == 0 || !isSafeToTower())
                        {
                            float n = towerActionVal;
                            playerActionVal += n / 3;
                            clearActionVal += n / 3;
                            towerActionVal = 0;
                            backActionVal += n / 3;
                        }
                    }
                }

                if (r < playerActionVal)
                {

                    behaviorTree.currentAction = ActionType.trade;
                    checkStateTimer = 2f;
                    Debug.Log("Action : " + behaviorTree.currentAction.ToString() + " for " + checkStateTimer.ToString());
                    return;
                }
                if (r < playerActionVal + clearActionVal)
                {
                    behaviorTree.currentAction = ActionType.clear;
                    checkStateTimer = 2.5f;
                    Debug.Log("Action : " + behaviorTree.currentAction.ToString() + " for " + checkStateTimer.ToString());
                    return;
                }
                if(r < playerActionVal + clearActionVal + towerActionVal)
                {
                    behaviorTree.currentAction = ActionType.tower;
                    checkStateTimer = 1f;
                    Debug.Log("Action : " + behaviorTree.currentAction.ToString() + " for " + checkStateTimer.ToString());
                    return;
                }
                
                behaviorTree.currentAction = ActionType.back;
                checkStateTimer = 1f;
                Debug.Log("Action : " + behaviorTree.currentAction.ToString() + " for " + checkStateTimer.ToString());
                return;
            }
            else
            {
                if (enemyFrontMinionCluster.Count == 0)
                {
                    float r = Random.Range(0.0f, 1.0f);

                    if (r < 0.5f)
                    {
                        behaviorTree.currentAction = ActionType.trade;
                        checkStateTimer = 2f;
                        Debug.Log("Action : " + behaviorTree.currentAction.ToString() + " for " + checkStateTimer.ToString());
                        return;
                    }

                    behaviorTree.currentAction = ActionType.back;
                    checkStateTimer = 1f;
                    Debug.Log("Action : " + behaviorTree.currentAction.ToString() + " for " + checkStateTimer.ToString());
                    return;
                }
                else
                {
                    float r = Random.Range(0.0f, 1.0f);

                    if (r < 0.2f)
                    {
                        behaviorTree.currentAction = ActionType.trade;
                        checkStateTimer = 2f;
                        Debug.Log("Action : " + behaviorTree.currentAction.ToString() + " for " + checkStateTimer.ToString());
                        return;
                    }
                    if (r < 0.3f)
                    {
                        behaviorTree.currentAction = ActionType.back;
                        checkStateTimer = 1f;
                        Debug.Log("Action : " + behaviorTree.currentAction.ToString() + " for " + checkStateTimer.ToString());
                        return;
                    }

                    behaviorTree.currentAction = ActionType.clear;
                    checkStateTimer = 2.5f;
                    Debug.Log("Action : " + behaviorTree.currentAction.ToString() + " for " + checkStateTimer.ToString());
                    return;
                }
            }
        }

        if(enemyTower.GetComponent<TowerScript>().currentTarget == gameObject)
        {
            towerReactionTimer -= Time.deltaTime;
            if (towerReactionTimer <= 0)
            {
                towerReactionTimer = Random.Range(towerReactionTime.x, towerReactionTime.y);
                behaviorTree.currentAction = ActionType.back;
            }
        }
    }

    private bool isSafeToTower()
    {
        TowerScript enemyTowerComp = enemyTower.GetComponent<TowerScript>();

        if(enemyTowerComp.currentTarget == null || enemyTowerComp.currentTarget == gameObject)
        {
            return false;
        }

        return true;
    }
}
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Unity.Cinemachine;
using UnityEngine;

public class AI_PickAction : MonoBehaviour
{
    public string statesDataFile;

    private GameStatesList states;

    public float checkStateFrequency;
    private float checkStateTimer;

    private bool checkState = false;

    private GameObject enemyPlayer;
    private GameObject enemyTower;
    private GameObject friendlyTower;
    private Vector2 enemyTowerPos;
    private Vector2 friendlyTowerPos;
    private string friendlyMinionTag;
    private string enemyMinionTag;

    public int minionsAggroThreshold = 3;

    public float maxMinionClusterDistance = 2;

    private float[] distanceIncrementsPlayerToEnemyTower = { 6, 10, 18, 27, 35, 40 };
    private float[] distanceIncrementsPlayerToPlayer = { 6, 10, 15, 20 };
    private float[] distanceIncrementsPlayerToEnemyWave = { 6, 10, 15, 20 };
    private float[] distanceIncrementsWaveToEnemyTower = { 6, 10, 18, 27, 35, 40 };
    private int[] playerHealthIncrements = { 200, 400 };

    private AI_BehavorTree behaviorTree;

    public float backActionDuration = 0.75f;
    public float tradeActionDuration = 2f;
    public float clearActionDuration = 1.5f;
    public float towerActionDuration = 1.5f;

    public Vector2 towerReactionTime = new Vector2(0.1f, 0.2f);
    private float towerReactionTimer = 0.1f;

    void Start()
    {
        string exeFolder = Directory.GetParent(Application.dataPath).FullName;

        string s = exeFolder + statesDataFile;

        string rawData = File.ReadAllText(s);

        states = JsonUtility.FromJson<GameStatesList>(rawData);

        behaviorTree = GetComponent<AI_BehavorTree>();
    }

    public void tickUpdate()
    {
        //making sure references to other GameObjects are set
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

        //timer used to check before needing to pick the next action
        checkStateTimer -= Time.deltaTime;

        if(checkStateTimer < 0)
        {
            checkState = true;
        }

        GameObject[] enemyMinions = GameObject.FindGameObjectsWithTag(enemyMinionTag);

        //checking the state and determining StateID to search through states file
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
                
                GameObject friendlyFrontMinion;
                GameObject enemyFrontMinion;

                float shortestFriendlyMinionDistanceToTower = Vector2.Distance(friendlyTowerPos, enemyPlayerPos);
                float shortestEnemyMinionDistanceToTower = Vector2.Distance(friendlyTowerPos, enemyPlayerPos);

                //finding the front-most minion for the friendly wave
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

                //finding all the minions in proximity to the first minion to get the front of the friendly wave centre
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

                //finding the front-most minion for the enemy wave
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

                //finding all the minions in proximity to the first minion to get the front of the enemy wave centre
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

            //calculating all the distances required for determining the StateID
            int playerToEnemyTowerDistanceIndex;
            int playersDistanceIndex;
            int playerToEnemyWaveDistanceIndex;
            int waveToEnemyTowerDistanceIndex;

            float playerToEnemyTowerDistance = Vector2.Distance(friendlyPlayerPos, enemyTowerPos);
            float playersDistance = Vector2.Distance(friendlyPlayerPos, enemyPlayerPos);
            float playerToEnemyWaveDistance = Vector2.Distance(friendlyPlayerPos, enemyMinionCentre);
            float waveToEnemyTowerDistance = Vector2.Distance(friendlyMinionCentre, enemyTowerPos);

            //comparing distances and player health to set indexes to determine StateID
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

            //constructing StateID
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

            //retreaving action distribution from the state if found
            bool stateFound = false;
            foreach(GameState s in states.states)
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

            //redistributing action probability based on if action is safe to do
            if (stateFound)
            {
                if (enemyFrontMinionCluster.Count == 0)
                {
                    if(enemyPlayer.activeInHierarchy && isSafeToTrade())
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
                    else
                    {
                        if (friendlyFrontMinionCluster.Count == 0 || !isSafeToTower())
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
                }
                else
                {
                    if (enemyPlayer.activeInHierarchy && isSafeToTrade())
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
                    else
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
                }
                //if it is safe to hit the tower (the friendly wave is hitting it) hit the tower
                if(!enemyPlayer.activeInHierarchy && isSafeToTower())
                {
                    behaviorTree.currentAction = ActionType.tower;
                    checkStateTimer = towerActionDuration;
                    //Debug.Log("Action : " + behaviorTree.currentAction.ToString() + " for " + checkStateTimer.ToString());
                    return;
                }

                float r = Random.Range(0.0f, 1.0f);

                //picking the action based on probabilities
                if (r < playerActionVal)
                {

                    behaviorTree.currentAction = ActionType.trade;
                    checkStateTimer = tradeActionDuration;
                    //Debug.Log("Action : " + behaviorTree.currentAction.ToString() + " for " + checkStateTimer.ToString());
                    return;
                }
                if (r < playerActionVal + clearActionVal)
                {
                    behaviorTree.currentAction = ActionType.clear;
                    checkStateTimer = clearActionDuration;
                    //Debug.Log("Action : " + behaviorTree.currentAction.ToString() + " for " + checkStateTimer.ToString());
                    return;
                }
                if(r < playerActionVal + clearActionVal + towerActionVal)
                {
                    behaviorTree.currentAction = ActionType.tower;
                    checkStateTimer = towerActionDuration;
                    //Debug.Log("Action : " + behaviorTree.currentAction.ToString() + " for " + checkStateTimer.ToString());
                    return;
                }
                
                behaviorTree.currentAction = ActionType.back;
                checkStateTimer = backActionDuration;
                //Debug.Log("Action : " + behaviorTree.currentAction.ToString() + " for " + checkStateTimer.ToString());
                return;
            }
            //decision making if state is not found
            else
            {
                if (enemyFrontMinionCluster.Count == 0)
                {
                    float r = Random.Range(0.0f, 1.0f);

                    if (r < 0.5f || isSafeToTrade())
                    {
                        behaviorTree.currentAction = ActionType.trade;
                        checkStateTimer = tradeActionDuration;
                        //Debug.Log("Action : " + behaviorTree.currentAction.ToString() + " for " + checkStateTimer.ToString());
                        return;
                    }

                    behaviorTree.currentAction = ActionType.back;
                    checkStateTimer = backActionDuration;
                    //Debug.Log("Action : " + behaviorTree.currentAction.ToString() + " for " + checkStateTimer.ToString());
                    return;
                }
                else
                {
                    float r = Random.Range(0.0f, 1.0f);

                    if (r < 0.2f || isSafeToTrade())
                    {
                        behaviorTree.currentAction = ActionType.trade;
                        checkStateTimer = tradeActionDuration;
                        //Debug.Log("Action : " + behaviorTree.currentAction.ToString() + " for " + checkStateTimer.ToString());
                        return;
                    }
                    if (r < 0.3f)
                    {
                        behaviorTree.currentAction = ActionType.back;
                        checkStateTimer = backActionDuration;
                        //Debug.Log("Action : " + behaviorTree.currentAction.ToString() + " for " + checkStateTimer.ToString());
                        return;
                    }

                    behaviorTree.currentAction = ActionType.clear;
                    checkStateTimer = clearActionDuration;
                    //Debug.Log("Action : " + behaviorTree.currentAction.ToString() + " for " + checkStateTimer.ToString());
                    return;
                }
            }
        }

        //getting out of tower aggro
        if(enemyTower.GetComponent<TowerScript>().currentTarget == gameObject)
        {
            towerReactionTimer -= Time.deltaTime;
            if (towerReactionTimer <= 0)
            {
                towerReactionTimer = Random.Range(towerReactionTime.x, towerReactionTime.y);
                behaviorTree.currentAction = ActionType.back;
                checkStateTimer = backActionDuration;
                //Debug.Log("Action : " + behaviorTree.currentAction.ToString() + " for " + checkStateTimer.ToString());
                //Debug.Log("Dropping Agro");
            }
        }

        int minionsAggroed = 0;

        //getting away from minion aggro
        foreach (GameObject minion in enemyMinions)
        {
            Vector2 minionPos = new Vector2(minion.transform.position.x, minion.transform.position.z);

            if (minion.GetComponent<MinionManager>().playerAggro)
            {
                ++minionsAggroed;
            }
        }

        if (minionsAggroed >= minionsAggroThreshold)
        {
            behaviorTree.currentAction = ActionType.back;
            checkStateTimer = backActionDuration;
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

    private bool isSafeToTrade()
    {
        TowerScript enemyTowerComp = enemyTower.GetComponent<TowerScript>();

        if (enemyTowerComp.currentTarget == gameObject)
        {
            return false;
        }

        Vector2 enemypos = new Vector2(enemyPlayer.transform.position.x, enemyPlayer.transform.position.z);

        if (Vector2.Distance(enemypos, friendlyTowerPos) > 41 || Vector2.Distance(enemypos, enemyTowerPos) <= 4)
        {
            return false;
        }

        return true;
    }
}
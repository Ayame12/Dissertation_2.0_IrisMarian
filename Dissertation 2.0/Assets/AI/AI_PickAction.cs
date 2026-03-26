using System.IO;
using UnityEngine;

public class AI_PickAction : MonoBehaviour
{
    public string statesDataFile;

    private StatesList states;

    public float checkStateFrequency;
    private float checkStateTimer;

    private bool checkState = false;

    public Vector2 blueTowerPosition = new Vector2(-16.5f, -16.5f);
    public Vector2 redTowerPosition = new Vector2(16.5f, 16.5f);

    private GameObject redPlayer;
    //Vector2 bluePlayerSpawn;
    //Vector2 redPlayerSpawn;

    public float maxMinionClusterDistance = 2;

    private float[] distanceIncrementsPlayerToEnemyTower = { 6, 10, 18, 27, 35, 40 };
    private float[] distanceIncrementsPlayerToPlayer = { 6, 10, 15, 20 };
    private float[] distanceIncrementsPlayerToEnemyWave = { 6, 10, 15, 20 };
    private float[] distanceIncrementsWaveToEnemyTower = { 6, 10, 18, 27, 35, 40 };
    private int[] playerHealthIncrements = { 200, 400 };

    private AI_BehavorTree behaviorTree;

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
        if (!redPlayer)
        {
            redPlayer = GameObject.FindGameObjectWithTag("RedPlayer");
            if (!redPlayer)
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

            int bluePlayerToRedTowerDistanceIndex;
            int playersDistanceIndex;
            int bluePlayerToRedWaveDistanceIndex;
            int blueWaveToRedTowerDistanceIndex;

            float bluePlayerToRedTowerDistance = Vector2.Distance(behaviorTree.bluePlayerPosition, redTowerPosition);
            float playersDistance = Vector2.Distance(behaviorTree.bluePlayerPosition, behaviorTree.redPlayerPosition);
            float bluePlayerToRedWaveDistance = Vector2.Distance(behaviorTree.bluePlayerPosition, behaviorTree.redMinionCentre);
            float blueWaveToRedTowerDistance = Vector2.Distance(behaviorTree.blueMinionCentre, redTowerPosition);

            {
                int index = 0;
                for (int i = 0; i < distanceIncrementsPlayerToEnemyTower.Length; ++i)
                {
                    index = i;
                    if (bluePlayerToRedTowerDistance < distanceIncrementsPlayerToEnemyTower[i])
                    {
                        break;
                    }
                    if (i == distanceIncrementsPlayerToEnemyTower.Length - 1)
                    {
                        ++index;
                    }
                }
                ++index;
                bluePlayerToRedTowerDistanceIndex = index;
            }

            if (!redPlayer.GetComponent<AgentStats>().isAlive)
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

            if (behaviorTree.redFrontMinionCluster.Count == 0)
            {
                bluePlayerToRedWaveDistanceIndex = 0;
            }
            else
            {
                int index = 0;
                for (int i = 0; i < distanceIncrementsPlayerToEnemyWave.Length; ++i)
                {
                    index = i;
                    if (bluePlayerToRedWaveDistance < distanceIncrementsPlayerToEnemyWave[i])
                    {
                        break;
                    }
                    if (i == distanceIncrementsPlayerToEnemyWave.Length - 1)
                    {
                        ++index;
                    }
                }
                ++index;
                bluePlayerToRedWaveDistanceIndex = index;
            }

            if (behaviorTree.blueFrontMinionCluster.Count == 0)
            {
                blueWaveToRedTowerDistanceIndex = 0;
            }
            else
            {
                int index = 0;
                for (int i = 0; i < distanceIncrementsWaveToEnemyTower.Length; ++i)
                {
                    index = i;
                    if (blueWaveToRedTowerDistance < distanceIncrementsWaveToEnemyTower[i])
                    {
                        break;
                    }
                    if (i == distanceIncrementsWaveToEnemyTower.Length - 1)
                    {
                        ++index;
                    }
                }
                ++index;
                blueWaveToRedTowerDistanceIndex = index;
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
                if (redPlayer.GetComponent<AgentStats>().isAlive)
                {
                    if (redPlayer.GetComponent<AgentStats>().health < playerHealthIncrements[i])
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
            stateID += bluePlayerToRedTowerDistanceIndex * 100000;
            stateID += playersDistanceIndex * 10000;
            stateID += bluePlayerToRedWaveDistanceIndex * 1000;
            stateID += blueWaveToRedTowerDistanceIndex * 100;
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

                if (behaviorTree.redFrontMinionCluster.Count == 0)
                {
                    if(!redPlayer.activeInHierarchy)
                    {
                        if(behaviorTree.blueFrontMinionCluster.Count == 0)
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
                        if (behaviorTree.blueFrontMinionCluster.Count == 0)
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
                    if (!redPlayer.activeInHierarchy)
                    {
                        if (behaviorTree.blueFrontMinionCluster.Count == 0)
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
                        if (behaviorTree.blueFrontMinionCluster.Count == 0)
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
                if (behaviorTree.redFrontMinionCluster.Count == 0)
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
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ImproovedTargetingScript : MonoBehaviour
{
    List<GameState> states = new List<GameState>();
    GameStatesList statesData = new GameStatesList();

    public string fileFolder;
    public string saveFolder;
    public float actionDuration;

    //public float towerDamageThreshold;
    //public float playerDamageThreshold;
    //public float backDistanceThreshold;
    //public float mouseProximityThreshold;

    Vector2 blueTowerPosition = new Vector2(-16.5f, -16.5f);
    Vector2 redTowerPosition = new Vector2(16.5f, 16.5f);
    //Vector2 bluePlayerSpawn;
    //Vector2 redPlayerSpawn;

    float maxMinionClusterDistance = 2;

    private float[] distanceIncrementsPlayerToEnemyTower = { 6, 10, 18, 27, 35, 40 };
    private float[] distanceIncrementsPlayerToPlayer = { 6, 10, 15, 20 };
    private float[] distanceIncrementsPlayerToEnemyWave = { 6, 10, 15, 20 };
    private float[] distanceIncrementsWaveToEnemyTower = { 6, 10, 18, 27, 35, 40 };
    private int[] playerHealthIncrements = { 200, 400 };

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        string[] files = Directory.GetFiles(fileFolder);

        SerializedGameDataList data = new SerializedGameDataList();

        foreach (string filePath in files)
        {
            string rawData = File.ReadAllText(filePath);

            bool isBlueLog;

            data = JsonUtility.FromJson<SerializedGameDataList>(rawData);

            string fileName = Path.GetFileName(filePath);
            if (fileName.Contains("Blue"))
            {
                isBlueLog = true;
            }
            else
            {
                isBlueLog = false;
            }

            for (int logItt = 0; logItt < data.logs.Length; ++logItt)
            {
                SerializedGameData log = data.logs[logItt];

                if (isBlueLog)
                {
                    if (!log.bluePlayerData.isAlive)
                    {
                        continue;
                    }
                }
                else
                {
                    if (!log.redPlayerData.isAlive)
                    {
                        continue;
                    }
                }

                Vector2 bluePlayerPosition = new Vector2(log.bluePlayerData.position.x, log.bluePlayerData.position.z);
                Vector2 redPlayerPosition = new Vector2(log.redPlayerData.position.x, log.redPlayerData.position.z);

                Vector2 blueMinionCentre = new Vector2(0, 0);
                Vector2 redMinionCentre = new Vector2(0, 0);

                float shortestBlueMinionDistanceToTower = Vector2.Distance(blueTowerPosition, redTowerPosition);
                float shortestRedMinionDistanceToTower = Vector2.Distance(blueTowerPosition, redTowerPosition);
                List<MinionSerializationData> blueFrontMinionCluster = new List<MinionSerializationData>();
                List<MinionSerializationData> redFrontMinionCluster = new List<MinionSerializationData>();

                //blue minions
                {
                    MinionSerializationData frontMinion;
                    //find first minion down the lane (closest to enemy tower)
                    foreach (MinionSerializationData minion in log.blueMinions)
                    {
                        Vector2 minionPos = new Vector2(minion.position.x, minion.position.z);

                        float distanceToTower = Vector2.Distance(redTowerPosition, minionPos);
                        if (distanceToTower < shortestBlueMinionDistanceToTower)
                        {
                            shortestBlueMinionDistanceToTower = distanceToTower;
                            frontMinion = minion;
                            blueMinionCentre = new Vector2(frontMinion.position.x, frontMinion.position.z);
                        }
                    }

                    foreach (MinionSerializationData minion in log.blueMinions)
                    {
                        Vector2 minionPosition = new Vector2(minion.position.x, minion.position.z);
                        float distanceFromCentre = Vector2.Distance(blueMinionCentre, minionPosition);
                        if (distanceFromCentre < maxMinionClusterDistance)
                        {
                            blueFrontMinionCluster.Add(minion);

                            float xCentre = 0;
                            float zCentre = 0;

                            foreach (MinionSerializationData minionInList in blueFrontMinionCluster)
                            {
                                xCentre += minionInList.position.x;
                                zCentre += minionInList.position.z;
                            }

                            xCentre = xCentre / blueFrontMinionCluster.Count;
                            zCentre = zCentre / blueFrontMinionCluster.Count;

                            blueMinionCentre.x = xCentre;
                            blueMinionCentre.y = zCentre;
                        }
                    }
                }

                //red minions
                {
                    MinionSerializationData frontMinion;
                    //find first minion down the lane (closest to enemy tower)
                    foreach (MinionSerializationData minion in log.redMinions)
                    {
                        Vector2 minionPos = new Vector2(minion.position.x, minion.position.z);

                        float distanceToTower = Vector2.Distance(redTowerPosition, minionPos);
                        if (distanceToTower < shortestRedMinionDistanceToTower)
                        {
                            shortestRedMinionDistanceToTower = distanceToTower;
                            frontMinion = minion;
                            redMinionCentre = new Vector2(frontMinion.position.x, frontMinion.position.z);
                        }
                    }

                    foreach (MinionSerializationData minion in log.redMinions)
                    {
                        Vector2 minionPosition = new Vector2(minion.position.x, minion.position.z);
                        float distanceFromCentre = Vector2.Distance(redMinionCentre, minionPosition);
                        if (distanceFromCentre < maxMinionClusterDistance)
                        {
                            redFrontMinionCluster.Add(minion);

                            float xCentre = 0;
                            float zCentre = 0;

                            foreach (MinionSerializationData minionInList in redFrontMinionCluster)
                            {
                                xCentre += minionInList.position.x;
                                zCentre += minionInList.position.z;
                            }

                            xCentre = xCentre / redFrontMinionCluster.Count;
                            zCentre = zCentre / redFrontMinionCluster.Count;

                            redMinionCentre.x = xCentre;
                            redMinionCentre.y = zCentre;
                        }
                    }
                }

                if (isBlueLog)
                {
                    int bluePlayerToRedTowerDistanceIndex;
                    int playersDistanceIndex;
                    int bluePlayerToRedWaveDistanceIndex;
                    int blueWaveToRedTowerDistanceIndex;

                    float bluePlayerToRedTowerDistance = Vector2.Distance(bluePlayerPosition, redTowerPosition);
                    float playersDistance = Vector2.Distance(bluePlayerPosition, redPlayerPosition);
                    float bluePlayerToRedWaveDistance = Vector2.Distance(bluePlayerPosition, redMinionCentre);
                    float blueWaveToRedTowerDistance = Vector2.Distance(blueMinionCentre, redTowerPosition);

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

                    if (!log.redPlayerData.isAlive)
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

                    if (log.redMinionsAlive == 0)
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

                    if (log.blueMinionsAlive == 0)
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
                        if (log.bluePlayerData.isAlive)
                        {
                            if (log.bluePlayerData.health < playerHealthIncrements[i])
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
                        if (log.redPlayerData.isAlive)
                        {
                            if (log.redPlayerData.health < playerHealthIncrements[i])
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

                    {
                        int currentSecond = log.secondsElapsed;
                        int currentMilisecond = log.milisecondsElapsed;
                        int lastSecond = log.secondsElapsed + (int)(actionDuration / 1.0f);
                        int lastMilisecond = log.milisecondsElapsed + (int)((actionDuration / 0.1f) * 100);

                        int lastLogIndex = logItt + 1;
                        bool foundLastLog = false;
                        int smallestDifference = (int)(actionDuration * 2);

                        if (lastLogIndex >= data.logs.Length)
                        {
                            lastLogIndex = data.logs.Length - 1;
                            foundLastLog = true;
                            break;
                        }

                        while (!foundLastLog)
                        {
                            SerializedGameData nextLog = data.logs[lastLogIndex];
                            int secondsDiff = nextLog.secondsElapsed - lastSecond;
                            int milisecondsDiff = nextLog.milisecondsElapsed - lastMilisecond;

                            int diff = secondsDiff * 1000 + milisecondsDiff;

                            if (diff < smallestDifference)
                            {
                                smallestDifference = diff;

                                if (lastLogIndex + 1 >= data.logs.Length)
                                {
                                    foundLastLog = true;
                                    break;
                                }

                                ++lastLogIndex;
                            }
                            else
                            {
                                foundLastLog = true;
                                break;
                            }
                        }

                        SerializedGameData lastLog = data.logs[lastLogIndex];

                        //calculating difference health for ENEMY player, minions and tower

                        //float actionDistribution = 1;

                        //bool backAction = false;
                        bool towerAction = false;
                        bool playerAction = false;
                        bool clearAction = false;

                        int totalLogsAnalysed = lastLogIndex - logItt;

                        int playerTargetingCounter = 0;
                        int waveTargetingCounter = 0;

                        for (int stateItt = logItt; stateItt < lastLogIndex; ++stateItt)
                        {
                            if (data.logs[stateItt].bluePlayerData.playerInput.target == 1)
                            {
                                ++playerTargetingCounter;
                            }
                            else if (data.logs[stateItt].bluePlayerData.playerInput.ability1 || data.logs[stateItt].bluePlayerData.playerInput.ability3)
                            {
                                Vector2 mousePos2D = new Vector2(data.logs[stateItt].bluePlayerData.playerInput.mousePosInGame.x, data.logs[stateItt].bluePlayerData.playerInput.mousePosInGame.z);

                                float playerDis = Vector2.Distance(mousePos2D, redPlayerPosition);
                                float waveDis = Vector2.Distance(mousePos2D, redMinionCentre);

                                if(playerDis < waveDis)
                                {
                                    playerAction = true;
                                    break;
                                }
                            }
                        }

                        for (int stateItt = logItt; stateItt < lastLogIndex; ++stateItt)
                        {
                            if (data.logs[stateItt].bluePlayerData.playerInput.target == 2)
                            {
                                towerAction = true;
                                break;
                            }
                        }

                        for (int stateItt = logItt; stateItt < lastLogIndex; ++stateItt)
                        {
                            if (data.logs[stateItt].bluePlayerData.playerInput.target == 3)
                            {
                                ++waveTargetingCounter;
                            }
                            else if (data.logs[stateItt].bluePlayerData.playerInput.ability1 || data.logs[stateItt].bluePlayerData.playerInput.ability3)
                            {
                                Vector2 mousePos2D = new Vector2(data.logs[stateItt].bluePlayerData.playerInput.mousePosInGame.x, data.logs[stateItt].bluePlayerData.playerInput.mousePosInGame.z);

                                float playerDis = Vector2.Distance(mousePos2D, redPlayerPosition);
                                float waveDis = Vector2.Distance(mousePos2D, redMinionCentre);

                                if (playerDis > waveDis)
                                {
                                    clearAction = true;
                                    break;
                                }
                            }
                        }

                        if(playerTargetingCounter>waveTargetingCounter)
                        {
                            playerAction = true;
                        }
                        else
                        {
                            clearAction = true;
                        }

                        //float closestDistanceToBlueTower = Vector2.Distance(blueTowerPosition, bluePlayerPosition);

                        //for (int stateItt = logItt; stateItt < lastLogIndex; ++stateItt)
                        //{
                        //    Vector2 pos = new Vector2(data.logs[stateItt].bluePlayerData.position.x, data.logs[stateItt].bluePlayerData.position.z);
                        //    float dist = Vector2.Distance(blueTowerPosition, pos);

                        //    if (dist < closestDistanceToBlueTower)
                        //    {
                        //        closestDistanceToBlueTower = dist;
                        //    }
                        //}

                        //float backedOff = Vector2.Distance(bluePlayerPosition, blueTowerPosition) - closestDistanceToBlueTower;

                        //if (backedOff > backDistanceThreshold)
                        //{
                        //    backAction = true;
                        //}

                        if (towerAction)
                        {
                            //if(playerAction && clearAction)
                            //{
                            //    towerActionVal = 0.75f;
                            //    playerActionVal = 0.1f;
                            //    clearActionVal = 0.15f;
                            //}
                            //else if(playerAction)
                            //{
                            //    towerActionVal = 0.85f;
                            //    playerActionVal = 0.15f;
                            //}
                            //else if(clearAction)
                            //{
                            //    towerActionVal = 0.75f;
                            //    clearActionVal = 0.25f;
                            //}
                            //else
                            //{
                                towerActionVal = 1f;
                            //}
                        }
                        else
                        {
                            if (playerAction && clearAction)
                            {
                                playerActionVal = 0.5f;
                                clearActionVal = 0.5f;
                            }
                            else if (playerAction)
                            {
                                playerActionVal = 1f;
                            }
                            else
                            {
                                clearActionVal = 1f;
                            }
                        }

                        //// rework this

                        //if (backAction)
                        //{
                        //    if (towerAction)
                        //    {
                        //        backActionVal = 0.35f;
                        //        towerActionVal = 0.5f;
                        //        actionDistribution = 0.25f;
                        //    }
                        //    else if (playerAction)
                        //    {
                        //        playerActionVal = 0.5f;
                        //        backActionVal = 0.5f;
                        //        actionDistribution = 0;
                        //    }
                        //    else
                        //    {
                        //        backActionVal = 0.5f;
                        //        actionDistribution = 0.5f;
                        //    }
                        //}
                        //else if (towerAction)
                        //{
                        //    towerActionVal = 0.8f;
                        //    actionDistribution = 0.2f;
                        //}
                        //else if (playerAction)
                        //{
                        //    playerActionVal = 0.5f;
                        //    actionDistribution = 0.5f;
                        //}

                        //clearActionVal = actionDistribution;


                        //float minionCurrentHealth = 0;
                        //foreach (MinionSerializationData min in log.redMinions)
                        //{
                        //    minionCurrentHealth += min.health;
                        //}

                        //float minionLastHealth = 0;
                        //foreach (MinionSerializationData min in lastLog.redMinions)
                        //{
                        //    minionLastHealth += min.health;
                        //}

                        //float minionHealthDifference = minionCurrentHealth - minionLastHealth;

                        //float playerPercentageHealthLost = (float)log.redPlayerData.health / playerHealthDifference;
                        //float minionsPercentageHealthLost = minionCurrentHealth / minionHealthDifference;
                        ////float towerPercentageHealthLost = (float)log.redTowerData.health / towerHealthDifference;

                        //bool targettingTower = false;
                        //bool targetingPlayer = false;
                        //bool targetingwave = false;

                        //if(playerPercentageHealthLost > minionsPercentageHealthLost / 2 )
                        //{

                        //}

                        //_________________________________________________________________________________________________________

                        //for (int stateItt = logItt; stateItt < lastLogIndex; ++stateItt)
                        //{
                        //    SerializedGameData nLog = data.logs[stateItt];

                        //    if(nLog.logType == "log")
                        //    {
                        //        continue;
                        //    }

                        //    float mouseToPlayer = Vector2.Distance(nLog.mo);
                        //    float mouseToWave = 0;
                        //    float mouseToTower = 0;
                        //    float mouseToAllyTower = 0;


                        //}

                    }

                    bool foundState = false;
                    foreach (GameState s in states)
                    {
                        if (s.stateID == stateID)
                        {
                            ++s.frequency;

                            //more stuff abt the action
                            s.trade += playerActionVal;
                            s.wave += clearActionVal;
                            s.tower += towerActionVal;
                            s.back += backActionVal;

                            foundState = true;
                            break;
                        }
                    }
                    if (!foundState)
                    {
                        GameState currentState = new GameState();
                        currentState.stateID = stateID;

                        currentState.trade += playerActionVal;
                        currentState.wave += clearActionVal;
                        currentState.tower += towerActionVal;
                        currentState.back += backActionVal;

                        states.Add(currentState);

                    }
                }
                else
                {

                }
            }
        }

        statesData.states = states.ToArray();

        string saveFile = "/combinedStatesData.json";
        string savePath = saveFolder + saveFile;

        try
        {
            string json = JsonUtility.ToJson(statesData, true);

            File.WriteAllText(savePath, json);
            Debug.Log("States saved to" + savePath);
        }
        catch (Exception ex)
        {
            Debug.LogError("Error saving states");
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}


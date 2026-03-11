using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

[Serializable]
class State
{
    public int stateID;
    public int frequency = 1;
    public float trade = 0;
    public float wave = 0;
    public float tower = 0;
    public float back = 0;
}

[Serializable]
class StatesList
{
    public State[] states;
}

public class SimplePositionBasedStatesParsing : MonoBehaviour
{
    List<State> states = new List<State>();
    StatesList statesData = new StatesList();

    public string fileFolder;
    public string saveFolder;

    Vector2 blueTowerPosition = new Vector2(-16.5f, -16.5f);
    Vector2 redTowerPosition = new Vector2(16.5f, 16.5f);
    //Vector2 bluePlayerSpawn;
    //Vector2 redPlayerSpawn;

    float maxMinionClusterDistance = 2;

    private float[] distanceIncrementsPlayerToPlayer = { 6, 10, 15, 20 };
    private float[] distanceIncrementsPlayerToEnemyWave = { 6, 10, 15,20 };
    private float[] distanceIncrementsPlayerToEnemyTower = { 6, 10, 18, 27, 35, 40 };
    private float[] distanceIncrementsWaveToEnemyTower = { 6, 10, 18, 27, 35, 40 };

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

            foreach (SerializedGameData log in data.logs)
            {
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
                    float playersDistance = Vector2.Distance(bluePlayerPosition,redPlayerPosition);
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
                            if(i == distanceIncrementsPlayerToPlayer.Length -1)
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

                    int stateID = 0;
                    stateID += bluePlayerToRedTowerDistanceIndex * 1000;
                    stateID += playersDistanceIndex * 100;
                    stateID += bluePlayerToRedWaveDistanceIndex * 10;
                    stateID += blueWaveToRedTowerDistanceIndex;

                    bool foundState = false;
                    foreach(State s in states)
                    {
                        if(s.stateID == stateID)
                        {
                            ++s.frequency;

                            //more stuff abt the action

                            foundState = true; 
                            break;
                        }
                    }
                    if(!foundState)
                    {
                        State currentState = new State();
                        currentState.stateID = stateID;
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

using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class simplePositionBasedStatesParsing : MonoBehaviour
{
    public string fileFolder;
    public string saveFolder;

    Vector2 blueTowerPosition;
    Vector2 redTowerPosition;

    float maxMinionClusterDistance;

    public SimpleDistanceParameters distanceParameters;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        string[] files = Directory.GetFiles(fileFolder);

        Vector2 frontWavePosition;

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
                Vector2 blueWaveFrontPosition;
                Vector2 bluePlayerPosition;
                Vector2 redWaveFrontPosition;
                Vector2 redPlayerPosition;
                {
                    float shortestMinionDistanceToTower = Vector2.Distance(blueTowerPosition, redTowerPosition);
                    List<MinionSerializationData> fronMinionCluster = new List<MinionSerializationData>();

                    MinionSerializationData frontMinion;
                    //find first minion down the lane (closest to enemy tower)
                    foreach (MinionSerializationData minion in log.blueMinions)
                    {
                        Vector2 minionPos = new Vector2(minion.position.x, minion.position.z);
                        
                        float distanceToTower = Vector2.Distance(redTowerPosition, minionPos);
                        if(distanceToTower < shortestMinionDistanceToTower)
                        {
                            shortestMinionDistanceToTower = distanceToTower;
                            frontMinion = minion;
                        }
                    }

                    foreach (MinionSerializationData minion in log.blueMinions)
                    {
                    }
                    //find minions adjiacent to the first minion

                    }
                if (isBlueLog)
                {

                }
                else
                {

                }
            }
        }
    
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

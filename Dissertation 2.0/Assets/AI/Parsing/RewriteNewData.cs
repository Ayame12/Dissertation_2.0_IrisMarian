using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

//this script reqrites the initial json file to be able to be read in an array

public class RewriteNewData : MonoBehaviour
{
    public string fileFolder;
    public string saveFolder;

    void Start()
    {
        string[] files = Directory.GetFiles(fileFolder);

        foreach (string filePath in files)
        {
            List<deathTimeStamp> blueDeathList = new List<deathTimeStamp>();
            List<deathTimeStamp> redDeathList = new List<deathTimeStamp>();

            List<SerializedGameData> oldLogs = new List<SerializedGameData>();
            List<SerializedGameData> correctLogs = new List<SerializedGameData>();

            SerializedGameDataList newData = new SerializedGameDataList();

            try
            {
                string raw = File.ReadAllText(filePath).Trim();

                string[] jsonObjects = Regex.Split(raw, @"}\s*{");

                for (int i = 0; i < jsonObjects.Length; i++)
                {
                    string obj = jsonObjects[i];

                    if (i != 0) obj = "{" + obj;
                    if (i != jsonObjects.Length - 1) obj = obj + "}";

                    SerializedGameData log = JsonUtility.FromJson<SerializedGameData>(obj);
                    oldLogs.Add(log);
                }

                Debug.Log("Loaded " + jsonObjects.Length + " log entries successfully!");

            }
            catch (Exception e)
            {
                Debug.LogError("JSON PARSE ERROR:\n" + e);
            }

            foreach (SerializedGameData log in oldLogs)
            {
                SerializedGameData newLog = new SerializedGameData();

                newLog.logType = log.logType;
                newLog.minutesElapsed = log.minutesElapsed;
                newLog.secondsElapsed = log.secondsElapsed;
                newLog.milisecondsElapsed = log.milisecondsElapsed;

                newLog.blueMinionsAlive = log.blueMinionsAlive;
                newLog.redMinionsAlive = log.redMinionsAlive;


                //blue player 
                newLog.bluePlayerData.objectType = log.bluePlayerData.objectType;
                newLog.bluePlayerData.isBlue = log.bluePlayerData.isBlue;
                newLog.bluePlayerData.isAlive = true;
                newLog.bluePlayerData.health = log.bluePlayerData.health;
                newLog.bluePlayerData.position = log.bluePlayerData.position;
                newLog.bluePlayerData.ability1 = log.bluePlayerData.ability1;
                newLog.bluePlayerData.ability2 = log.bluePlayerData.ability2;
                newLog.bluePlayerData.ability3 = log.bluePlayerData.ability3;
                newLog.bluePlayerData.ability1CD = log.bluePlayerData.ability1CD;
                newLog.bluePlayerData.ability2CD = log.bluePlayerData.ability2CD;
                newLog.bluePlayerData.ability3CD = log.bluePlayerData.ability3CD;
                newLog.bluePlayerData.creepScore = log.bluePlayerData.creepScore;
                newLog.bluePlayerData.isStunned = log.bluePlayerData.isStunned;
                newLog.bluePlayerData.isSlowed = log.bluePlayerData.isSlowed;
                newLog.bluePlayerData.stunRemaining = log.bluePlayerData.stunRemaining;
                newLog.bluePlayerData.playerInput = log.bluePlayerData.playerInput;

                //red player 
                newLog.redPlayerData.objectType = log.redPlayerData.objectType;
                newLog.redPlayerData.isBlue = log.redPlayerData.isBlue;
                newLog.redPlayerData.isAlive = true;
                newLog.redPlayerData.health = log.redPlayerData.health;
                newLog.redPlayerData.position = log.redPlayerData.position;
                newLog.redPlayerData.ability1 = log.redPlayerData.ability1;
                newLog.redPlayerData.ability2 = log.redPlayerData.ability2;
                newLog.redPlayerData.ability3 = log.redPlayerData.ability3;
                newLog.redPlayerData.ability1CD = log.redPlayerData.ability1CD;
                newLog.redPlayerData.ability2CD = log.redPlayerData.ability2CD;
                newLog.redPlayerData.ability3CD = log.redPlayerData.ability3CD;
                newLog.redPlayerData.creepScore = log.redPlayerData.creepScore;
                newLog.redPlayerData.isStunned = log.redPlayerData.isStunned;
                newLog.redPlayerData.isSlowed = log.redPlayerData.isSlowed;
                newLog.redPlayerData.stunRemaining = log.redPlayerData.stunRemaining;
                newLog.redPlayerData.playerInput = log.redPlayerData.playerInput;

                newLog.blueTowerData = log.blueTowerData;
                newLog.redTowerData = log.redTowerData;

                newLog.blueMinions = log.blueMinions;
                newLog.redMinions = log.redMinions;

                correctLogs.Add(newLog);
            }

            Debug.Log("corrected " + correctLogs.Count + " log entries successfully!");

            newData.logs = correctLogs.ToArray();

            string saveName = filePath.Substring(fileFolder.Length);
            string savePath = saveFolder + saveName;

            try
            {
                string json = JsonUtility.ToJson(newData, true);

                File.WriteAllText(savePath, json);
                Debug.Log("Players saved to" + savePath);
            }
            catch (Exception ex)
            {
                Debug.LogError("Error saving new player data");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}

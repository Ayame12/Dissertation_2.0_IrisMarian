using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public class deathTimeStamp
{
    public int deathMin = 0;
    public int deathSec = 0;
    public int lifeMin = 0;
    public int lifeSec = 0;

    public int milisecond;
}

public class AmmendPlayerDeath : MonoBehaviour
{
    public string fileFolder;
    public string saveFolder;

    //private string savePath;
    //private string filePath;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        string[] files = Directory.GetFiles(fileFolder);

        foreach (string filePath in files)
        {
            bool blueLookFor650 = false;
            bool redLookFor650 = false;
            int blueDeathTimer = 15;
            int redDeathTimer = 15;

            List<deathTimeStamp> blueDeathList = new List<deathTimeStamp>();
            List<deathTimeStamp> redDeathList = new List<deathTimeStamp>();

            List<SerializedGameDataOld> oldLogs = new List<SerializedGameDataOld>();
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

                    SerializedGameDataOld log = JsonUtility.FromJson<SerializedGameDataOld>(obj);
                    oldLogs.Add(log);
                }

                Debug.Log("Loaded " + jsonObjects.Length + " log entries successfully!");

            }
            catch (Exception e)
            {
                Debug.LogError("JSON PARSE ERROR:\n" + e);
            }

            foreach (SerializedGameDataOld log in oldLogs)
            {
                /*
                    public string logType = "log";
                    public int minutesElapsed = 0;
                    public int secondsElapsed = 0;
                    public int milisecondsElapsed = 0;

                    public int blueMinionsAlive = 0;
                    public int redMinionsAlive = 0;

                    public PlayerSerializedData bluePlayerData;
                    public PlayerSerializedData redPlayerData;
                    public TowerSerializationData blueTowerData;
                    public TowerSerializationData redTowerData;

                    public List<MinionSerializationData> blueMinions;
                    public List<MinionSerializationData> redMinions;
                 */

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

                if (blueLookFor650)
                {
                    if (newLog.bluePlayerData.health == 650)
                    {
                        blueLookFor650 = false;

                        bool validDeath = true;

                        deathTimeStamp death = new deathTimeStamp();
                        death.lifeMin = newLog.minutesElapsed;
                        death.lifeSec = newLog.secondsElapsed;
                        death.deathMin = death.lifeMin;
                        death.deathSec = death.lifeSec - blueDeathTimer;
                        death.milisecond = newLog.milisecondsElapsed;
                        if (death.deathSec < 0)
                        {
                            death.deathSec = 60 + death.deathSec;
                            death.deathMin -= 1;

                            if (death.deathMin < 0)
                            {
                                Debug.LogError("Invalid Death in file " + filePath);
                                validDeath = false;
                            }
                        }
                        if (validDeath)
                        {
                            ++blueDeathTimer;
                            blueDeathList.Add(death);
                        }
                    }
                }
                else
                {
                    if (newLog.bluePlayerData.health < 400)
                    {
                        blueLookFor650 = true;
                    }
                }

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

                if (redLookFor650)
                {
                    if (newLog.redPlayerData.health == 650)
                    {
                        redLookFor650 = false;

                        bool validDeath = true;

                        deathTimeStamp death = new deathTimeStamp();
                        death.lifeMin = newLog.minutesElapsed;
                        death.lifeSec = newLog.secondsElapsed;
                        death.deathMin = death.lifeMin;
                        death.deathSec = death.lifeSec - redDeathTimer;
                        if (death.deathSec < 0)
                        {
                            death.deathSec = 60 + death.lifeSec;
                            death.deathMin -= 1;

                            if (death.deathMin < 0)
                            {
                                Debug.LogError("Invalid Death");
                                validDeath = false;
                            }
                        }
                        if (validDeath)
                        {
                            ++redDeathTimer;
                            redDeathList.Add(death);
                        }
                    }
                }
                else
                {
                    if (newLog.redPlayerData.health < 650)
                    {
                        redLookFor650 = true;
                    }
                }

                //---------------

                newLog.blueTowerData = log.blueTowerData;
                newLog.redTowerData = log.redTowerData;

                newLog.blueMinions = log.blueMinions;
                newLog.redMinions = log.redMinions;

                correctLogs.Add(newLog);
            }

            int blueDeathInc = 0;
            int redDeathInc = 0;

            bool blueIsDead = false;
            bool redIsDead = false;

            foreach (SerializedGameData log in correctLogs)
            {
                if (blueDeathInc < blueDeathList.Count)
                {
                    if (!blueIsDead)
                    {
                        if (log.minutesElapsed > blueDeathList[blueDeathInc].deathMin)
                        {
                            blueIsDead = true;
                            log.bluePlayerData.isAlive = false;
                        }
                        else if (log.minutesElapsed == blueDeathList[blueDeathInc].deathMin)
                        {
                            if (log.secondsElapsed > blueDeathList[blueDeathInc].deathSec)
                            {
                                blueIsDead = true;
                                log.bluePlayerData.isAlive = false;
                            }
                            else if (log.secondsElapsed == blueDeathList[blueDeathInc].deathSec)
                            {
                                if (log.milisecondsElapsed > blueDeathList[blueDeathInc].milisecond)
                                {
                                    blueIsDead = true;
                                    log.bluePlayerData.isAlive = false;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (log.minutesElapsed > blueDeathList[blueDeathInc].lifeMin)
                        {
                            blueIsDead = false;
                            log.bluePlayerData.isAlive = true;
                            ++blueDeathInc;
                        }
                        else if (log.minutesElapsed == blueDeathList[blueDeathInc].lifeMin)
                        {
                            if (log.secondsElapsed > blueDeathList[blueDeathInc].lifeSec)
                            {
                                blueIsDead = false;
                                log.bluePlayerData.isAlive = true;
                                ++blueDeathInc;
                            }
                            else if (log.secondsElapsed == blueDeathList[blueDeathInc].lifeSec)
                            {
                                if (log.milisecondsElapsed > blueDeathList[blueDeathInc].milisecond)
                                {
                                    blueIsDead = false;
                                    log.bluePlayerData.isAlive = true;
                                    ++blueDeathInc;
                                }
                            }
                        }
                        log.bluePlayerData.isAlive = false;
                    }
                }

                if (redDeathInc < redDeathList.Count)
                {
                    if (!redIsDead)
                    {
                        if (log.minutesElapsed > redDeathList[redDeathInc].deathMin)
                        {
                            redIsDead = true;
                            log.redPlayerData.isAlive = false;
                        }
                        else if (log.minutesElapsed == redDeathList[redDeathInc].deathMin)
                        {
                            if (log.secondsElapsed > redDeathList[redDeathInc].deathSec)
                            {
                                redIsDead = true;
                                log.redPlayerData.isAlive = false;
                            }
                            else if (log.secondsElapsed == redDeathList[redDeathInc].deathSec)
                            {
                                if (log.milisecondsElapsed > redDeathList[redDeathInc].milisecond)
                                {
                                    redIsDead = true;
                                    log.redPlayerData.isAlive = false;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (log.minutesElapsed > redDeathList[redDeathInc].lifeMin)
                        {
                            redIsDead = false;
                            log.redPlayerData.isAlive = true;
                            ++redDeathInc;
                        }
                        else if (log.minutesElapsed == redDeathList[redDeathInc].lifeMin)
                        {
                            if (log.secondsElapsed > redDeathList[redDeathInc].lifeSec)
                            {
                                redIsDead = false;
                                log.redPlayerData.isAlive = true;
                                ++redDeathInc;
                            }
                            else if (log.secondsElapsed == redDeathList[redDeathInc].lifeSec)
                            {
                                if (log.milisecondsElapsed > redDeathList[redDeathInc].milisecond)
                                {
                                    redIsDead = false;
                                    log.redPlayerData.isAlive = true;
                                    ++redDeathInc;
                                }
                            }
                        }
                        log.redPlayerData.isAlive = false;
                    }
                }
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
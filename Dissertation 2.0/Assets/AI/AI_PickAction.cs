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
    //Vector2 bluePlayerSpawn;
    //Vector2 redPlayerSpawn;

    public float maxMinionClusterDistance = 2;

    private float[] distanceIncrementsPlayerToEnemyTower = { 6, 10, 18, 27, 35, 40 };
    private float[] distanceIncrementsPlayerToPlayer = { 6, 10, 15, 20 };
    private float[] distanceIncrementsPlayerToEnemyWave = { 6, 10, 15, 20 };
    private float[] distanceIncrementsWaveToEnemyTower = { 6, 10, 18, 27, 35, 40 };

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //string rawData = File.ReadAllText(statesDataFile);

        //states = JsonUtility.FromJson<StatesList>(rawData);
    }

    // Update is called once per frame
    public void tickUpdate()
    {
        if(checkState)
        {
            gameObject.GetComponent<AI_BehavorTree>().currentAction = ActionType.clear;
        }


    }
}

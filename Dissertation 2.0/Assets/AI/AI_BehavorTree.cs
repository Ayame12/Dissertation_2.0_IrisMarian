using System.Collections.Generic;
using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.InputSystem;

public enum ActionType
{
    trade,
    clear,
    tower,
    back,
}

public class AI_BehavorTree : MonoBehaviour
{
    public ActionType currentAction;

    public float playerAbilityCastRangeWave = 8;
    public float playerAbilityCastRangePlayer = 10;

    private float maxMinionClusterDistance = 2;

    private Vector2 blueTowerPosition = new Vector2(-16.5f, -16.5f);
    private Vector2 redTowerPosition = new Vector2(16.5f, 16.5f);

    public Vector2 bluePlayerSpawn = new Vector2(-19.2f, -19.2f);
    public Vector2 redPlayerSpawn = new Vector2(19.2f, 19.2f);

    private Vector2 blueMinionCentre = new Vector2(0, 0);
    private Vector2 redMinionCentre = new Vector2(0, 0);

    private Vector2 bluePlayerPosition = new Vector2(0, 0);
    private Vector2 redPlayerPosition = new Vector2(0, 0);

    private Vector3 bluePlayerPosition3D = new Vector3(0, 0);
    private Vector3 redPlayerPosition3D = new Vector3(0, 0);

    private GameObject redTower;
    private GameObject redPlayer;
    private List<GameObject> blueFrontMinionCluster = new List<GameObject>();
    private List<GameObject> redFrontMinionCluster = new List<GameObject>();

    private PlayerInputScript playerInput;
    private PlayerAttackScript playerAttack;

    ActionType lastAction = ActionType.clear;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        maxMinionClusterDistance = gameObject.GetComponent<AI_PickAction>().maxMinionClusterDistance;

        blueTowerPosition = gameObject.GetComponent<AI_PickAction>().blueTowerPosition;
        redTowerPosition = gameObject.GetComponent<AI_PickAction>().redTowerPosition;

        playerInput = gameObject.GetComponent<PlayerInputScript>();
        redTower = GameObject.FindGameObjectWithTag("RedTower");
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

        bluePlayerPosition.x = transform.position.x;
        bluePlayerPosition.y = transform.position.z;

        redPlayerPosition.x = redPlayer.transform.position.x;
        redPlayerPosition.y = redPlayer.transform.position.z;

        bluePlayerPosition3D = transform.position;
        redPlayerPosition3D = redPlayer.transform.position;

        GameObject[] blueMinions = GameObject.FindGameObjectsWithTag("BlueMinion");
        GameObject[] redMinions = GameObject.FindGameObjectsWithTag("RedMinion");

        blueFrontMinionCluster.Clear();
        redFrontMinionCluster.Clear();

        {
            GameObject blueFrontMinion;
            GameObject redFrontMinion;

            float shortestBlueMinionDistanceToTower = Vector2.Distance(blueTowerPosition, redTowerPosition);
            float shortestRedMinionDistanceToTower = Vector2.Distance(blueTowerPosition, redTowerPosition);

            //blue minion wave
            foreach (GameObject minion in blueMinions)
            {
                Vector2 minionPos = new Vector2(minion.transform.position.x, minion.transform.position.z);

                float distanceToTower = Vector2.Distance(redTowerPosition, minionPos);
                if (distanceToTower < shortestBlueMinionDistanceToTower)
                {
                    shortestBlueMinionDistanceToTower = distanceToTower;
                    blueFrontMinion = minion;
                    blueMinionCentre = new Vector2(blueFrontMinion.transform.position.x, blueFrontMinion.transform.position.z);
                }
            }

            foreach (GameObject minion in blueMinions)
            {
                Vector2 minionPosition = new Vector2(minion.transform.position.x, minion.transform.position.z);
                float distanceFromCentre = Vector2.Distance(blueMinionCentre, minionPosition);
                if (distanceFromCentre < maxMinionClusterDistance)
                {
                    blueFrontMinionCluster.Add(minion);

                    float xCentre = 0;
                    float zCentre = 0;

                    foreach (GameObject minionInList in blueFrontMinionCluster)
                    {
                        xCentre += minionInList.transform.position.x;
                        zCentre += minionInList.transform.position.z;
                    }

                    xCentre = xCentre / blueFrontMinionCluster.Count;
                    zCentre = zCentre / blueFrontMinionCluster.Count;

                    blueMinionCentre.x = xCentre;
                    blueMinionCentre.y = zCentre;
                }
            }

            //red minion wave
            foreach (GameObject minion in redMinions)
            {
                Vector2 minionPos = new Vector2(minion.transform.position.x, minion.transform.position.z);

                float distanceToTower = Vector2.Distance(blueTowerPosition, minionPos);
                if (distanceToTower < shortestRedMinionDistanceToTower)
                {
                    shortestRedMinionDistanceToTower = distanceToTower;
                    redFrontMinion = minion;
                    redMinionCentre = new Vector2(redFrontMinion.transform.position.x, redFrontMinion.transform.position.z);
                }
            }

            foreach (GameObject minion in redMinions)
            {
                Vector2 minionPosition = new Vector2(minion.transform.position.x, minion.transform.position.z);
                float distanceFromCentre = Vector2.Distance(redMinionCentre, minionPosition);
                if (distanceFromCentre < maxMinionClusterDistance)
                {
                    redFrontMinionCluster.Add(minion);

                    float xCentre = 0;
                    float zCentre = 0;

                    foreach (GameObject minionInList in redFrontMinionCluster)
                    {
                        xCentre += minionInList.transform.position.x;
                        zCentre += minionInList.transform.position.z;
                    }

                    xCentre = xCentre / redFrontMinionCluster.Count;
                    zCentre = zCentre / redFrontMinionCluster.Count;

                    redMinionCentre.x = xCentre;
                    redMinionCentre.y = zCentre;
                }
            }
        }
        currentAction = ActionType.trade;
        switch (currentAction)
        {
            case ActionType.trade:
                tradeAction();
                break;
            case ActionType.clear:
                clearAction();
                break;
            case ActionType.tower:
                towerAction();
                break;
            case ActionType.back:
                backAction();
                break;
            default:
                break;
        }
        lastAction = currentAction;
    }

    void tradeAction()
    {
        if (!redPlayer.activeInHierarchy)
        {
            return;
        }

        if (Vector2.Distance(bluePlayerPosition, redPlayerPosition) > playerAbilityCastRangePlayer)
        {
            Vector2 direction = (redPlayerPosition - bluePlayerPosition).normalized;
            float rotY = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
            Vector2 targetPos = redPlayerPosition + direction * playerAbilityCastRangePlayer;

            Vector3 targetPos3D = new Vector3(targetPos.x, -1, targetPos.y);

            playerInput.move = true;
            playerInput.mousePosInGame = targetPos3D;
        }
        else
        {
            if (gameObject.GetComponent<PlayerAttackScript>().rootIsAvailable)
            {
                playerInput.ability1 = true;
                playerInput.mousePosInGame = redPlayerPosition3D;
            }
            else if (gameObject.GetComponent<PlayerAttackScript>().ultIsAvailable && redPlayer.GetComponent<AgentStats>().isStunned)
            {
                playerInput.ability3 = true;
                playerInput.mousePosInGame = redPlayerPosition3D;
            }
            else
            {
                playerInput.attack = true;
                playerInput.target = redPlayer;
            }
        }
    }

    void clearAction()
    {
        if(redFrontMinionCluster.Count == 0)
        {
            return;
        }

        playerInput.move = false;
        playerInput.attack = false;
        playerInput.ability1 = false;
        playerInput.ability2 = false;
        playerInput.ability3 = false;

        if (Vector2.Distance(bluePlayerPosition, redMinionCentre) > playerAbilityCastRangeWave)
        {
            Vector2 direction = (redMinionCentre - bluePlayerPosition).normalized;
            float rotY = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
            Vector2 targetPos = redMinionCentre + direction * playerAbilityCastRangeWave;

            Vector3 targetPos3D = new Vector3(targetPos.x, -1, targetPos.y);

            playerInput.move = true;
            playerInput.mousePosInGame = targetPos3D;
        }
        else
        {
            if (gameObject.GetComponent<PlayerAttackScript>().rootIsAvailable && redFrontMinionCluster.Count > 1)
            {
                playerInput.ability1 = true;
                playerInput.mousePosInGame = new Vector3(redMinionCentre.x, -1, redMinionCentre.y);
            }
            else if (gameObject.GetComponent<PlayerAttackScript>().ultIsAvailable && redFrontMinionCluster.Count > 4)
            {
                playerInput.ability3 = true;
                playerInput.mousePosInGame = new Vector3(redMinionCentre.x, -1, redMinionCentre.y); ;
            }
            else
            {
                GameObject closestMinion = redFrontMinionCluster[0];
                Vector2 nextMinionPos = new Vector2(closestMinion.transform.position.x, closestMinion.transform.position.z);
                float shortestDistanceToMinion = Vector2.Distance(bluePlayerPosition, nextMinionPos);

                foreach(GameObject minion in redFrontMinionCluster)
                {
                    nextMinionPos.x = minion.transform.position.x;
                    nextMinionPos.y = minion.transform.position.z;

                    if(Vector2.Distance(nextMinionPos, bluePlayerPosition) < shortestDistanceToMinion)
                    {
                        closestMinion = minion;
                        shortestDistanceToMinion = Vector2.Distance(nextMinionPos, bluePlayerPosition);
                    }
                }

                playerInput.attack = true;
                playerInput.target = closestMinion;
            }
        }
    }

    void towerAction()
    {
        playerInput.attack = true;
        playerInput.target = redTower;
    }

    void backAction()
    {
        if (lastAction != ActionType.back)
        {
            Vector2 direction = (bluePlayerPosition - redMinionCentre).normalized;
            float rotY = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
            Vector2 targetPos = bluePlayerPosition + direction * playerAbilityCastRangeWave;

            Vector3 targetPos3D = new Vector3(targetPos.x, 0, targetPos.y);

            playerInput.move = true;
            playerInput.mousePosInGame = targetPos3D;
        }
        //playerInput.move = true;
    }
}

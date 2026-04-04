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
    public ActionType currentAction = ActionType.clear;
    private ActionType lastAction = ActionType.clear;

    public float playerAbilityCastRangeWave = 8;
    public float playerAbilityCastRangePlayer = 10;
    public float maxMinionClusterDistance = 2;

    private Vector2 friendlyTowerPos;
    private Vector2 enemyTowerPos;

    private Vector2 friendlyPlayerSpawn = new Vector2(-19.2f, -19.2f);
    private Vector2 enemyPlayerSpawn = new Vector2(19.2f, 19.2f);

    private Vector2 friendlyMinionCentre = new Vector2(0, 0);
    private Vector2 enemyMinionCentre = new Vector2(0, 0);

    private Vector2 friendlyPlayerPos = new Vector2(0, 0);
    private Vector2 enemyPlayerPos = new Vector2(0, 0);
    private Vector3 friendlyPlayerPos3D = new Vector3(0, 0);
    private Vector3 enemyPlayerPos3D = new Vector3(0, 0);

    private Vector2 ultDestination = new Vector2(0, 0);

    private List<GameObject> friendlyFrontMinionCluster = new List<GameObject>();
    private List<GameObject> enemyFrontMinionCluster = new List<GameObject>();

    private GameObject enemyPlayer;
    private GameObject enemyTower;
    private GameObject friendlyTower;
    private string enemyMinionTag;
    private string friendlyMinionTag;

    private string ultTag;
    private string enemyRootTag;
    private string enemyUltTag;
    public float dodgeDistance = 9;

    private PlayerInputScript playerInput;
    private PlayerAttackScript playerAttack;
    private PlayerMovement playerMovement;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        maxMinionClusterDistance = gameObject.GetComponent<AI_PickAction>().maxMinionClusterDistance;

        if(gameObject.tag == "BluePlayer")
        {
            enemyTowerPos = new Vector2(16.5f, 16.5f);
            friendlyTowerPos = new Vector2(-16.5f, -16.5f);
            
            friendlyTower = GameObject.FindGameObjectWithTag("BlueTower");
            enemyTower = GameObject.FindGameObjectWithTag("RedTower");
            friendlyMinionTag = "BlueMinion";
            enemyMinionTag = "RedMinion";
            friendlyPlayerSpawn = new Vector2(-19.2f, -19.2f);
            enemyPlayerSpawn = new Vector2(19.2f, 19.2f);
            ultTag = "BlueUlt";
            enemyRootTag = "RedRoot";
            enemyUltTag = "RedUlt";
        }
        else
        {
            enemyTowerPos = new Vector2(-16.5f, -16.5f);
            friendlyTowerPos = new Vector2(16.5f, 16.5f);

            friendlyTower = GameObject.FindGameObjectWithTag("RedTower");
            enemyTower = GameObject.FindGameObjectWithTag("BlueTower");
            friendlyMinionTag = "RedMinion";
            enemyMinionTag = "BlueMinion";
            friendlyPlayerSpawn = new Vector2(19.2f, 19.2f);
            enemyPlayerSpawn = new Vector2(-19.2f, -19.2f);
            ultTag = "RedUlt";
            enemyRootTag = "BlueRoot";
            enemyUltTag = "BlueUlt";
        }

        playerInput = gameObject.GetComponent<PlayerInputScript>();
        playerAttack = GetComponent<PlayerAttackScript>();
        playerMovement = GetComponent<PlayerMovement>();

        calculateStateValues();
    }

    // Update is called once per frame
    public void tickUpdate()
    {
        playerInput.move = false;
        playerInput.attack = false;
        playerInput.ability1 = false;
        playerInput.ability2 = false;
        playerInput.ability3 = false;

        calculateStateValues();

        //currentAction = ActionType.trade;
        switch (currentAction)
        {
            case ActionType.trade:
                if (currentAction != lastAction)
                {
                    Debug.Log("ATTACK");
                }
                tradeAction();
                break;
            case ActionType.clear:
                if (currentAction != lastAction)
                {
                    Debug.Log("CLEAR");
                }
                clearAction();
                break;
            case ActionType.tower:
                if (currentAction != lastAction)
                {
                    Debug.Log("TOWER");
                }
                towerAction();
                break;
            case ActionType.back:
                if (currentAction != lastAction)
                {
                    Debug.Log("BACK");
                }
                backAction();
                break;
            default:
                clearAction();
                break;
        }
        lastAction = currentAction;

        if (GameObject.FindGameObjectWithTag(enemyRootTag))
        {
            GameObject enemyRoot = GameObject.FindGameObjectWithTag(enemyRootTag);
            Vector2 enemyRootPos = new Vector2(enemyRoot.transform.position.x, enemyRoot.transform.position.z);

            if (Vector2.Distance(friendlyPlayerPos, enemyRootPos) <= dodgeDistance)
            {
                Vector3 spawn3D = new Vector3(friendlyPlayerSpawn.x, 0, friendlyPlayerSpawn.y);
                playerInput.mousePosInGame = spawn3D;
                playerInput.ability2 = true;
            }
        }

        if (GameObject.FindGameObjectWithTag(enemyUltTag))
        {
            GameObject enemyUlt = GameObject.FindGameObjectWithTag(enemyUltTag);
            Vector2 enemyUltPos = new Vector2(enemyUlt.transform.position.x, enemyUlt.transform.position.z);

            if (Vector2.Distance(friendlyPlayerPos, enemyUltPos) <= dodgeDistance)
            {
                Vector3 spawn3D = new Vector3(friendlyPlayerSpawn.x, 0, friendlyPlayerSpawn.y);
                playerInput.mousePosInGame = spawn3D;
                playerInput.ability2 = true;
            }
        }
    }

    private void calculateStateValues()
    {
        if (!enemyPlayer)
        {
            if (gameObject.tag == "RedPlayer")
            {
                enemyPlayer = GameObject.FindGameObjectWithTag("BluePlayer");
            }
            else
            {
                enemyPlayer = GameObject.FindGameObjectWithTag("RedPlayer");
            }

            if (!enemyPlayer)
            {
                return;
            }
        }

        friendlyPlayerPos.x = transform.position.x;
        friendlyPlayerPos.y = transform.position.z;

        enemyPlayerPos.x = enemyPlayer.transform.position.x;
        enemyPlayerPos.y = enemyPlayer.transform.position.z;

        //friendlyPlayerPosition3D = transform.position;
        enemyPlayerPos3D = enemyPlayer.transform.position;

        friendlyFrontMinionCluster.Clear();
        enemyFrontMinionCluster.Clear();

        {
            GameObject[] friendlyMinions = GameObject.FindGameObjectsWithTag(friendlyMinionTag);
            GameObject[] enemyMinions = GameObject.FindGameObjectsWithTag(enemyMinionTag);

            GameObject friendlyFrontMinion;
            GameObject enemyFrontMinion;

            float shortestFriendlyMinionDistanceToTower = Vector2.Distance(friendlyTowerPos, enemyTowerPos);
            float shortestEnemyMinionDistanceToTower = Vector2.Distance(friendlyTowerPos, enemyTowerPos);

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

            //enemy minion wave
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
    }

    void tradeAction()
    {
        if (!enemyPlayer.activeInHierarchy)
        {
            return;
        }

        if(Vector2.Distance(friendlyPlayerPos, enemyPlayerPos) > playerAbilityCastRangePlayer + 5)
        {
            Vector2 direction = (enemyPlayerPos - friendlyPlayerPos).normalized;
            float rotY = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
            Vector2 targetPos = enemyPlayerPos + direction * playerAbilityCastRangePlayer;

            Vector3 targetPos3D = new Vector3(targetPos.x, -1, targetPos.y);

            playerInput.move = true;
            playerInput.mousePosInGame = targetPos3D;

            playerInput.ability2 = true;
        }
        else if (Vector2.Distance(friendlyPlayerPos, enemyPlayerPos) > playerAbilityCastRangePlayer)
        {
            Vector2 direction = (enemyPlayerPos - friendlyPlayerPos).normalized;
            float rotY = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
            Vector2 targetPos = enemyPlayerPos + direction * playerAbilityCastRangePlayer;

            Vector3 targetPos3D = new Vector3(targetPos.x, -1, targetPos.y);

            playerInput.move = true;
            playerInput.mousePosInGame = targetPos3D;
            
        }
        else
        {
            if (playerAttack.rootIsAvailable)
            {
                playerInput.ability1 = true;
                playerInput.mousePosInGame = enemyPlayerPos3D;
            }
            else if (playerAttack.ultIsAvailable && enemyPlayer.GetComponent<AgentStats>().isStunned)
            {
                playerInput.ability3 = true;
                playerInput.mousePosInGame = enemyPlayerPos3D;
                ultDestination = enemyPlayerPos;
            }
            else
            {
                if (GameObject.FindGameObjectWithTag(ultTag))
                {
                    Vector2 ultPos = new Vector2(GameObject.FindGameObjectWithTag(ultTag).transform.position.x, GameObject.FindGameObjectWithTag(ultTag).transform.position.z);
                    if (Vector2.Distance(ultPos, ultDestination) < 0.1f)
                    {
                        GameObject.FindGameObjectWithTag(ultTag).GetComponent<UltProjectile>().RecastRpc();
                    }
                }
                else
                {
                    playerInput.attack = true;
                    playerInput.move = false;
                    playerInput.target = enemyPlayer;
                }
            }
        }
    }

    void clearAction()
    {
        if(enemyFrontMinionCluster.Count == 0)
        {
            return;
        }

        if (Vector2.Distance(friendlyPlayerPos, enemyMinionCentre) > playerAbilityCastRangeWave)
        {
            Vector2 direction = (enemyMinionCentre - friendlyPlayerPos).normalized;
            float rotY = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
            Vector2 targetPos = enemyMinionCentre + direction * playerAbilityCastRangeWave;

            Vector3 targetPos3D = new Vector3(targetPos.x, -1, targetPos.y);

            playerInput.move = true;
            playerInput.mousePosInGame = targetPos3D;
        }
        else
        {
            if (playerAttack.rootIsAvailable && enemyFrontMinionCluster.Count > 1)
            {
                playerInput.ability1 = true;
                playerInput.mousePosInGame = new Vector3(enemyMinionCentre.x, -1, enemyMinionCentre.y);
            }
            else if (playerAttack.ultIsAvailable && enemyFrontMinionCluster.Count > 4)
            {
                playerInput.ability3 = true;
                playerInput.mousePosInGame = new Vector3(enemyMinionCentre.x, -1, enemyMinionCentre.y);
                ultDestination = enemyMinionCentre;
            }
            else
            {
                if (GameObject.FindGameObjectWithTag(ultTag))
                {
                    Vector2 ultPos = new Vector2(GameObject.FindGameObjectWithTag(ultTag).transform.position.x, GameObject.FindGameObjectWithTag(ultTag).transform.position.z);
                    if (Vector2.Distance(ultPos, ultDestination) < 0.1f)
                    {
                        GameObject.FindGameObjectWithTag(ultTag).GetComponent<UltProjectile>().RecastRpc();
                    }
                }
                else
                {
                    GameObject closestMinion = enemyFrontMinionCluster[0];
                    Vector2 nextMinionPos = new Vector2(closestMinion.transform.position.x, closestMinion.transform.position.z);
                    float shortestDistanceToMinion = Vector2.Distance(friendlyPlayerPos, nextMinionPos);

                    foreach (GameObject minion in enemyFrontMinionCluster)
                    {
                        nextMinionPos.x = minion.transform.position.x;
                        nextMinionPos.y = minion.transform.position.z;

                        if (Vector2.Distance(nextMinionPos, friendlyPlayerPos) < shortestDistanceToMinion)
                        {
                            closestMinion = minion;
                            shortestDistanceToMinion = Vector2.Distance(nextMinionPos, friendlyPlayerPos);
                        }
                    }

                    playerInput.attack = true;
                    playerInput.target = closestMinion;
                }
            }
        }
    }

    void towerAction()
    {
        playerInput.attack = true;
        playerInput.target = enemyTower;
    }

    void backAction()
    {
        if (lastAction != ActionType.back)
        {
            Vector2 direction = (friendlyPlayerSpawn - friendlyPlayerPos).normalized;
            float rotY = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
            Vector2 targetPos = friendlyPlayerPos + direction * playerAbilityCastRangeWave;

            Vector3 targetPos3D = new Vector3(targetPos.x, 0, targetPos.y);

            playerInput.move = true;
            playerInput.attack = false;
            playerMovement.targetEnemy = null;
            playerInput.mousePosInGame = targetPos3D;
        }
        //playerInput.move = true;
    }
}

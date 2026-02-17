using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;

public class AgentStats : NetworkBehaviour
{
    public float health;
    public float passiveHealing;
    public float damage;
    public float speed;
    public float currentSpeed;

    private float healingTimer = 1;

    public float damageLerpDuration;
    public float currentHealth;
    private Coroutine damageCoroutine;

    private HealthUI healthUI;

    public int groundLayer = 8;
    public int enemyLayer;
    public int friendlyLayer;

    //public GameObject enemyPlayer;
    //public GameObject enemyTower;

    public string enemyTowerTag;
    public string enemyMinionTag;
    public string enemyPlayerTag;

    public bool isLocked;
    public float lockTimer;

    public bool isStunned;
    public float stunTimer = 0;

    public bool isSlowed;
    public float slowTimer = 0;
    public float slowPercentage;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        healthUI = GetComponentInChildren<HealthUI>();
        healthUI.Start3DSlider(health);
        currentSpeed = speed;

        currentHealth = health;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner)
        { return; }

        if (slowTimer > 0)
        {
            slowTimer -= Time.deltaTime;
            if (slowTimer <= 0)
            {
                removeSlow();
            }
        }

        if (stunTimer > 0)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0)
            {
                removeStun();
            }
        }

        if (lockTimer > 0)
        {
            lockTimer -= Time.deltaTime;
            if (lockTimer <= 0)
            {
                removeLock();
            }
        }

        healingTimer -= Time.deltaTime;
        if (healingTimer <= 0)
        {
            healingTimer = 1;
            healAmmount(currentHealth, passiveHealing);
        }

        updateOtherSideRpc(currentHealth, isStunned, isSlowed, stunTimer);
    }

    [Rpc(SendTo.Owner)]
    public void takeDamageRpc(float damage, int damageType)
    {
        if(!gameObject.activeInHierarchy)
        {
            return;
        }
        currentHealth -= damage;

        if (damageType == 1)
        {
            if (GetComponent<PlayerManager>())
            {
                GameObject enemyPlayer = GameObject.FindGameObjectWithTag(enemyPlayerTag);
                enemyPlayer.GetComponent<PlayerManager>().aggroAllInRangeRpc();
            }
            else if (GetComponent<MinionManager>())
            {
                MinionManager minion = GetComponent<MinionManager>();
                minion.targetSwitchTimer = minion.CSTimerThreshhold;
            }
        }

        if (currentHealth <= 0 )
        {
            if (GetComponent<MinionManager>())
            {
                if (GetComponent<MinionManager>().targetSwitchTimer > 0)
                {
                    if (damageType == 1)
                    {
                        PlayerManager playerScr = GameObject.FindGameObjectWithTag(GetComponent<AgentStats>().enemyPlayerTag).GetComponent<PlayerManager>();
                        playerScr.incrementCsRpc();
                    }
                }
                Destroy(gameObject);
            }
            else if(GetComponent<PlayerManager>())
            {
                bool isblueplayer = false;
                if(gameObject.tag == "BluePlayer")
                {
                    isblueplayer = true;
                }
                currentHealth = health;
                GetComponent<PlayerManager>().resetPlayerComponents();
                healthUI.Update3DSlider(currentHealth);
                removeSlow();
                removeStun();
                removeLock();
                GameManagerScript.Instance.handlePlayerDeathRpc(isblueplayer);

                if(isblueplayer)
                {
                    GetComponent<PlayerAttackScript>().rootCooldownTimer -= GameManagerScript.Instance.bluePlayerRespawnCooldown;
                }
                else
                {
                    GetComponent<PlayerAttackScript>().rootCooldownTimer -= GameManagerScript.Instance.redPlayerRespawnCooldown;
                }

            }
            else if(GetComponent<TowerScript>())
            {
                GameManagerScript.LoadNetwork(GameManagerScript.Scene.GameEnd);

                if (friendlyLayer == 9)
                {
                    GameManagerScript.Instance.blueWins = true;
                }
                else
                {
                    GameManagerScript.Instance.blueWins = false;
                }
                GameManagerScript.Instance.gameDone = true;
                
            }
        }
        //else
        //{
        //    startLerpHealth();
        
        //}

        updateOtherSideRpc(currentHealth, isStunned, isSlowed, stunTimer);
    }

    [Rpc(SendTo.Owner)]
    public void applySlowRpc(float slow, float slowDuration = 0.2f)
    {
        if (!IsOwner)
        { return; }

        if (!isStunned)
        {
            currentSpeed = speed * slow;
        }

        isSlowed = true;
        slowTimer = slowDuration;
        slowPercentage = slow;
    }

    [Rpc(SendTo.Owner)]
    public void applyStunRpc(float stunDuration)
    {
        isStunned = true;
        stunTimer = stunDuration;
        currentSpeed = 0;
    }

    public void applyLock(float duration)
    {
        isLocked = true;
        lockTimer = duration;
        currentSpeed = 0;
    }
    public void removeSlow()
    {
        isSlowed = false;
        slowTimer = 0;
        slowPercentage = 0;

        if (!isStunned)
        {
            currentSpeed = speed;
        }
    }

    public void removeStun()
    {
        isStunned = false;
        stunTimer = 0;

        if (isSlowed)
        {
            currentSpeed = speed * slowPercentage;
        }
        else
        {
            currentSpeed = speed;
        }
    }

    public void removeLock()
    {
        isLocked = false;
        lockTimer = 0;

        if (isSlowed)
        {
            currentSpeed = speed * slowPercentage;
        }
        else
        {
            currentSpeed = speed;
        }
    }

    private void healAmmount(float currentTargetHP, float amount)
    {

        currentHealth = currentTargetHP + amount;
        if (currentHealth > health)
        {
            currentHealth = health;
        }
        healthUI.Update3DSlider(currentHealth);
    }

    //private void startLerpHealth()
    //{
    //    if (damageCoroutine == null)
    //    {
    //        damageCoroutine = StartCoroutine(lerpHealth());
    //    }
    //}

    //private IEnumerator lerpHealth()
    //{
    //    float elapsedTime = 0;
    //    float initialHealth = currentHealth;
    //    float target = targetHealth;

    //    while (elapsedTime < damageLerpDuration)
    //    {
    //        currentHealth = Mathf.Lerp(initialHealth, target, elapsedTime / damageLerpDuration);
    //        updateHealthUI();

    //        elapsedTime += Time.deltaTime;
    //        yield return null;
    //    }

    //    currentHealth = target;
    //    updateHealthUI();

    //    damageCoroutine = null;
    //}

    //[Rpc(SendTo.Everyone)]
    //private void resetStatsRpc()
    //{
    //    //if(GetComponent<PlayerManager>())
    //    //{
    //    //    currentHealth = health;
    //    //    updateHealthUI();
    //    //    GetComponent<PlayerManager>().resetPlayerComponents();
    //    //}

    //}

    [Rpc(SendTo.Everyone)]
    private void updateOtherSideRpc(float curHealth, bool stun, bool slow, float stunRemaining)
    {
        if(!gameObject.activeInHierarchy)
        {
            return;
        }

        currentHealth = curHealth;
        isStunned = stun;
        isSlowed = slow;
        stunTimer = stunRemaining;

        if (healthUI)
        {
            healthUI.Update3DSlider(currentHealth);
        }
    }
}

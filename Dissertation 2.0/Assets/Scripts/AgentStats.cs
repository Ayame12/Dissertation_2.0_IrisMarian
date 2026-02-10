using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class AgentStats : NetworkBehaviour
{
    public float health;
    public float damage;
    public float speed;
    public float currentSpeed;

    public float damageLerpDuration;
    private float currentHealth;
    private float targetHealth;
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

    public bool isStunned;
    public float stunTimer = 0;

    public bool isSlowed;
    public float slowTimer = 0;
    public float slowPercentage;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentSpeed = speed;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner)
        { return; }

        if (slowTimer > 0)
        {
            slowTimer -= Time.deltaTime;
            if (slowTimer == 0)
            {
                removeSlow();
            }
        }
        else if (slowTimer < 0)
        {
            removeSlow();
        }

        if (stunTimer > 0)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer == 0)
            {
                removeStun();
            }
        }
        else if (stunTimer < 0)
        {
            removeStun();
        }
    }

    private void Awake()
    {
        healthUI = GetComponentInChildren<HealthUI>();
        currentHealth = health;
        targetHealth = health;

        healthUI.Start3DSlider(health);

    }

    [Rpc(SendTo.Owner)]
    public void takeDamageRpc(float damage, int damageType)
    {
        targetHealth -= damage;

        if (damageType == 1)
        {
            if (GetComponent<PlayerManager>())
            {
                //enemyPlayer.GetComponent<PlayerManager>().aggroAllInRange();
            }
            //else if (GetComponent<MinionScript>())
            //{
            //    MinionScript minion = GetComponent<MinionScript>();
            //    minion.targetSwitchTimer = minion.CSTimerThreshhold;
            //}
        }

        if (targetHealth <= 0 )
        {
            if (GetComponent<MinionManager>())
            {
                if (GetComponent<MinionManager>().targetSwitchTimer > 0)
                {
                    PlayerManager playerScr = GameObject.FindGameObjectWithTag(GetComponent<MinionManager>().enemyPlayerTag).GetComponent<PlayerManager>();
                    playerScr.creepScore++;
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
                targetHealth = health;
                GetComponent<PlayerManager>().resetPlayerComponents();
                updateHealthUI();
                GameManagerScript.Instance.handlePlayerDeathRpc(isblueplayer);
            }
            
        }
        else
        {
            startLerpHealth();
        
        }
    }

    [Rpc(SendTo.Everyone)]
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

    [Rpc(SendTo.Everyone)]
    public void applyStunRpc(float stunDuration)
    {
        isStunned = true;
        stunTimer = stunDuration;
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

    private void startLerpHealth()
    {
        if (damageCoroutine == null)
        {
            damageCoroutine = StartCoroutine(lerpHealth());
        }
    }

    private IEnumerator lerpHealth()
    {
        float elapsedTime = 0;
        float initialHealth = currentHealth;
        float target = targetHealth;

        while (elapsedTime < damageLerpDuration)
        {
            currentHealth = Mathf.Lerp(initialHealth, target, elapsedTime / damageLerpDuration);
            updateHealthUI();

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        currentHealth = target;
        updateHealthUI();

        damageCoroutine = null;
    }

    private void updateHealthUI()
    {
        healthUI.Update3DSliderRpc(currentHealth);
    }

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
}

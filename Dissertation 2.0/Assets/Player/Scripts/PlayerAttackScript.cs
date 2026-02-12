using JetBrains.Annotations;
using TMPro;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAttackScript : NetworkBehaviour
{
    private PlayerInputScript playerInput;
    private PlayerMovement playerMovement;

    [Header("Ability 1")]
    public Image ability1Image;
    public Text ability1Text;

    public GameObject rootPrefab;
    public float rootCooldown;
    public float rootCooldownTimer = 0;
    public bool rootIsAvailable = true;

    [Header("Ability 2")]
    public Image ability2Image;
    public Text ability2Text;

    public float dashRange;
    public float dashSpeed;
    public float dashCooldown;
    public float dashCooldownTimer;
    public bool dashIsAvailable = true;


    [Header("Ability 3")]
    public Image ability3Image;
    public Text ability3Text;
    
    public GameObject ultPrefab;
    public string ultTag;
    public float ultCooldown;
    public float ultCooldownTimer = 0;
    public bool ultIsAvailable = true;

    private void Start()
    {
        playerInput = GetComponent<PlayerInputScript>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    public void tickUpdate()
    {
        if (playerInput.ability1 && rootIsAvailable)
        {
            spawnAbilityRpc(NetworkManager.Singleton.LocalClientId, 1, transform.position, playerInput.mousePosInGame);
            rootCooldownTimer = rootCooldown;
            rootIsAvailable = false;
        }

        if(playerInput.ability2 && dashIsAvailable)
        {
            Vector3 initPos = transform.position;

            Vector3 direction = (playerInput.mousePosInGame - initPos).normalized;
            float rotY = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            Vector3 targetPos = initPos + direction * dashRange;

            playerMovement.startDash(initPos, targetPos, rotY);

            dashCooldownTimer = dashCooldown;
            dashIsAvailable = false;
        }

        if (playerInput.ability3)
        {
            if(ultIsAvailable)
            {
                spawnAbilityRpc(NetworkManager.Singleton.LocalClientId, 3, transform.position, playerInput.mousePosInGame);
                ultCooldownTimer = ultCooldown;
                ultIsAvailable = false;
            }
            else
            {
                if(GameObject.FindGameObjectWithTag(ultTag))
                {
                    GameObject.FindGameObjectWithTag(ultTag).GetComponent<UltProjectile>().RecastRpc();
                }
            }
        }

        if (rootCooldownTimer > 0)
        {
            rootCooldownTimer -= Time.deltaTime;
            if (rootCooldownTimer <= 0)
            {
                rootCooldownTimer = 0;
                rootIsAvailable = true;
            }
        }

        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
            if (dashCooldownTimer <= 0)
            {
                dashCooldownTimer = 0;
                dashIsAvailable = true;
            }
        }

        if (ultCooldownTimer > 0)
        {
            ultCooldownTimer -= Time.deltaTime;
            if (ultCooldownTimer <= 0)
            {
                ultCooldownTimer = 0;
                ultIsAvailable = true;
            }
        }

        //UI stuff

        if (!rootIsAvailable)
        {
            ability1Image.fillAmount = rootCooldownTimer / rootCooldown;
            ability1Text.text = Mathf.Ceil(rootCooldownTimer).ToString();
        }
        else
        {
            ability1Text.text = "";
        }

        if (!dashIsAvailable)
        {
            ability2Image.fillAmount = dashCooldownTimer / dashCooldown;
            ability2Text.text = Mathf.Ceil(dashCooldownTimer).ToString();
        }
        else
        {
            ability2Text.text = "";
        }

        if (!ultIsAvailable)
        {
            ability3Image.fillAmount = ultCooldownTimer / ultCooldown;
            ability3Text.text = Mathf.Ceil(ultCooldownTimer).ToString();
        }
        else
        {
            ability3Text.text = "";
        }

    }

    [Rpc(SendTo.Server)]
    private void spawnAbilityRpc( ulong cliendId, int abilityId, Vector3 initialPos, Vector3 targetPos)
    {
        ulong debugId = NetworkManager.Singleton.LocalClientId;

        GameObject ability = null;

        //Quaternion rotation = Quaternion.LookRotation(playerInput.mousePosInGame - transform.position);

        if (abilityId == 1)
        {
            Vector3 initRootPos = new Vector3(initialPos.x, -0.5f, initialPos.z);
            Vector3 targetRootPos = new Vector3(targetPos.x, -0.5f, targetPos.z);

            Vector3 direction = (targetRootPos - initRootPos).normalized;
            float rot = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;


            ability = Instantiate(rootPrefab, initRootPos, Quaternion.Euler(0, rot, 0));
            ability.GetComponent<NetworkObject>().SpawnWithOwnership(cliendId, true);
            ability.GetComponent<RootProjectile>().initializeRpc(initRootPos, targetRootPos, rot);
        }
        else if(abilityId == 3)
        {
            Vector3 initUltPos = new Vector3(initialPos.x, -0.5f, initialPos.z);
            Vector3 targetUltPos = new Vector3(targetPos.x, -0.5f, targetPos.z);

            Vector3 direction = (targetUltPos - initUltPos).normalized;
            float rot = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;


            ability = Instantiate(ultPrefab, initUltPos, Quaternion.Euler(0, rot, 0));
            ability.GetComponent<NetworkObject>().SpawnWithOwnership(cliendId, true);
            ability.GetComponent<UltProjectile>().initializeRpc(initUltPos, targetUltPos, rot);
        }
        else
        {
            Debug.Log("Invalid Ability ID");
            return;
        }

    }

    //old script --------------------------------------------------------------------------------------------------------

    //PlayerInputScript playerInput;

    //[Header("Ability 1")]
    //public Image ability1Image;
    //public Text ability1Text;
    //public string ability1Tag;

    //private AbilityScript ability1Script;
    //private GameObject ability1;

    ////[Header("Ability 2")]
    ////public Image ability2Image;
    ////public Text ability2Text;
    ////public string ability2Tag;

    ////private AbilityScript ability2Script;
    ////private GameObject ability2;

    ////[Header("Ability 3")]
    ////public Image ability3Image;
    ////public Text ability3Text;
    ////public string ability3Tag;

    ////private AbilityScript ability3Script;
    ////private GameObject ability3;

    //// Start is called once before the first execution of Update after the MonoBehaviour is created
    //public void initialize()
    //{
    //    if(!IsOwner)
    //    { return; }

    //    playerInput = GetComponent<PlayerInputScript>();

    //    ability1 = GameObject.FindGameObjectWithTag(ability1Tag);
    //    ability1Script = ability1.GetComponent<AbilityScript>();
    //    ability1Script.initialize(gameObject);
    //    ability1Text.text = "";
    //    ability1Image.fillAmount = 0;
    //    ability1.transform.position = new Vector3(-10, 0, -10);

    //    //ability2 = GameObject.FindGameObjectWithTag(ability2Tag);
    //    //ability2Script = ability2.GetComponent<AbilityScript>();
    //    //ability2Script.initialize(gameObject);
    //    //ability2Text.text = "";
    //    //ability2Image.fillAmount = 0;
    //    //ability2.transform.position = new Vector3(-10, 0, -10);

    //    //ability3 = GameObject.FindGameObjectWithTag(ability3Tag);
    //    //ability3Script = ability3.GetComponent<AbilityScript>();
    //    //ability3Script.initialize(gameObject);
    //    //ability3Text.text = "";
    //    //ability3Image.fillAmount = 0;
    //    //ability3.transform.position = new Vector3(-10, 0, -10);

    //    //if (IsOwner)
    //    //{
    //    //    ability1 = Instantiate(ability1Prefab);
    //    //    ability1.GetComponent<NetworkObject>().SpawnWithOwnership(NetworkManager.LocalClientId);

    //    //    ability1Image.fillAmount = 0;
    //    //    //ability2Image.fillAmount = 0;
    //    //    //ability3Image.fillAmount = 0;

    //    //    ability1Text.text = "";
    //    //    //ability2Text.text = "";
    //    //    //ability3Text.text = "";

    //    //    //ability1Script.initialize(gameObject);
    //    //    //ability2Script.initialize(gameObject);
    //    //    //ability3Script.initialize(gameObject);
    //    //}
    //}

    //// Update is called once per frame
    //public void tickUpdate()
    //{
    //    if (playerInput.ability1 && ability1Script.isAvailable)
    //    {
    //        ability1Script.action();
    //    }

    //    //if (playerInput.ability2 && ability2Script.isAvailable)
    //    //{
    //    //    ability2Script.action();
    //    //}

    //    //if (playerInput.ability3 && ability3Script.isAvailable)
    //    //{
    //    //    ability3Script.action();
    //    //}

    //    if (!ability1Script.isAvailable)
    //    {
    //        ability1Image.fillAmount = ability1Script.cooldownTimer / ability1Script.cooldown;
    //        ability1Text.text = Mathf.Ceil(ability1Script.cooldownTimer).ToString();
    //    }
    //    else
    //    {
    //        ability1Text.text = "";
    //    }

    //    //if (!ability2Script.isAvailable)
    //    //{
    //    //    ability2Image.fillAmount = ability2Script.cooldownTimer / ability2Script.cooldown;
    //    //    ability2Text.text = Mathf.Ceil(ability2Script.cooldownTimer).ToString();
    //    //}
    //    //else
    //    //{
    //    //    ability2Text.text = "";
    //    //}

    //    //if (!ability3Script.isAvailable)
    //    //{
    //    //    ability3Image.fillAmount = ability3Script.cooldownTimer / ability3Script.cooldown;
    //    //    ability3Text.text = Mathf.Ceil(ability3Script.cooldownTimer).ToString();
    //    //}
    //    //else
    //    //{
    //    //    ability3Text.text = "";
    //    //}
    //}
}

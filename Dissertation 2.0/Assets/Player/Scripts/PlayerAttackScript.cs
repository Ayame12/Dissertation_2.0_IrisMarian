using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAttackScript : NetworkBehaviour
{
    PlayerInputScript playerInput;

    [Header("Ability 1")]
    public Image ability1Image;
    public Text ability1Text;
    public string ability1Tag;

    private AbilityScript ability1Script;
    private GameObject ability1;

    //[Header("Ability 2")]
    //public Image ability2Image;
    //public Text ability2Text;
    //public string ability2Tag;

    //private AbilityScript ability2Script;
    //private GameObject ability2;

    //[Header("Ability 3")]
    //public Image ability3Image;
    //public Text ability3Text;
    //public string ability3Tag;

    //private AbilityScript ability3Script;
    //private GameObject ability3;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void initialize()
    {
        if(!IsOwner)
        { return; }

        playerInput = GetComponent<PlayerInputScript>();

        ability1 = GameObject.FindGameObjectWithTag(ability1Tag);
        ability1Script = ability1.GetComponent<AbilityScript>();
        ability1Script.initialize(gameObject);
        ability1Text.text = "";
        ability1Image.fillAmount = 0;
        ability1.transform.position = new Vector3(-10, 0, -10);

        //ability2 = GameObject.FindGameObjectWithTag(ability2Tag);
        //ability2Script = ability2.GetComponent<AbilityScript>();
        //ability2Script.initialize(gameObject);
        //ability2Text.text = "";
        //ability2Image.fillAmount = 0;
        //ability2.transform.position = new Vector3(-10, 0, -10);

        //ability3 = GameObject.FindGameObjectWithTag(ability3Tag);
        //ability3Script = ability3.GetComponent<AbilityScript>();
        //ability3Script.initialize(gameObject);
        //ability3Text.text = "";
        //ability3Image.fillAmount = 0;
        //ability3.transform.position = new Vector3(-10, 0, -10);

        //if (IsOwner)
        //{
        //    ability1 = Instantiate(ability1Prefab);
        //    ability1.GetComponent<NetworkObject>().SpawnWithOwnership(NetworkManager.LocalClientId);

        //    ability1Image.fillAmount = 0;
        //    //ability2Image.fillAmount = 0;
        //    //ability3Image.fillAmount = 0;

        //    ability1Text.text = "";
        //    //ability2Text.text = "";
        //    //ability3Text.text = "";

        //    //ability1Script.initialize(gameObject);
        //    //ability2Script.initialize(gameObject);
        //    //ability3Script.initialize(gameObject);
        //}
    }

    // Update is called once per frame
    public void tickUpdate()
    {
        if (playerInput.ability1 && ability1Script.isAvailable)
        {
            ability1Script.action();
        }

        //if (playerInput.ability2 && ability2Script.isAvailable)
        //{
        //    ability2Script.action();
        //}

        //if (playerInput.ability3 && ability3Script.isAvailable)
        //{
        //    ability3Script.action();
        //}

        if (!ability1Script.isAvailable)
        {
            ability1Image.fillAmount = ability1Script.cooldownTimer / ability1Script.cooldown;
            ability1Text.text = Mathf.Ceil(ability1Script.cooldownTimer).ToString();
        }
        else
        {
            ability1Text.text = "";
        }

        //if (!ability2Script.isAvailable)
        //{
        //    ability2Image.fillAmount = ability2Script.cooldownTimer / ability2Script.cooldown;
        //    ability2Text.text = Mathf.Ceil(ability2Script.cooldownTimer).ToString();
        //}
        //else
        //{
        //    ability2Text.text = "";
        //}

        //if (!ability3Script.isAvailable)
        //{
        //    ability3Image.fillAmount = ability3Script.cooldownTimer / ability3Script.cooldown;
        //    ability3Text.text = Mathf.Ceil(ability3Script.cooldownTimer).ToString();
        //}
        //else
        //{
        //    ability3Text.text = "";
        //}
    }
}

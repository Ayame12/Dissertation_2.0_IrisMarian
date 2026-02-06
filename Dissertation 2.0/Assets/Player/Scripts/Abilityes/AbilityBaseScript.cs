using Unity.Netcode;
using UnityEngine;

public class AbilityBaseScript : NetworkBehaviour
{
    public int enemyLayer;
    public int friendlyLayer;
    public string enemyTowerTag;
    public string enemyPlayerTag;
    public string enemyMinionTag;

    public float cooldown;

    // Update is called once per frame
    void Update()
    {
        
    }
}

using UnityEngine;

public class PlayerSpawnPointScript : MonoBehaviour
{
    public bool isBlueSpawn;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManagerScript.Instance.setupPlayerSpawnPointTransforms(isBlueSpawn, gameObject.transform);
    }
}

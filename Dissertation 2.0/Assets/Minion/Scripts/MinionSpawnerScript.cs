using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class MinionSpawnerScript : NetworkBehaviour
{
    public GameObject minionPrefab;
    //public Transform spawnPoint;
    public float waveSpawnInterval;
    public float minionSpawnInterval;
    public int minionsperWave;
    public float firstWaveDelay;

    private int minionIdentifier = 2;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnNetworkSpawn()
    {

        if (NetworkManager.IsHost)
        {
            StartCoroutine(spawnMinions());
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    private IEnumerator spawnMinions()
    {
        yield return new WaitForSeconds(firstWaveDelay);
        while (true)
        {
            for (int i = 0; i < minionsperWave; i++)
            {
                spawnMinion();

                ++minionIdentifier;
                if(minionIdentifier >= 100)
                {
                    minionIdentifier = 2;
                }
                yield return new WaitForSeconds(minionSpawnInterval);
            }

            yield return new WaitForSeconds(waveSpawnInterval);
        }
    }

    private void spawnMinion()
    {
        GameObject minion = Instantiate(minionPrefab, gameObject.transform.position, gameObject.transform.rotation);
        minion.GetComponent<NetworkObject>().Spawn(true);
        
        minion.GetComponent<MinionManager>().setIdentifierRpc(minionIdentifier);
    }
}

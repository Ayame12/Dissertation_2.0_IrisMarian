using System.Runtime.CompilerServices;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraScript : MonoBehaviour
{
    public CinemachineCamera cineCam;

    private bool setupDone = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!setupDone)
        {
            if (NetworkManager.Singleton.IsHost)
            {
                if(GameObject.FindGameObjectWithTag("BluePlayer"))
                {
                    cineCam.Target.TrackingTarget = GameObject.FindGameObjectWithTag("BluePlayer").transform;
                    setupDone = true;
                }
                
            }
            else
            {
                if (GameObject.FindGameObjectWithTag("RedPlayer"))
                {
                    cineCam.Target.TrackingTarget = GameObject.FindGameObjectWithTag("RedPlayer").transform;
                    setupDone = true;
                }
            }
        }
    }
}

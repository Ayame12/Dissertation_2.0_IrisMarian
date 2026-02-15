using System;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;

public class MenuSceneManager : MonoBehaviour
{
    public Button hostGameButton;
    public Button joinGameButton;

    public TMP_InputField ipInput;
    public TMP_InputField playerIdentifier;

    private void Awake()
    {
        hostGameButton.onClick.AddListener(() => {
            GameManagerScript.Instance.StartHost();
            GameManagerScript.LoadNetwork(GameManagerScript.Scene.GameSetupScene);
        });

        joinGameButton.onClick.AddListener(() => {

            string ipString = ipInput.text;

            if (!string.IsNullOrEmpty(ipString))
            {
                NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Address = ipString;
                NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Port = 7777;
            }

            bool clientStarted = NetworkManager.Singleton.StartClient();

        });
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(string.IsNullOrEmpty(playerIdentifier.text))
        {
            hostGameButton.GetComponent<Button>().enabled = false;
            joinGameButton.GetComponent<Button>().enabled = false;
        }
        else
        {
            hostGameButton.GetComponent<Button>().enabled = true;
            joinGameButton.GetComponent<Button>().enabled = true;
        }
    }

    //public void hostGame()
    //{
    //    gameManager.isHost = true;
    //    gameManager.waitingToLoadGame = true;

        

    //    //SceneManager.LoadScene(1);
    //    //networkManager.StartHost();
        
    //}

    //public void joinGame()
    //{
    //    gameManager.isHost = false;
    //    gameManager.waitingToLoadGame = true;

    //    //SceneManager.LoadScene(1);
    //    //networkManager.StartClient();
    //}
}

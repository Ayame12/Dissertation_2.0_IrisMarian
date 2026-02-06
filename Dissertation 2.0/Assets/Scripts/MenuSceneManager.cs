using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;

public class MenuSceneManager : MonoBehaviour
{
    public Button hostGameButton;
    public Button joinGameButton;

    public TMP_InputField input;

    private void Awake()
    {
        hostGameButton.onClick.AddListener(() => {
            GameManagerScript.Instance.StartHost();
            GameManagerScript.LoadNetwork(GameManagerScript.Scene.GameSetupScene);
        });

        joinGameButton.onClick.AddListener(() => {

            string ipString = input.text;

            if (string.IsNullOrEmpty(ipString))
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

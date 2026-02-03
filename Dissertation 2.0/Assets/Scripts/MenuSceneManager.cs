using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuSceneManager : MonoBehaviour
{
    public Button hostGameButton;
    public Button joinGameButton;

    private void Awake()
    {
        hostGameButton.onClick.AddListener(() => {
            GameManagerScript.Instance.StartHost();
            GameManagerScript.LoadNetwork(GameManagerScript.Scene.GameSetupScene);
        });

        joinGameButton.onClick.AddListener(() => {
            GameManagerScript.Instance.StartClient();
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

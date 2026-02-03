using System.Collections.Generic;
using System.Xml.Serialization;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameSetupScene : MonoBehaviour
{
    public Button readyButton;

    

    private void Awake()
    {
        readyButton.onClick.AddListener(() =>
        {
            CharacterSelectReady.Instance.setPlayerReady();
        });
    }

    
}

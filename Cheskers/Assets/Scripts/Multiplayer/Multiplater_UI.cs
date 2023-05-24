using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
public class Multiplater_UI : MonoBehaviour
{
    //Temporary, need to make a lobby
    [SerializeField] Button hostButton;
    [SerializeField] Button clientButton;

    public static event EventHandler OnGameHosted;
    public static event EventHandler OnGameClientJoined;

    [SerializeField] GameObject playerPrefab;

    // Start is called before the first frame update
    void Start()
    {
        if (Multiplayer_Manager.instance.isMultiplayerGame == true) {
            hostButton.onClick.AddListener(() =>
            {
                GetComponent<NetworkManager>().StartHost();
                Invoke("GameHosted", 1f);
            }
                );
            clientButton.onClick.AddListener(() =>
            {
                GetComponent<NetworkManager>().StartClient();
                Invoke("ClinetJoined", 1f);
            }
                );
        }
        else {
            hostButton.gameObject.SetActive(false);
            clientButton.gameObject.SetActive(false);
            Instantiate(playerPrefab);
        }
    }

    void GameHosted()
    {
        OnGameHosted?.Invoke(this, EventArgs.Empty);
        Debug.Log(Multiplayer_Manager.instance.isMultiplayerGame);
    }

    void ClinetJoined()
    {
        OnGameClientJoined?.Invoke(this, EventArgs.Empty);
        Debug.Log(Multiplayer_Manager.instance.isMultiplayerGame);
    }

}

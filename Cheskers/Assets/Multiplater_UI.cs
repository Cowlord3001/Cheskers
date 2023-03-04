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

    // Start is called before the first frame update
    void Start()
    {
        hostButton.onClick.AddListener(() => {
            GetComponent<NetworkManager>().StartHost();
            Invoke("GameHosted", .2f);
            }
            ) ;
        clientButton.onClick.AddListener(() => {
            GetComponent<NetworkManager>().StartClient();
            Invoke("ClinetJoined", .2f);
        }
            );
    }

    void GameHosted()
    {
        OnGameHosted?.Invoke(this, EventArgs.Empty);
    }

    void ClinetJoined()
    {
        OnGameClientJoined?.Invoke(this, EventArgs.Empty);
    }

}

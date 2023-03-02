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

    // Start is called before the first frame update
    void Start()
    {
        hostButton.onClick.AddListener(() =>
            GetComponent<NetworkManager>().StartHost());
        clientButton.onClick.AddListener(() =>
            GetComponent<NetworkManager>().StartClient());
    }

}

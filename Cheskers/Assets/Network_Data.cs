using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Network_Data : NetworkBehaviour
{
    //There will be only 1 network_data in the game
    public static Network_Data instance;
    //This script will update data across users.
    //TODO: Update board function -> Syncs board data across users
    //TODO: Update Piece
    //TODO: Turn tracking that both clients agree on
    //TODO: Reroll token tracking

    private void Awake()
    {
        instance = this;
    }

    public void UpdateBoardData()
    {
        //Board Data already updated on the client side when this is called
        if(IsOwner == true) { return; }
        UpdateBoardDataServerRPC();

    }

    [ServerRpc(RequireOwnership = false)]//Can be called from any client
    void UpdateBoardDataServerRPC()
    {

    }

    [ClientRpc]
    void UpdateBoardDataClientRPC()
    {

    }
}

public struct SerializableBoardData{
    //Parallel arrays of basic type data?
    //Maybe turn piece_data into a struct so it can be serialized for network transport

}

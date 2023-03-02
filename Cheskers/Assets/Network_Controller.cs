using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Network_Controller : NetworkBehaviour
{
    //There will be only 1 network_controller in the game
    public static Network_Controller instance;

    Piece_Data.Color turnColor = Piece_Data.Color.white;
    //Need to know players turn. what should track it
    //Need players multiplayer ID's 
    int whitePlayerID;
    bool whitePlayerDeclined = false;
    int blackPlayerID;
    bool blackPlayerDeclined = false;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        
    }
    //Need event for contest starting so we can popup window for both players
    //Then Need an event for listening for input from either players
    void OnContestStarting()
    {
        blackPlayerDeclined = false;
        whitePlayerDeclined = false;

    }

    //Transforming Pieces and Highlighted Squares
    void OnTransformPiece()
    {

    }

    void OnHighLightSquares()
    {

    }
    //Removing Highlights from Squares
    void OnRemoveHighLightSquares()
    {

    }
    //EndOfTurn Reset the Pieces
    void OnEndTurn()
    {

    }
    //Detect when declined for each player
    void OnDeclinedPressed()
    {

    }
    //Someone decides to reroll
    void OnContestButtonPressed(object sender, EventArgs e)
    {

    }

    //Sync when a piece is removed
    void OnPieceRemovedFromBoard(object sender, Board_Data.PieceRemovedEventArgs e)
    {
        //Board Data already updated on the client side when this is called
        
        //TODO: Check which player called this to make sure its real

        PieceRemovedServerRPC();

    }

    //Sync when a piece is damaged
    void OnPieceDamaged(object sender, Board_Data.PieceDamageEventArgs e)
    {
        //TODO: Check which player called this to make sure its real
        PieceDamagedServerRPC();
    }

    //Sync when a piece is moved
    void OnPieceMoved(object sender)
    {

    }

    [ServerRpc(RequireOwnership = false)]//Can be called from any client
    void PieceRemovedServerRPC()
    {
        PieceRemovedClientRPC();
    }

    [ClientRpc]
    void PieceRemovedClientRPC()
    {

    }

    [ServerRpc(RequireOwnership = false)]//Can be called from any client
    void PieceDamagedServerRPC()
    {
        PieceDamagedClientRPC();
    }

    [ClientRpc]
    void PieceDamagedClientRPC()
    {

    }
}

public struct SerializableBoardData{
    //Parallel arrays of basic type data?
    //Maybe turn piece_data into a struct so it can be serialized for network transport

}

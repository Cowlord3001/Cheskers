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
        //Calls from changes to pieces
        Board_Data.instance.OnPieceRemovedFromBoard += OnPieceRemovedFromBoard;
        Board_Data.instance.OnPieceDamaged += OnPieceDamaged;
        Board_Data.instance.OnPieceMoved += OnPieceMoved;
        //Calls from Update in Graphics
        //TODO: These need to be linked up when the game is hosted and after the players are spawned not here
        if (Piece_Controller.instance != null) {
            Piece_Controller.instance.OnPieceTransformed += OnPieceTransformed;
            Piece_Controller.instance.OnValidMovesHighlighted += OnValidMovesHighlighted;
            Piece_Controller.instance.OnValidMovesDeHighlighted += OnValidMovesDeHighlighted;
        }
        //Calls from inputcontroller
        //TODO:Need to check or send information about who is clicking the buttons.
        Input_Controller.instance.OnContestButtonClicked += OnContestButtonPressed;
        Input_Controller.instance.OnDeclineButtonClicked += OnDeclinedPressed;

        //TODO: Implement EndofTurn event and turns in general for this event
    }
    //Need event for contest starting so we can popup window for both players
    //Then Need an event for listening for input from either players
    void OnContestStarting()
    {
        blackPlayerDeclined = false;
        whitePlayerDeclined = false;

    }

    //Transforming Pieces and Highlighted Squares
    void OnPieceTransformed(object sender, Piece_Controller.EventArgsOnPieceTransformed e) 
    {

    }

    void OnValidMovesHighlighted(object sender, Piece_Controller.EventArgsOnValidMovesHighlighted e)
    {

    }
    //Removing Highlights from Squares
    void OnValidMovesDeHighlighted(object sender, Piece_Controller.EventArgsOnValidMovesDeHighlighted e)
    {

    }
    //EndOfTurn Reset the Pieces
    void OnEndTurn()
    {

    }
    //Detect when declined for each player
    void OnDeclinedPressed(object sender, EventArgs e)
    {

    }
    //Someone decides to reroll
    void OnContestButtonPressed(object sender, EventArgs e)
    {

    }

    //Sync when a piece is removed
    void OnPieceRemovedFromBoard(object sender, Board_Data.EventArgsPieceRemoved e)
    {
        //Board Data already updated on the client side when this is called
        
        //TODO: Check which player called this to make sure its real

        PieceRemovedServerRPC();

    }

    //Sync when a piece is damaged
    void OnPieceDamaged(object sender, Board_Data.EventArgsPieceDamaged e)
    {
        //TODO: Check which player called this to make sure its real
        PieceDamagedServerRPC();
    }

    //Sync when a piece is moved
    void OnPieceMoved(object sender, Board_Data.EventArgsPieceMoved e)
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


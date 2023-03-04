using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Network_Controller : NetworkBehaviour
{
    //There will be only 1 network_controller in the game
    public static Network_Controller instance;

    public NetworkVariable<Piece_Data.Color> turnColor;
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
        NetworkVariable<Piece_Data.Color> turnColor = new NetworkVariable<Piece_Data.Color>();
        //Detecting Game being hosted and clinet joining
        Multiplater_UI.OnGameHosted += OnGameHosted;
        Multiplater_UI.OnGameClientJoined += OnGameClientJoined;

        //Calls from changes to pieces
        Board_Data.instance.OnPieceRemovedFromBoard += OnPieceRemovedFromBoard;
        Board_Data.instance.OnPieceDamaged += OnPieceDamaged;
        Board_Data.instance.OnPieceMoved += OnPieceMoved;
        
        //Calls from inputcontroller
        //TODO:Need to check or send information about who is clicking the buttons.
        Input_Controller.instance.OnContestButtonClicked += OnContestButtonPressed;
        Input_Controller.instance.OnDeclineButtonClicked += OnDeclinedPressed;

        //TODO: Implement EndofTurn event and turns in general for this event
    }
    void OnGameHosted(object sender, EventArgs e)
    {
        //White goes first
        turnColor.Value = Piece_Data.Color.white;
        //Set Host to white
        Piece_Controller.instance.color = Piece_Data.Color.white;
        if (Piece_Controller.instance != null) {
            Piece_Controller.instance.OnPieceTransformed += OnPieceTransformed;
            Piece_Controller.instance.OnValidMovesHighlighted += OnValidMovesHighlighted;
            Piece_Controller.instance.OnValidMovesDeHighlighted += OnValidMovesDeHighlighted;
            Piece_Controller.instance.OnContestStarted += OnContestStarting;
        }
        else {
            Debug.LogWarning("Spawn Order Error, NetworkController can not find local player");
        }
    }
    void OnGameClientJoined(object sender, EventArgs e)
    {
        if (Piece_Controller.instance != null) {
            Piece_Controller.instance.OnPieceTransformed += OnPieceTransformed;
            Piece_Controller.instance.OnValidMovesHighlighted += OnValidMovesHighlighted;
            Piece_Controller.instance.OnValidMovesDeHighlighted += OnValidMovesDeHighlighted;
        }
        else {
            Debug.LogWarning("Spawn Order Error, NetworkController can not find local player");
        }
    }
    //Need event for contest starting so we can popup window for both players
    //Then Need an event for listening for input from either players
    void OnContestStarting(object sender, Piece_Controller.EventArgsOnContestStarted e)
    {
        Debug.Log("NETWORK_EVENT: Contest Event Detected");
        blackPlayerDeclined = false;
        whitePlayerDeclined = false;

    }
    //Transforming Pieces and Highlighted Squares
    void OnPieceTransformed(object sender, Piece_Controller.EventArgsOnPieceTransformed e)
    {
        Debug.Log("NETWORK_EVENT: TransformedEvent Detected");

    }

    void OnValidMovesHighlighted(object sender, Piece_Controller.EventArgsOnValidMovesHighlighted e)
    {
        Debug.Log("NETWORK_EVENT: MovesHighlighted Detected");
    }
    //Removing Highlights from Squares
    void OnValidMovesDeHighlighted(object sender, Piece_Controller.EventArgsOnValidMovesDeHighlighted e)
    {
        Debug.Log("NETWORK_EVENT: MovesDeHighlighted Detected");

    }
    //EndOfTurn Reset the Pieces
    void OnEndTurn()
    {
        Debug.Log("NETWORK_EVENT: EndTurn Detected");
    }
    //Detect when declined for each player
    void OnDeclinedPressed(object sender, EventArgs e)
    {
        Debug.Log("NETWORK_EVENT: DeclinedPressed Detected");

    }
    //Someone decides to reroll
    void OnContestButtonPressed(object sender, EventArgs e)
    {
        Debug.Log("NETWORK_EVENT: ContestButtonPressed Detected");

    }

    //Sync when a piece is removed
    void OnPieceRemovedFromBoard(object sender, Board_Data.EventArgsPieceRemoved e)
    {
        Debug.Log("NETWORK_EVENT: PieceRemovedFromBoard Detected");
        //Board Data already updated on the client side when this is called

        //TODO: Check which player called this to make sure its real

        PieceRemovedServerRPC();

    }

    //Sync when a piece is damaged
    void OnPieceDamaged(object sender, Board_Data.EventArgsPieceDamaged e)
    {
        Debug.Log("NETWORK_EVENT: OnPieceDamaged Detected");
        //TODO: Check which player called this to make sure its real
        PieceDamagedServerRPC();
    }

    //Sync when a piece is moved
    void OnPieceMoved(object sender, Board_Data.EventArgsPieceMoved e)
    {
        Debug.Log("NETWORK_EVENT: OnPieceMoved Detected");

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


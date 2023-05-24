using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.Netcode;
using UnityEngine;

public class Multiplayer_Manager : NetworkBehaviour
{
    //There will be only 1 network_controller in the game
    public static Multiplayer_Manager instance;

    public NetworkVariable<Piece.Color> turnColor;

    public bool isMultiplayerGame = true;
    
    NetworkVariable<bool> whitePlayerDeclined;
    NetworkVariable<bool> blackPlayerDeclined;
    NetworkVariable<int> coinFlip;

    //Contest Variables
    const int CAPTURE = 0;
    const int DAMAGE = 1;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        //Only Listen and set up networkvariables if its a multiplayer game.
        if(isMultiplayerGame == false) { return; }

        NetworkVariable<Piece.Color> turnColor =
            new NetworkVariable<Piece.Color>(Piece.Color.white);
        whitePlayerDeclined = new NetworkVariable<bool>();
        whitePlayerDeclined.Value = false;
        blackPlayerDeclined = new NetworkVariable<bool>();
        blackPlayerDeclined.Value = false;

        coinFlip = new NetworkVariable<int>();
        coinFlip.Value = CAPTURE;

        //Detecting Game being hosted and clinet joining
        Multiplater_UI.OnGameHosted += OnGameHosted;
        Multiplater_UI.OnGameClientJoined += OnGameClientJoined;

        //Piece and Board Display Events
        PieceDisplay_Manager.instance.OnPieceTransformed += OnPieceTransformed;
        BoardDisplay_Manager.instance.OnValidMovesHighlighted += OnHighlightValidMoves;
        BoardDisplay_Manager.instance.OnValidMovesDeHighlighted += OnValidMovesDeHighlighted;

        //Calls from changes to pieces
        Board.instance.OnPieceRemovedFromBoard += OnPieceRemovedFromBoard;
        Board.instance.OnPieceDamaged += OnPieceDamaged;
        Board.instance.OnPieceMoved += OnPieceMoved;

        //Calls from inputcontroller
        //TODO:Need to check or send information about who is clicking the buttons.
        Input_Manager.instance.OnContestButtonClicked += OnContestButtonPressed;
        Input_Manager.instance.OnDeclineButtonClicked += OnDeclinedPressed;

        //TODO: Implement EndofTurn event and turns in general for this event
    }

    #region Game Network SetUp
    void OnGameHosted(object sender, EventArgs e)
    {
        //White goes first
        turnColor.Value = Piece.Color.white;
        //Set Host to white
        Turn_Manager.instance.SetPlayerColor(Piece.Color.white);
        Turn_Manager.instance.StartTurn();

        SubscribeToPieceControllerEvents();

    }
    void OnGameClientJoined(object sender, EventArgs e)
    {
        SubscribeToPieceControllerEvents();
    }

    void SubscribeToPieceControllerEvents()
    {
        if (Turn_Manager.instance != null) {
            Turn_Manager.instance.OnContestStarted += OnContestStarting;
            Turn_Manager.instance.OnEndOfTurn += OnEndTurn;
        }
        else {
            Debug.LogWarning("Spawn Order Error, NetworkController can not find local player");
        }
    }

    #endregion

    #region Contest
    //Need event for contest starting so we can popup window for both players
    //Then Need an event for listening for input from either players
    void OnContestStarting(object sender, EventArgs e)
    {
        Debug_Manager.instance.Log(Debug_Manager.type.NetworkEvents, "NETWORK_EVENT: Contest Event Detected");
        //blackPlayerDeclined = false;
        //whitePlayerDeclined = false;
        NewContestServerRPC();
    }

    [ServerRpc(RequireOwnership = false)]
    void NewContestServerRPC()
    {
        coinFlip.Value = UnityEngine.Random.Range(0, 2);
        NewContestClientRPC();
    }

    [ClientRpc]
    void NewContestClientRPC()
    {
        Debug_Manager.instance.Log(Debug_Manager.type.NetworkEvents, "CLIENTRPC: New Contest Started");
        Input_Manager.instance.UpdateDisplayBasedOnCoinFlip(coinFlip.Value);
    }
    //Someone decides to reroll
    void OnContestButtonPressed(object sender, Input_Manager.EventArgsOnContestButtonClicked e)
    {
        Debug_Manager.instance.Log(Debug_Manager.type.NetworkEvents, "NETWORK_EVENT: ContestButtonPressed Detected");
        ContestedServerRPC(e.colorOfPresser);
    }

    [ServerRpc(RequireOwnership = false)]
    void ContestedServerRPC(Piece.Color colorOfSender)
    {
        coinFlip.Value = UnityEngine.Random.Range(0, 2);
        ContestedClientRPC(colorOfSender);
    }

    [ClientRpc]
    void ContestedClientRPC(Piece.Color colorOfSender)
    {
        Debug_Manager.instance.Log(Debug_Manager.type.NetworkEvents, "CLIENTRPC: New Contest Started");


        Input_Manager.instance.UpdateDisplayBasedOnCoinFlip(coinFlip.Value);

        //Check if contest should end
        if(Token_Manager.instance.WhitePlayerHasTokens() == false &&
           Token_Manager.instance.BlackPlayerHasTokens() == false) {
            //Simulate both pressing decline button
            //TODO: Children not adding up correctly so this doesnt run
            DeclinePressedServerRpc(Turn_Manager.instance.GetPlayerColor());
        }
    }

    void OnDeclinedPressed(object sender, EventArgs e)
    {
        Debug_Manager.instance.Log(Debug_Manager.type.NetworkEvents, "NETWORK_EVENT: DeclinedPressed Detected");
        DeclinePressedServerRpc(Turn_Manager.instance.GetPlayerColor());
    }

    [ServerRpc(RequireOwnership = false)]
    void DeclinePressedServerRpc(Piece.Color colorOfSender)
    {
        Debug_Manager.instance.Log(Debug_Manager.type.NetworkEvents, "CLIENTRPC: Decline presserd by " + colorOfSender);
        if (colorOfSender == Piece.Color.white) {
            whitePlayerDeclined.Value = true;
        }
        if(colorOfSender == Piece.Color.black) {
            blackPlayerDeclined.Value = true;
        }

        if(whitePlayerDeclined.Value && blackPlayerDeclined.Value) {
            //EndContest
            whitePlayerDeclined.Value = false;
            blackPlayerDeclined.Value = false;
            EndContestClientRpc(coinFlip.Value);
        }
    }

    [ClientRpc]
    void EndContestClientRpc(int coinFlip)
    {
        //Piece_Controller.instance
        Turn_Manager.instance.EndContest(coinFlip);
    }

    #endregion

    #region GraphicsUpdates
        //Transforming Pieces and Highlighted Squares
    void OnPieceTransformed(object sender, PieceDisplay_Manager.EventArgsOnPieceTransformed e)
    {
        Debug_Manager.instance.Log(Debug_Manager.type.NetworkEvents, "NETWORK_EVENT: TransformedEvent Detected");
        Debug_Manager.instance.Log(Debug_Manager.type.NetworkEvents, "TURN COLOR NETVARIABLE: " + turnColor.Value);
        Debug_Manager.instance.Log(Debug_Manager.type.NetworkEvents, "TURN COLOR PIECECONTROLLER: " + Turn_Manager.instance.GetPlayerColor());

        if (turnColor.Value == Turn_Manager.instance.GetPlayerColor()) {
            //If a piece was transformed on your turn you need to tell the other player
            PieceTransformedServerRpc(e.Piece.positionOnBoard.x, e.Piece.positionOnBoard.y, Turn_Manager.instance.GetPlayerColor());
        }
    }
    [ServerRpc(RequireOwnership = false)]
    void PieceTransformedServerRpc(int pieceXPos, int pieceYPos, Piece.Color colorCallingUpdate){
        PieceTransformedClientRpc(pieceXPos, pieceYPos, colorCallingUpdate);
    }
    [ClientRpc]
    void PieceTransformedClientRpc(int pieceXPos, int pieceYpos, Piece.Color colorCallingUpdate)
    {
        Debug_Manager.instance.Log(Debug_Manager.type.NetworkEvents, "CLIENTRPC: Piece Transformed");
        if(colorCallingUpdate != Turn_Manager.instance.GetPlayerColor()) {
            //Piece_Controller.instance.TransformSelectedPiece();
            PieceDisplay_Manager.instance.TransformSelectedPiece(Board.instance.boardPieces[new Vector2Int(pieceXPos, pieceYpos)]);
        }
    }
    void OnHighlightValidMoves(object sender, BoardDisplay_Manager.EventArgsOnValidMovesHighlighted e)
    {
        Debug_Manager.instance.Log(Debug_Manager.type.NetworkEvents, "NETWORK_EVENT: MovesHighlighted Detected");
        if(turnColor.Value == Turn_Manager.instance.GetPlayerColor()) {
            HighlightValidMovesServerRpc(e.validMoves, Turn_Manager.instance.GetPlayerColor());
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void HighlightValidMovesServerRpc(Vector2Int[] validMoves, Piece.Color colorOfSender)
    {
        HighlightValidMovesClientRpc(validMoves, colorOfSender);
    }

    [ClientRpc]
    void HighlightValidMovesClientRpc(Vector2Int[] validMoves, Piece.Color colorOfSender)
    {
        Debug_Manager.instance.Log(Debug_Manager.type.NetworkEvents, "CLIENTRPC: Valid Moves Highlighted");
        if(Turn_Manager.instance.GetPlayerColor() != colorOfSender) {
            List<Vector2Int> validMovesList = new List<Vector2Int>(validMoves);
            BoardDisplay_Manager.instance.HighLightPossibleMoves(validMovesList);
        }
    }

    //Removing Highlights from Squares
    void OnValidMovesDeHighlighted(object sender, BoardDisplay_Manager.EventArgsOnValidMovesDeHighlighted e)
    {
        Debug_Manager.instance.Log(Debug_Manager.type.NetworkEvents, "NETWORK_EVENT: MovesDeHighlighted Detected");
        if(turnColor.Value == Turn_Manager.instance.GetPlayerColor()) {
            RemoveHighlightValidMovesServerRpc(e.validMoves, Turn_Manager.instance.GetPlayerColor());
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void RemoveHighlightValidMovesServerRpc(Vector2Int[] validMoves, Piece.Color colorOfSender)
    {
        RemoveHighlightValidMovesClientRpc(validMoves, colorOfSender);
    }

    [ClientRpc]
    void RemoveHighlightValidMovesClientRpc(Vector2Int[] validMoves, Piece.Color colorOfSender)
    {
        if(colorOfSender != Turn_Manager.instance.GetPlayerColor()) {
            List<Vector2Int> validMovesList = new List<Vector2Int>(validMoves);
            BoardDisplay_Manager.instance.RemoveHighLightPossibleMoves(validMovesList);
        }
    }

    #endregion

    #region End of Turn
    //EndOfTurn Reset the Pieces
    void OnEndTurn(object sender, EventArgs e)
    {
        Debug_Manager.instance.Log(Debug_Manager.type.NetworkEvents, "NETWORK_EVENT: EndTurn Detected");
        EndTurnServerRPC();
    }

    [ServerRpc(RequireOwnership = false)]
    void EndTurnServerRPC()
    {
        EndTurnClientRPC(turnColor.Value);
        if (turnColor.Value == Piece.Color.white) {
            turnColor.Value = Piece.Color.black;
        }
        else {
            turnColor.Value = Piece.Color.white;
        }
    }
    [ClientRpc]
    void EndTurnClientRPC(Piece.Color colorWhosTurnIsEnding)
    {
        Debug_Manager.instance.Log(Debug_Manager.type.NetworkEvents, "CLIENT RPC: End Turn");
        Debug_Manager.instance.Log(Debug_Manager.type.NetworkEvents, "EndingColor: " + colorWhosTurnIsEnding.ToString());
        Debug_Manager.instance.Log(Debug_Manager.type.NetworkEvents, "PlayerColor: " + Turn_Manager.instance.GetPlayerColor().ToString());

        //This is called on all clients. 
        if (colorWhosTurnIsEnding == Piece.Color.white) {
            if (Turn_Manager.instance.GetPlayerColor() == Piece.Color.black) {
                //Start turn
                Turn_Manager.instance.StartTurn();
            }
        }
        else {
            if (Turn_Manager.instance.GetPlayerColor() == Piece.Color.white) {
                //Start turn
                Turn_Manager.instance.StartTurn();
            }
        }
    }
    #endregion

    #region MovingPieces and Updating the Board
    //Sync when a piece is removed
    void OnPieceRemovedFromBoard(object sender, Board.EventArgsPieceRemoved e)
    {
        Debug_Manager.instance.Log(Debug_Manager.type.NetworkEvents, "NETWORK_EVENT: PieceRemovedFromBoard Detected");
        //Board Data already updated on the client side when this is called
        if (turnColor.Value == Turn_Manager.instance.GetPlayerColor()) {
            //Only update other players on your turn
            PieceRemovedServerRPC(e.removedPiece.positionOnBoard.x, e.removedPiece.positionOnBoard.y, Turn_Manager.instance.GetPlayerColor());
        }
    }

    [ServerRpc(RequireOwnership = false)]//Can be called from any client
    void PieceRemovedServerRPC(int pieceRemovedXBoardLocation, int pieceRemoveYBoardLocation, Piece.Color colorCallingTheUpdate)
    {
        PieceRemovedClientRPC(pieceRemovedXBoardLocation, pieceRemoveYBoardLocation, colorCallingTheUpdate);
    }

    [ClientRpc]
    void PieceRemovedClientRPC(int pieceRemovedXBoardLocation, int pieceRemoveYBoardLocation, Piece.Color colorCallingTheUpdate)
    {
        Debug_Manager.instance.Log(Debug_Manager.type.NetworkEvents, "CLIENT RPC: PieceRemovedClientRPC called");
        if(Turn_Manager.instance.GetPlayerColor() != colorCallingTheUpdate) {
            //if it is not your turn you need to be updated of your oponents move
            Board.instance.RemovePiece(new Vector2Int(pieceRemovedXBoardLocation, pieceRemoveYBoardLocation));
        }
        PieceDisplay_Manager.instance.UpdatePieces();
    }

    //Sync when a piece is damaged
    void OnPieceDamaged(object sender, Board.EventArgsPieceDamaged e)
    {
        Debug_Manager.instance.Log(Debug_Manager.type.NetworkEvents, "NETWORK_EVENT: OnPieceDamaged Detected");
        if (turnColor.Value == Turn_Manager.instance.GetPlayerColor()) {
            //Only update other players on your turn
            PieceDamagedServerRPC(e.damagedPiece.positionOnBoard.x, e.damagedPiece.positionOnBoard.y, Turn_Manager.instance.GetPlayerColor());
        }
    }

    [ServerRpc(RequireOwnership = false)]//Can be called from any client
    void PieceDamagedServerRPC(int pieceDamagedXBoardLocation, int pieceDamagedYBoardLocation, Piece.Color colorCallingTheUpdate)
    {
        PieceDamagedClientRPC(pieceDamagedXBoardLocation, pieceDamagedYBoardLocation, colorCallingTheUpdate);
    }

    [ClientRpc]
    void PieceDamagedClientRPC(int pieceDamagedXBoardLocation, int pieceDamagedYBoardLocation, Piece.Color colorCallingTheUpdate)
    {
        Debug_Manager.instance.Log(Debug_Manager.type.NetworkEvents, "CLIENTRPC: Piece Damaged Detected");
        if (Turn_Manager.instance.GetPlayerColor() != colorCallingTheUpdate) {
            //if it is not your turn you need to be updated of your oponents move
            Board.instance.DamagePiece(new Vector2Int(pieceDamagedXBoardLocation, pieceDamagedYBoardLocation));
        }
        PieceDisplay_Manager.instance.UpdatePieces();
    }
    //Sync when a piece is moved
    void OnPieceMoved(object sender, Board.EventArgsPieceMoved e)
    {
        Debug_Manager.instance.Log(Debug_Manager.type.NetworkEvents, "NETWORK_EVENT: OnPieceMoved Detected");
        if (turnColor.Value == Turn_Manager.instance.GetPlayerColor()) {
            //Only update other players on your turn
            PieceMovedServerRPC(e.pieceStartBoardCordintaes.x, e.pieceStartBoardCordintaes.y,
                                  e.pieceEndBoardCordintaes.x, e.pieceEndBoardCordintaes.y,
                                  Turn_Manager.instance.GetPlayerColor());
        }
    }

    [ServerRpc(RequireOwnership = false)]//Can be called from any client
    void PieceMovedServerRPC(int oldXPos, int oldYPos, int newXPos, int newYPos, Piece.Color colorCallingTheUpdate)
    {
        PieceMovedClientRPC( oldXPos,  oldYPos,  newXPos,  newYPos, colorCallingTheUpdate);
    }

    [ClientRpc]
    void PieceMovedClientRPC(int oldXPos, int oldYPos, int newXPos, int newYPos, Piece.Color colorCallingTheUpdate)
    {
        Debug_Manager.instance.Log(Debug_Manager.type.NetworkEvents, "CLIENTRPC: Piece Moved Detected");
        if (Turn_Manager.instance.GetPlayerColor() != colorCallingTheUpdate) {
            //if it is not your turn you need to be updated of your oponents move
            Board.instance.MovePieceNetworkCall(new Vector2Int(oldXPos, oldYPos), new Vector2Int(newXPos, newYPos));
        }
        PieceDisplay_Manager.instance.UpdatePieces();
    }
    #endregion
}



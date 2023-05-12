using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.Netcode;
using UnityEngine;

public class Network_Controller : NetworkBehaviour
{
    //There will be only 1 network_controller in the game
    public static Network_Controller instance;

    public NetworkVariable<Piece_Data.Color> turnColor;

    
    NetworkVariable<bool> whitePlayerDeclined;
    NetworkVariable<bool> blackPlayerDeclined;

    //Contest Variables
    const int CAPTURE = 0;
    const int DAMAGE = 1;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        NetworkVariable<Piece_Data.Color> turnColor =
            new NetworkVariable<Piece_Data.Color>(Piece_Data.Color.white);
        whitePlayerDeclined = new NetworkVariable<bool>();
        whitePlayerDeclined.Value = false;
        blackPlayerDeclined = new NetworkVariable<bool>();
        blackPlayerDeclined.Value = false;

        //Detecting Game being hosted and clinet joining
        Multiplater_UI.OnGameHosted += OnGameHosted;
        Multiplater_UI.OnGameClientJoined += OnGameClientJoined;

        //Piece and Board Display Events
        Piece_Display.instance.OnPieceTransformed += OnPieceTransformed;
        Board_Display.instance.OnValidMovesHighlighted += OnHighlightValidMoves;
        Board_Display.instance.OnValidMovesDeHighlighted += OnValidMovesDeHighlighted;

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

    #region Game Network SetUp
    void OnGameHosted(object sender, EventArgs e)
    {
        //White goes first
        turnColor.Value = Piece_Data.Color.white;
        //Set Host to white
        Piece_Controller.instance.color = Piece_Data.Color.white;
        Piece_Controller.instance.StartTurn();
        SubscribeToPieceControllerEvents();

    }
    void OnGameClientJoined(object sender, EventArgs e)
    {
        SubscribeToPieceControllerEvents();
    }

    void SubscribeToPieceControllerEvents()
    {
        if (Piece_Controller.instance != null) {
            Piece_Controller.instance.OnContestStarted += OnContestStarting;
            Piece_Controller.instance.OnEndOfTurn += OnEndTurn;
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
        Debug.Log("NETWORK_EVENT: Contest Event Detected");
        //blackPlayerDeclined = false;
        //whitePlayerDeclined = false;
        NewContestServerRPC();
    }

    [ServerRpc(RequireOwnership = false)]
    void NewContestServerRPC()
    {
        int coinFlip = UnityEngine.Random.Range(0, 2);
        NewContestClientRPC(coinFlip);
    }

    [ClientRpc]
    void NewContestClientRPC(int coinFlip)
    {
        Debug.Log("CLIENTRPC: New Contest Started");
        UpdateDisplayBasedOnCoinFlip(coinFlip);
    }
    //Someone decides to reroll
    void OnContestButtonPressed(object sender, Input_Controller.EventArgsOnContestButtonClicked e)
    {
        Debug.Log("NETWORK_EVENT: ContestButtonPressed Detected");

        ContestedServerRPC(e.colorOfPresser);
    }

    [ServerRpc(RequireOwnership = false)]
    void ContestedServerRPC(Piece_Data.Color colorOfSender)
    {
        int coinFlip = UnityEngine.Random.Range(0, 2);
        ContestedClientRPC(coinFlip, colorOfSender);
    }

    [ClientRpc]
    void ContestedClientRPC(int coinFlip, Piece_Data.Color colorOfSender)
    {
        Debug.Log("CLIENTRPC: New Contest Started");

        if(colorOfSender == Piece_Data.Color.white) {
            Destroy( Input_Controller.instance.whiteButtonHolder.transform.GetChild(0).gameObject);
        }
        else {
            Destroy( Input_Controller.instance.blackButtonHolder.transform.GetChild(0).gameObject);
        }
        UpdateDisplayBasedOnCoinFlip(coinFlip);

        //Check if contest should end
        if( Input_Controller.instance.whiteButtonHolder.transform.childCount +
            Input_Controller.instance.blackButtonHolder.transform.childCount == 0){
            //Simulate both pressing decline button
            //TODO: Children not adding up correctly so this doesnt run
            DeclinePressedServerRpc(Piece_Controller.instance.color);
        }
    }

    void UpdateDisplayBasedOnCoinFlip(int coinFlip) 
    {
        Input_Controller.instance.contestHolder.SetActive(true);
        if (coinFlip == CAPTURE) {
            Input_Controller.instance.contestText.text = "Piece Capture";
        }
        else {
            Input_Controller.instance.contestText.text = "Piece Damage";
        }

    }

    void OnDeclinedPressed(object sender, EventArgs e)
    {
        Debug.Log("NETWORK_EVENT: DeclinedPressed Detected");
        DeclinePressedServerRpc(Piece_Controller.instance.color);
    }

    [ServerRpc(RequireOwnership = false)]
    void DeclinePressedServerRpc(Piece_Data.Color colorOfSender)
    {
        Debug.Log("CLIENTRPC: Decline presserd by " + colorOfSender);
        if (colorOfSender == Piece_Data.Color.white) {
            whitePlayerDeclined.Value = true;
        }
        if(colorOfSender == Piece_Data.Color.black) {
            blackPlayerDeclined.Value = true;
        }

        if(whitePlayerDeclined.Value && blackPlayerDeclined.Value) {
            //EndContest
            whitePlayerDeclined.Value = false;
            blackPlayerDeclined.Value = false;
            EndContestClientRpc();
        }
    }

    [ClientRpc]
    void EndContestClientRpc()
    {
        //Piece_Controller.instance
        Piece_Controller.instance.EndContest();
    }

    #endregion

    #region GraphicsUpdates
        //Transforming Pieces and Highlighted Squares
    void OnPieceTransformed(object sender, Piece_Display.EventArgsOnPieceTransformed e)
    {
        Debug.Log("NETWORK_EVENT: TransformedEvent Detected");
        if (turnColor.Value == Piece_Controller.instance.color) {
            //If a piece was transformed on your turn you need to tell the other player
            PieceTransformedServerRpc(e.Piece.positionOnBoard.x, e.Piece.positionOnBoard.y, e.chessMoveIndex, Piece_Controller.instance.color);

        }
    }
    [ServerRpc(RequireOwnership = false)]
    void PieceTransformedServerRpc(int pieceXPos, int pieceYPos, int chessMoveIndex, Piece_Data.Color colorCallingUpdate){
        PieceTransformedClientRpc(pieceXPos, pieceYPos, chessMoveIndex, colorCallingUpdate);
    }
    [ClientRpc]
    void PieceTransformedClientRpc(int pieceXPos, int pieceYpos, int chessMoveIndex, Piece_Data.Color colorCallingUpdate)
    {
        Debug.Log("CLIENTRPC: Piece Transformed");
        if(colorCallingUpdate != Piece_Controller.instance.color) {
            //Piece_Controller.instance.TransformSelectedPiece();
            Piece_Display.instance.TransformSelectedPiece(Board_Data.instance.boardPieces[pieceXPos, pieceYpos], chessMoveIndex);
        }
    }
    void OnHighlightValidMoves(object sender, Board_Display.EventArgsOnValidMovesHighlighted e)
    {
        Debug.Log("NETWORK_EVENT: MovesHighlighted Detected");
        if(turnColor.Value == Piece_Controller.instance.color) {
            HighlightValidMovesServerRpc(e.validMoves, Piece_Controller.instance.color);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void HighlightValidMovesServerRpc(Vector2Int[] validMoves, Piece_Data.Color colorOfSender)
    {
        HighlightValidMovesClientRpc(validMoves, colorOfSender);
    }

    [ClientRpc]
    void HighlightValidMovesClientRpc(Vector2Int[] validMoves, Piece_Data.Color colorOfSender)
    {
        Debug.Log("CLIENTRPC: Valid Moves Highlighted");
        if(Piece_Controller.instance.color != colorOfSender) {
            List<Vector2Int> validMovesList = new List<Vector2Int>(validMoves);
            Board_Display.instance.HighLightPossibleMoves(validMovesList);
        }
    }

    //Removing Highlights from Squares
    void OnValidMovesDeHighlighted(object sender, Board_Display.EventArgsOnValidMovesDeHighlighted e)
    {
        Debug.Log("NETWORK_EVENT: MovesDeHighlighted Detected");
        if(turnColor.Value == Piece_Controller.instance.color) {
            RemoveHighlightValidMovesServerRpc(e.validMoves, Piece_Controller.instance.color);
        }

    }

    [ServerRpc(RequireOwnership = false)]
    void RemoveHighlightValidMovesServerRpc(Vector2Int[] validMoves, Piece_Data.Color colorOfSender)
    {
        RemoveHighlightValidMovesClientRpc(validMoves, colorOfSender);
    }

    [ClientRpc]
    void RemoveHighlightValidMovesClientRpc(Vector2Int[] validMoves, Piece_Data.Color colorOfSender)
    {
        if(colorOfSender != Piece_Controller.instance.color) {
            List<Vector2Int> validMovesList = new List<Vector2Int>(validMoves);
            Board_Display.instance.RemoveHighLightPossibleMoves(validMovesList);
        }
    }

    #endregion

    #region End of Turn
    //EndOfTurn Reset the Pieces
    void OnEndTurn(object sender, EventArgs e)
    {
        Debug.Log("NETWORK_EVENT: EndTurn Detected");
        EndTurnServerRPC();
    }

    [ServerRpc(RequireOwnership = false)]
    void EndTurnServerRPC()
    {
        EndTurnClientRPC(turnColor.Value);
        if (turnColor.Value == Piece_Data.Color.white) {
            turnColor.Value = Piece_Data.Color.black;
        }
        else {
            turnColor.Value = Piece_Data.Color.white;
        }
    }
    [ClientRpc]
    void EndTurnClientRPC(Piece_Data.Color colorWhosTurnIsEnding)
    {
        Debug.Log("CLIENT RPC: End Turn");
        //This is called on all clients. 
        if (colorWhosTurnIsEnding == Piece_Data.Color.white) {
            if (Piece_Controller.instance.color == Piece_Data.Color.black) {
                //Start turn
                Piece_Controller.instance.StartTurn();
            }
        }
        else {
            if (Piece_Controller.instance.color == Piece_Data.Color.white) {
                //Start turn
                Piece_Controller.instance.StartTurn();
            }
        }
    }
    #endregion

    #region MovingPieces and Updating the Board
    //Sync when a piece is removed
    void OnPieceRemovedFromBoard(object sender, Board_Data.EventArgsPieceRemoved e)
    {
        Debug.Log("NETWORK_EVENT: PieceRemovedFromBoard Detected");
        //Board Data already updated on the client side when this is called
        if (turnColor.Value == Piece_Controller.instance.color) {
            //Only update other players on your turn
            PieceRemovedServerRPC(e.removedPiece.positionOnBoard.x, e.removedPiece.positionOnBoard.y, Piece_Controller.instance.color);
        }
    }

    [ServerRpc(RequireOwnership = false)]//Can be called from any client
    void PieceRemovedServerRPC(int pieceRemovedXBoardLocation, int pieceRemoveYBoardLocation, Piece_Data.Color colorCallingTheUpdate)
    {
        PieceRemovedClientRPC(pieceRemovedXBoardLocation, pieceRemoveYBoardLocation, colorCallingTheUpdate);
    }

    [ClientRpc]
    void PieceRemovedClientRPC(int pieceRemovedXBoardLocation, int pieceRemoveYBoardLocation, Piece_Data.Color colorCallingTheUpdate)
    {
        Debug.Log("CLIENT RPC: PieceRemovedClientRPC called");
        if(Piece_Controller.instance.color != colorCallingTheUpdate) {
            //if it is not your turn you need to be updated of your oponents move
            Board_Data.instance.RemovePiece(pieceRemovedXBoardLocation, pieceRemoveYBoardLocation);
        }
        Piece_Display.instance.UpdatePieces();
    }

    //Sync when a piece is damaged
    void OnPieceDamaged(object sender, Board_Data.EventArgsPieceDamaged e)
    {
        Debug.Log("NETWORK_EVENT: OnPieceDamaged Detected");
        if (turnColor.Value == Piece_Controller.instance.color) {
            //Only update other players on your turn
            PieceDamagedServerRPC(e.damagedPiece.positionOnBoard.x, e.damagedPiece.positionOnBoard.y, Piece_Controller.instance.color);
        }
    }

    [ServerRpc(RequireOwnership = false)]//Can be called from any client
    void PieceDamagedServerRPC(int pieceDamagedXBoardLocation, int pieceDamagedYBoardLocation, Piece_Data.Color colorCallingTheUpdate)
    {
        PieceDamagedClientRPC(pieceDamagedXBoardLocation, pieceDamagedYBoardLocation, colorCallingTheUpdate);
    }

    [ClientRpc]
    void PieceDamagedClientRPC(int pieceDamagedXBoardLocation, int pieceDamagedYBoardLocation, Piece_Data.Color colorCallingTheUpdate)
    {
        Debug.Log("CLIENTRPC: Piece Damaged Detected");
        if (Piece_Controller.instance.color != colorCallingTheUpdate) {
            //if it is not your turn you need to be updated of your oponents move
            Board_Data.instance.DamagePiece(pieceDamagedXBoardLocation, pieceDamagedYBoardLocation);
        }
        Piece_Display.instance.UpdatePieces();
    }
    //Sync when a piece is moved
    void OnPieceMoved(object sender, Board_Data.EventArgsPieceMoved e)
    {
        Debug.Log("NETWORK_EVENT: OnPieceMoved Detected");
        if (turnColor.Value == Piece_Controller.instance.color) {
            //Only update other players on your turn
            PieceMovedServerRPC(e.pieceStartBoardCordintaes.x, e.pieceStartBoardCordintaes.y,
                                  e.pieceEndBoardCordintaes.x, e.pieceEndBoardCordintaes.y,
                                  Piece_Controller.instance.color);
        }
    }

    [ServerRpc(RequireOwnership = false)]//Can be called from any client
    void PieceMovedServerRPC(int oldXPos, int oldYPos, int newXPos, int newYPos, Piece_Data.Color colorCallingTheUpdate)
    {
        PieceMovedClientRPC( oldXPos,  oldYPos,  newXPos,  newYPos, colorCallingTheUpdate);
    }

    [ClientRpc]
    void PieceMovedClientRPC(int oldXPos, int oldYPos, int newXPos, int newYPos, Piece_Data.Color colorCallingTheUpdate)
    {
        Debug.Log("CLIENTRPC: Piece Moved Detected");
        if (Piece_Controller.instance.color != colorCallingTheUpdate) {
            //if it is not your turn you need to be updated of your oponents move
            Board_Data.instance.MovePieceNetworkCall(oldXPos, oldYPos, newXPos, newYPos);
        }
        Piece_Display.instance.UpdatePieces();
    }

    #endregion
}



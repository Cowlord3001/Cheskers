using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Turn_Manager : NetworkBehaviour
{
    //Graphics
    [SerializeField] GameObject highLightGraphicPrefab;
    GameObject highLightGraphic;

    //PRIVATE DATA
    Piece selectedPiece;
    Piece previouslySelectedPiece;
    public Piece SelectedPiece { get { return selectedPiece; } set { selectedPiece = value; } }
    public Piece PreviouslySelectedPiece { get { return previouslySelectedPiece; } set {  previouslySelectedPiece = value; } }

    List<Vector2Int> validMoves;
    public List<Vector2Int> ValidMoves { get { return validMoves; } set { validMoves = value; } }

    Vector2Int decidedMove;
    public Vector2Int DecidedMove { get { return decidedMove; } set { decidedMove = value; } }

    bool rerolled = false;
    public bool Rerolled { set { rerolled = value; } get { return rerolled; } }

    //PUBLIC DATA
    public static Turn_Manager instance;
    //Host sets themself to white so default color is black
    Piece.Color color = Piece.Color.black;
    public Piece.Color GetColor() { return color; }

    public Piece.Color GetPlayerColor() { return color; }
    public void SetPlayerColor(Piece.Color newColor) { color = newColor; }
    public enum PhaseInTurn
    {
        PIECE_SELECTION,
        PIECE_CONFIRMATION,
        DECIDE_MOVE_OR_REROLL,
        CONTEST,
        MOVE_AND_UPDATE,
        END_OF_TURN,
        WAITING_FOR_TURN
    }

    #region TurnStates
    PlayerTurnState pieceSelection;
    PlayerTurnState pieceConfirmation;
    PlayerTurnState decideMoveOrReroll;
    PlayerTurnState contest;
    PlayerTurnState moveAndUpdate;
    PlayerTurnState endOfTurn;

    private void Awake()
    {
        pieceSelection = new State_PieceSelection(this);
        pieceConfirmation = new State_PieceConfirmation(this);
        decideMoveOrReroll = new State_DecideMoveOrReroll(this);
        contest = new State_Contest(this);
        moveAndUpdate = new State_MoveAndUpdate(this);
        endOfTurn = new State_EndOfTurn(this);
    }

    public void SwapState(PhaseInTurn phaseInTurn, PlayerTurnState newState)
    {
        switch (phaseInTurn) {
            case PhaseInTurn.PIECE_SELECTION:
                pieceSelection = newState;
                break;
            case PhaseInTurn.PIECE_CONFIRMATION:
                pieceConfirmation = newState;
                break;
            case PhaseInTurn.DECIDE_MOVE_OR_REROLL:
                decideMoveOrReroll = newState;
                break;
            case PhaseInTurn.CONTEST:
                contest = newState;
                break;
            case PhaseInTurn.MOVE_AND_UPDATE:
                moveAndUpdate = newState;
                break;
            case PhaseInTurn.END_OF_TURN:
                endOfTurn = newState;
                break;
            case PhaseInTurn.WAITING_FOR_TURN:
                break;
            default:
                break;
        }
    }

    #endregion


    //Used by inputcontroller, maybe find a better solution
    public PhaseInTurn phaseInTurn { get; private set; }
    public void SetPhaseInTurn(PhaseInTurn phaseInTurn) { this.phaseInTurn = phaseInTurn; }

    //EVENTS
    public event EventHandler OnEndOfTurn;
    public event EventHandler OnContestStarted;
    int coinFlip;
    public int CoinFlip { get { return coinFlip; } set { coinFlip = value; } }

    //This runs from other script when it is the players turn
    public void StartTurn()
    {
        phaseInTurn = PhaseInTurn.PIECE_SELECTION;
    }
    // Start is called before the first frame update
    void Start()
    {
        if (Multiplayer_Manager.instance.isMultiplayerGame == true) {
            if (IsOwner == false) return;
            phaseInTurn = PhaseInTurn.WAITING_FOR_TURN;
        }
        else {
            //Not Multiplayer
            color = Piece.Color.white;
            phaseInTurn = PhaseInTurn.PIECE_SELECTION;
        }
        instance = this;


        Input_Manager.instance.OnLeftMouseClick += OnLeftMouseClick;
        Input_Manager.instance.OnRollAgainButtonClicked += OnRollAgainPressed;
        Input_Manager.instance.OnEndTurnButtonClicked += OnEndTurnPressed;
        //Input_Controller.instance.OnContestButtonClicked += OnContestPressed;
        //Input_Controller.instance.OnDeclineButtonClicked += OnDeclinePressed;
    }


    //Event only fires on mouseclicks on the board
    void OnLeftMouseClick(object sender, EventArgs e)
    {
        if (Multiplayer_Manager.instance.isMultiplayerGame == true) {
            if (IsOwner == false) return;
            if (Multiplayer_Manager.instance.turnColor.Value != color) { return; }
        }

        AdvanceGame();
    }
    public void AdvanceGame()
    {
        if (Multiplayer_Manager.instance.isMultiplayerGame == true) {
            if (IsOwner == false) return;
            if (Multiplayer_Manager.instance.turnColor.Value != color) { return; }
        }
        switch (phaseInTurn) {
            case PhaseInTurn.PIECE_SELECTION:                   // DONE
                Debug_Manager.instance.Log(Debug_Manager.type.PlayerCurrentState, "PIECE_SELECTION");
                PieceSelection();
                break;
            case PhaseInTurn.PIECE_CONFIRMATION:                // DONE
                Debug_Manager.instance.Log(Debug_Manager.type.PlayerCurrentState, "PIECE_CONFIRMATION");
                PieceConfirmation();
                break;
            case PhaseInTurn.DECIDE_MOVE_OR_REROLL:
                Debug_Manager.instance.Log(Debug_Manager.type.PlayerCurrentState, "DECIDE_MOVE_OR_REROLL");   // DONE
                DecideMove();
                break;
            case PhaseInTurn.CONTEST:
                Debug_Manager.instance.Log(Debug_Manager.type.PlayerCurrentState, "CONTEST");
                Contest();                  // DONE
                break;
            case PhaseInTurn.MOVE_AND_UPDATE:
                Debug_Manager.instance.Log(Debug_Manager.type.PlayerCurrentState, "MOVE_AND_UPDATE");
                //If this is being called from here it is
                //local and no contest was needed
                MoveAndUpdate();        // DONE
                break;
            case PhaseInTurn.END_OF_TURN:
                Debug_Manager.instance.Log(Debug_Manager.type.PlayerCurrentState, "END_OF_TURN");
                EndTurn();
                //For Testing turn back to start of turn
                rerolled = false;
                //phaseInTurn = PhaseInTurn.PIECE_SELECTION;
                break;
            default:
                break;
        }
    }
    public void PieceSelection(){ pieceSelection.RunState(); }
    void PieceConfirmation(){ pieceConfirmation.RunState(); }
    void DecideMove(){ decideMoveOrReroll.RunState(); }
    void Contest(){ contest.RunState(); }
    //Server may have to move piece based on contest,  public
    public void MoveAndUpdate() { moveAndUpdate.RunState(); }
    void EndTurn() { endOfTurn.RunState(); }
    public void SendEndOfTurnEvent(){ OnEndOfTurn?.Invoke(this, EventArgs.Empty); }
    public void SendContestEvent(){ OnContestStarted?.Invoke(this, EventArgs.Empty); }
    //Server may have to end contest, public
    public void EndContest(int coinFlip)
    {
        this.coinFlip = coinFlip;
        //Called by network controller to progress the game.
        Input_Manager.instance.TurnOffContestHolders();
        if (phaseInTurn == PhaseInTurn.CONTEST) {
            phaseInTurn = PhaseInTurn.MOVE_AND_UPDATE;
            AdvanceGame();
        }
    }

    void OnRollAgainPressed(object sender, EventArgs e)
    {
        if (phaseInTurn == PhaseInTurn.DECIDE_MOVE_OR_REROLL && 
           (rerolled == false || Input_Manager.developerCommandsEnabled)) {
            UpdateBoardAndPieceGraphics();
            RemoveHighlightOnSelectedPiece();

            previouslySelectedPiece = selectedPiece;
            rerolled = true;
            phaseInTurn = PhaseInTurn.PIECE_SELECTION;
        }
    }
    void OnEndTurnPressed(object sender, EventArgs e)
    {
        EndTurn();
    }


    public void UpdateBoardAndPieceGraphics()
    {
        BoardDisplay_Manager.instance.RemoveHighLightPossibleMoves(validMoves);
        PieceDisplay_Manager.instance.UpdatePieces();
        RemoveHighlightOnSelectedPiece();
    }

    public void HighLightSelectedPiece()
    {
        if (highLightGraphic != null)
        {
            Destroy(highLightGraphic);
        }
        highLightGraphic = Instantiate(highLightGraphicPrefab, 
                                           Piece_Detection.BoardIndextoWorld(selectedPiece.positionOnBoard), 
                                           Quaternion.identity);
    }
    public void RemoveHighlightOnSelectedPiece()
    {
        if (highLightGraphic != null) {
            Destroy(highLightGraphic);
        }
    }


}

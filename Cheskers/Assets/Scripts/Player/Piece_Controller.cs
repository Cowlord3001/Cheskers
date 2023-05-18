using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Piece_Controller : NetworkBehaviour
{
    //Graphics
    [SerializeField] GameObject highLightGraphicPrefab;
    GameObject highLightGraphic;

    //PRIVATE DATA
    Piece_Data selectedPiece;
    List<Vector2Int> validMoves;
    Vector2Int decidedMove;
    bool rerolled = false;

    //PUBLIC DATA
    public static Piece_Controller instance;
    //Host sets themself to white so default color is black
    public Piece_Data.Color color = Piece_Data.Color.black;
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
    //Used by inputcontroller, maybe find a better solution
    public PhaseInTurn phaseInTurn { get; private set; }

    //Contest Variables
    const int CAPTURE = 0;
    const int DAMAGE = 1;

    //EVENTS
    public event EventHandler OnEndOfTurn;
    public event EventHandler OnContestStarted;
    int coinFlip;
    private void Update()
    {

    }

    //This runs from other script when it is the players turn
    public void StartTurn()
    {
        phaseInTurn = PhaseInTurn.PIECE_SELECTION;
    }

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (Network_Controller.instance.isMultiplayerGame == true) {
            if (IsOwner == false) return;
            phaseInTurn = PhaseInTurn.WAITING_FOR_TURN;
        }
        else {
            //Not Multiplayer
            color = Piece_Data.Color.white;
            phaseInTurn = PhaseInTurn.PIECE_SELECTION;
        }
        Input_Controller.instance.OnLeftMouseClick += OnLeftMouseClick;
        Input_Controller.instance.OnRollAgainButtonClicked += OnRollAgainPressed;
        Input_Controller.instance.OnEndTurnButtonClicked += OnEndTurnPressed;
        //Input_Controller.instance.OnContestButtonClicked += OnContestPressed;
        //Input_Controller.instance.OnDeclineButtonClicked += OnDeclinePressed;
    }

    //Event only fires on mouseclicks on the board
    void OnLeftMouseClick(object sender, EventArgs e)
    {
        if (Network_Controller.instance.isMultiplayerGame == true) {
            if (IsOwner == false) return;
            if (Network_Controller.instance.turnColor.Value != color) { return; }
        }
        AdvanceGame(true);
    }
    void AdvanceGame(bool mouseClicked = false)
    {
        if (Network_Controller.instance.isMultiplayerGame == true) {
            if (IsOwner == false) return;
            if (Network_Controller.instance.turnColor.Value != color) { return; }
        }
        switch (phaseInTurn) {
            case PhaseInTurn.PIECE_SELECTION:                   // DONE
                Debug.Log("PIECE_SELECTION");
                TargetPieceToSelect();
                break;
            case PhaseInTurn.PIECE_CONFIRMATION:                // DONE
                Debug.Log("PIECE_CONFIRMATION");
                PieceSelection();
                break;
            case PhaseInTurn.DECIDE_MOVE_OR_REROLL:
                Debug.Log("DECIDE_MOVE_OR_REROLL");    // DONE
                DecideMove();
                break;
            case PhaseInTurn.CONTEST:
                Debug.Log("CONTEST");
                Contest();                  // DONE
                break;
            case PhaseInTurn.MOVE_AND_UPDATE:
                Debug.Log("MOVE_AND_UPDATE");
                //If this is being called from here it is
                //local and no contest was needed
                MovePiece();        // DONE
                break;
            case PhaseInTurn.END_OF_TURN:
                Debug.Log("END_OF_TURN");
                EndTurn();
                //For Testing turn back to start of turn
                rerolled = false;
                //phaseInTurn = PhaseInTurn.PIECE_SELECTION;
                break;
            default:
                break;
        }
    }
    void TargetPieceToSelect()
    {
        Piece_Data piece = Piece_Detection.GetPieceUnderMouse();
        if(piece != null) {
            if(piece.GetColor() != color) { return; }
            selectedPiece = piece;

            //Debug.Log(selectedPiece.gameObject.name);
            HighLightSelectedPiece();

            phaseInTurn = PhaseInTurn.PIECE_CONFIRMATION;
            //Happens on click, Game automatically advances
        }
    }
    void PieceSelection()
    {
        if (Input.GetMouseButtonDown(0)) {
            Piece_Data pieceData = Piece_Detection.GetPieceUnderMouse();
            if (pieceData != null) {
                RemoveHighlightOnSelectedPiece();

                if (selectedPiece == pieceData) {
                    //Select Piece
                    selectedPiece = pieceData;

                    //Get a random move
                    int randomChessMoveIndex = UnityEngine.Random.Range(0, Piece_Display.instance.chessMoves.Length);
                    Chess_Move_SO chessMove = Piece_Display.instance.chessMoves[randomChessMoveIndex];

                    //Generate a randomMove and update piece display
                    validMoves = Board_Data.instance.getAllLegalMoves(selectedPiece, chessMove);

                    //Update Display for board and pieces
                    Piece_Display.instance.TransformSelectedPiece(selectedPiece, randomChessMoveIndex);
                    Board_Display.instance.HighLightPossibleMoves(validMoves);

                    phaseInTurn = PhaseInTurn.DECIDE_MOVE_OR_REROLL;
                }
                else {
                    TargetPieceToSelect();
                }
            }
        }
    }
    void DecideMove()
    {
        decidedMove = Input_Controller.instance.mouseBoardPosition;
        foreach (var move in validMoves)
        {
            if(decidedMove == move)
            {
                //Starts the first contest
                phaseInTurn = PhaseInTurn.CONTEST;
                AdvanceGame();
            }
        }
    }
    void Contest()
    {
        if (Board_Data.instance.boardPieces[decidedMove.x,decidedMove.y] == null ||
            Board_Data.instance.boardPieces[decidedMove.x, decidedMove.y].health == 1)
        {
            //Skip Contest Phase
            coinFlip = UnityEngine.Random.Range(0, 2);
            phaseInTurn = PhaseInTurn.MOVE_AND_UPDATE;
            AdvanceGame();
            return;
        }

        if (Input_Controller.instance.WhitePlayerHasTokens() == false &&
            Input_Controller.instance.BlackPlayerHasTokens() == false) {
            coinFlip = UnityEngine.Random.Range(0, 2);
            phaseInTurn = PhaseInTurn.MOVE_AND_UPDATE;
            AdvanceGame();
            return;
        }

        OnContestStarted?.Invoke(this, EventArgs.Empty);
        //Contest code moved to network controller
    }
    //Server may have to end contest, public
    public void EndContest(int coinFlip)
    {
        this.coinFlip = coinFlip;
        //Called by network controller to progress the game.
        Input_Controller.instance.TurnOffContestHolders();
        if (phaseInTurn == PhaseInTurn.CONTEST) {
            phaseInTurn = PhaseInTurn.MOVE_AND_UPDATE;
            AdvanceGame();
        }
    }
    //Server may have to move piece based on contest,  public
    public void MovePiece()
    {
        if (coinFlip == CAPTURE)
        {
            Board_Data.instance.MoveAndTake(selectedPiece, decidedMove.x, decidedMove.y);
        }
        else if(coinFlip == DAMAGE)
        {
            Board_Data.instance.MoveAndDamage(selectedPiece, decidedMove.x, decidedMove.y);
        }
        else
        {
            Debug.LogWarning("Error: Invalid Coin Flip");
        }
        Debug.Log(Board_Data.instance.debugMessage);
        UpdateBoardAndPieceGraphics();
        RemoveHighlightOnSelectedPiece();
        phaseInTurn = PhaseInTurn.END_OF_TURN;
        AdvanceGame();
        
    }
    void EndTurn()
    {
        UpdateBoardAndPieceGraphics();
        rerolled = false;
        phaseInTurn = PhaseInTurn.WAITING_FOR_TURN;
        if(OnEndOfTurn == null) { 
            Debug.LogWarning("No subscribers on this instance for end turn"); 
        }
        OnEndOfTurn?.Invoke(this, EventArgs.Empty);
    }


    void OnRollAgainPressed(object sender, EventArgs e)
    {
        if (phaseInTurn == PhaseInTurn.DECIDE_MOVE_OR_REROLL && rerolled == false) {
            UpdateBoardAndPieceGraphics();
            RemoveHighlightOnSelectedPiece();

            rerolled = true;
            phaseInTurn = PhaseInTurn.PIECE_SELECTION;
        }
    }
    void OnEndTurnPressed(object sender, EventArgs e)
    {
        EndTurn();
    }
    void UpdateBoardAndPieceGraphics()
    {
        Board_Display.instance.RemoveHighLightPossibleMoves(validMoves);
        Piece_Display.instance.UpdatePieces();
        RemoveHighlightOnSelectedPiece();
    }
    void HighLightSelectedPiece()
    {
        if (highLightGraphic != null)
        {
            Destroy(highLightGraphic);
        }
        highLightGraphic = Instantiate(highLightGraphicPrefab, 
                                           selectedPiece.gameObject.transform.position, 
                                           Quaternion.identity);
    }
    void RemoveHighlightOnSelectedPiece()
    {
        if (highLightGraphic != null) {
            Destroy(highLightGraphic);
        }
    }


}

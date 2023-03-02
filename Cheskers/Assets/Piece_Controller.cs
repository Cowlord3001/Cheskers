using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Piece_Controller : NetworkBehaviour
{
    public static Piece_Controller instance;

    [SerializeField] Chess_Move_SO[] chessMoves;
    [SerializeField] GameObject highLightGraphicPrefab;
    GameObject highLightGraphic;

    Piece_Data selectedPiece;

    List<Vector2Int> validMoves;

    bool isYourTurn;
    public static Piece_Data.Color color = Piece_Data.Color.white;

    Vector2Int decidedMove;
    bool rerolled = false;
    int coinFlip;

    const int CAPTURE = 0;
    const int DAMAGE = 1;

    public event EventHandler<EventArgsOnValidMovesHighlighted> OnValidMovesHighlighted;
    public class EventArgsOnValidMovesHighlighted { public Vector2Int[] validMoves; }

    public event EventHandler<EventArgsOnValidMovesDeHighlighted> OnValidMovesDeHighlighted;
    public class EventArgsOnValidMovesDeHighlighted { public Vector2Int[] validMoves; }

    public event EventHandler<EventArgsOnPieceTransformed> OnPieceTransformed;
    public class EventArgsOnPieceTransformed { public Piece_Data Piece; public int randomMove; };

    public enum PhaseInTurn
    {
        PIECE_SELECTION,
        PIECE_CONFIRMATION,
        DECIDE_MOVE_OR_REROLL,//TODO: OnlyReroll Once(AfterDebugging done on movesets)
        CONTEST,//TODO: Add a reroll option that uses resources(dont exist yet)
        MOVE_AND_UPDATE,
        END_OF_TURN // Piece movement should only be finalized here after options to reroll given
    }

    public PhaseInTurn phaseInTurn { get; private set; }
    // Start is called before the first frame update
    void Start()
    {
        if (IsOwner == false) return;
        instance = this;
        phaseInTurn = PhaseInTurn.PIECE_SELECTION;
        Input_Controller.instance.OnLeftMouseClick += OnLeftMouseClick;
        Input_Controller.instance.OnRollAgainButtonClicked += OnRollAgainPressed;
        Input_Controller.instance.OnEndTurnButtonClicked += OnEndTurnPressed;
        Input_Controller.instance.OnContestButtonClicked += OnContestPressed;
        Input_Controller.instance.OnDeclineButtonClicked += OnDeclinePressed;
        //SHOULD BE ON ANOTHER SCRIPT MAYBE A DISCARD AREA IN THE FUTURE
        Board_Data.instance.OnPieceRemovedFromBoard += OnPieceRemoved;
        Board_Data.instance.OnPieceDamaged += OnPieceDamaged;
    }

    void OnLeftMouseClick(object sender, EventArgs e)
    {
        if (IsOwner == false) return;
        AdvanceGame(true);
    }

    void AdvanceGame(bool mouseClicked = false)
    {
        if (IsOwner == false) return;
        switch (phaseInTurn) {
            case PhaseInTurn.PIECE_SELECTION:                   // DONE
                Debug.Log("Applying PIECE_SELECTION");
                TargetPieceToSelect();
                break;
            case PhaseInTurn.PIECE_CONFIRMATION:                // DONE
                Debug.Log("Applying PIECE_CONFIRMATION");
                PieceSelection();
                break;
            case PhaseInTurn.DECIDE_MOVE_OR_REROLL:
                Debug.Log("Applying DECIDE_MOVE_OR_REROLL");    // DONE
                DecideMove();
                break;
            case PhaseInTurn.CONTEST:
                Debug.Log("Applying CONTEST");                  // DONE
                break;
            case PhaseInTurn.MOVE_AND_UPDATE:
                Debug.Log("Applying MOVE_AND_UPDATE");          // DONE
                break;
            case PhaseInTurn.END_OF_TURN:
                Debug.Log("Applying END_OF_TURN");
                //For Testing turn back to start of turn
                rerolled = false;
                phaseInTurn = PhaseInTurn.PIECE_SELECTION;
                break;
            default:
                break;
        }
    }

    void TargetPieceToSelect()
    {
        Piece_Data piece = Piece_Detection.GetPieceUnderMouse();
        if(piece != null) {
            selectedPiece = piece;

            //Debug.Log(selectedPiece.gameObject.name);
            HighLightSelectedPiece();

            phaseInTurn = PhaseInTurn.PIECE_CONFIRMATION;
            //Happens on click, Game automatically advances
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
                Contest();
            }
        }
    }

    void Contest()
    {
        if (Board_Data.instance.boardPieces[decidedMove.x,decidedMove.y] == null ||
            Input_Controller.instance.whiteButtonHolder.transform.childCount +
            Input_Controller.instance.whiteButtonHolder.transform.childCount == 0 ||
            Board_Data.instance.boardPieces[decidedMove.x, decidedMove.y].IsDamaged == true)
        {
            //Skip Contest Phase
            MovePiece();
            return;
        }
        coinFlip = UnityEngine.Random.Range(0, 2);  //0 = capture, 1 = damage
        Input_Controller.instance.contestHolder.SetActive(true);
        if (coinFlip == CAPTURE)
        {
            Input_Controller.instance.contestText.text = "Piece Capture";
        }
        else
        {
            Input_Controller.instance.contestText.text = "Piece Damage";
        }
    }

    void EndContest()
    {
        //TODO: Call if time runs out
        Input_Controller.instance.contestHolder.SetActive(false);
        MovePiece(true);
    }

    void OnContestPressed(object sender, EventArgs e)
    {
        if (phaseInTurn == PhaseInTurn.CONTEST) {
            Contest();
        }
        else {
            //Refund Token used
            //TODO: Check who used the token probably should be done in the input controller
            //Only inputs from player who's turn it is count in future multiplayer.
            Input_Controller.instance.GiveContestToken(Piece_Data.Color.white);
        }
    }

    void OnDeclinePressed(object sender, EventArgs e)
    {
        EndContest();
    }

    void MovePiece(bool contest = false)
    {
        if(contest == false)
        {
            Board_Data.instance.MoveAndTake(selectedPiece, decidedMove.x, decidedMove.y);
        }
        else if(coinFlip == CAPTURE)
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

        Board_Display.Instance.UpdatePieces();
        RemoveHighLightPossibleMoves();
        RemoveHighlightOnSelectedPiece();
        phaseInTurn = PhaseInTurn.END_OF_TURN;
        
    }

    void OnRollAgainPressed(object sender, EventArgs e)
    {
        if(phaseInTurn == PhaseInTurn.DECIDE_MOVE_OR_REROLL && rerolled == false) {
            RemoveHighLightPossibleMoves();
            RemoveHighlightOnSelectedPiece();

            rerolled = true;
            phaseInTurn = PhaseInTurn.PIECE_SELECTION;
        }
    }

    void OnEndTurnPressed(object sender, EventArgs e)
    {
        RemoveHighLightPossibleMoves();
        rerolled = false;
        phaseInTurn = PhaseInTurn.END_OF_TURN;
    }

    void PieceSelection()
    {
        if (Input.GetMouseButtonDown(0)) {
            Piece_Data pieceData = Piece_Detection.GetPieceUnderMouse();
            if (pieceData != null) {
                RemoveHighlightOnSelectedPiece();

                if (selectedPiece == pieceData) {

                    selectedPiece = pieceData;
                    TransformSelectedPiece();

                    HighLightPossibleMoves();
                    phaseInTurn = PhaseInTurn.DECIDE_MOVE_OR_REROLL;
                }
                else {
                    TargetPieceToSelect();
                }
            }
        }
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

    void TransformSelectedPiece()
    {
        int randomMove = UnityEngine.Random.Range(0, chessMoves.Length);

        validMoves = Board_Data.instance.getAllLegalMoves(selectedPiece, chessMoves[randomMove]);

        selectedPiece.type = chessMoves[randomMove].pieceType;
        selectedPiece.gameObject.GetComponent<SpriteRenderer>().sprite =
                chessMoves[randomMove].GetSprite(selectedPiece.GetColor(), selectedPiece.IsDamaged);

        EventArgsOnPieceTransformed e = new EventArgsOnPieceTransformed();
        e.Piece = selectedPiece;
        e.randomMove = randomMove;

        OnPieceTransformed?.Invoke(this, e);
    }
    void HighLightPossibleMoves()
    {
        foreach (Vector2Int moveLocation in validMoves) {
            SpriteRenderer spriteRenderer = Board_Display.Instance.boardTiles[moveLocation.x, moveLocation.y].GetComponent<SpriteRenderer>();
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, .1f);
        }

        EventArgsOnValidMovesHighlighted e = new EventArgsOnValidMovesHighlighted();
        e.validMoves = validMoves.ToArray();
        OnValidMovesHighlighted?.Invoke(this, e);
    }

    void RemoveHighLightPossibleMoves()
    {
        if (validMoves == null) {
            Debug.LogWarning("Removing Highlighted Tiles with any moves");
            return;
        }

        foreach (Vector2Int moveLocation in validMoves) {
            SpriteRenderer spriteRenderer = Board_Display.Instance.boardTiles[moveLocation.x, moveLocation.y].GetComponent<SpriteRenderer>();
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 1f);
        }

        EventArgsOnValidMovesDeHighlighted e = new EventArgsOnValidMovesDeHighlighted();
        e.validMoves = validMoves.ToArray();
        OnValidMovesDeHighlighted?.Invoke(this, e);
    }

    //SHOULD BE ON ANOTHER SCRIPT MAYBE A DISCARD AREA IN THE FUTURE
    void OnPieceRemoved(object sender, Board_Data.EventArgsPieceRemoved e)
    {
        Destroy(e.removedPiece.gameObject);
    }
    
    void OnPieceDamaged(object sender, Board_Data.EventArgsPieceDamaged e)
    {
        Board_Display.Instance.UpdatePieces();
    }
}

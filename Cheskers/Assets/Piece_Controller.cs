using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Board_Data;

public class Piece_Controller : MonoBehaviour
{
    [SerializeField] Chess_Move_SO[] chessMoves;

    Board_Data boardData;
    Piece_Data selectedPiece;

    List<Vector2Int> possibleMoves;

    bool isYourTurn;
    Piece_Data.Color color = Piece_Data.Color.white;

    enum PhaseInTurn
    {
        PIECE_SELECTION,
        MOVE_OR_ROLL_AGAIN,
        MOVE,
        APPLY_DAMAGE,//may be skipped if piece taken or no piece attacked
        END_OF_TURN
    }

    PhaseInTurn phaseInTurn;
    // Start is called before the first frame update
    void Start()
    {
        phaseInTurn = PhaseInTurn.PIECE_SELECTION;
        boardData = Board_Display.boardData;
        Input_Controller.instance.OnLeftMouseClick += OnLeftMouseClick;
        Input_Controller.instance.OnRollAgainButtonClicked += OnRollAgainPressed;
        Input_Controller.instance.OnEndTurnButtonClicked += OnEndTurnPressed;
        //SHOULD BE ON ANOTHER SCRIPT MAYBE A DISCARD AREA IN THE FUTURE
        boardData.OnPieceRemovedFromBoard += OnPieceRemoved;

    }

    void OnLeftMouseClick(object sender, EventArgs e)
    {
        switch (phaseInTurn) {
            case PhaseInTurn.PIECE_SELECTION:
                Debug.Log("Applying PIECE_SELECTION");
                PieceSelection();
                break;
            case PhaseInTurn.MOVE_OR_ROLL_AGAIN:
                Debug.Log("Applying MOVE_OR_ROLL_AGAIN");
                MovePiece();
                break;
            case PhaseInTurn.MOVE:
                Debug.Log("Applying MOVE");
                MovePiece();
                break;
            case PhaseInTurn.APPLY_DAMAGE:
                Debug.Log("Applying APPLY_DAMAGE");
                //MaybeNotNeed probably for playing animation or something
                break;
            case PhaseInTurn.END_OF_TURN:
                Debug.Log("Applying END_OF_TURN");
                //For Testing turn back to start of turn
                phaseInTurn = PhaseInTurn.PIECE_SELECTION;
                break;
            default:
                break;
        }
    }

    void MovePiece()
    {
        Vector2Int tileLocation = Piece_Detection.GetTileIndexUnderMouse();
        
        foreach (Vector2Int moveToTile in possibleMoves) {
            if(moveToTile == tileLocation) {

                boardData.MovePiece(selectedPiece, moveToTile.x, moveToTile.y);
                Board_Display.UpdatePiecesPositionsInGame();
                
                RemoveHighLightPossibleMoves();
                phaseInTurn = PhaseInTurn.END_OF_TURN;
                break;
            }
        }
    }

    void OnRollAgainPressed(object sender, EventArgs e)
    {
        Debug.Log("Roll again press in " + phaseInTurn.ToString());
        if (phaseInTurn == PhaseInTurn.MOVE_OR_ROLL_AGAIN) {
            Debug.Log(phaseInTurn.ToString());
            RemoveHighLightPossibleMoves();

            UpdateSelectedPiece();

            HighLightPossibleMoves();

            phaseInTurn = PhaseInTurn.MOVE;
        }
    }

    void OnEndTurnPressed(object sender, EventArgs e)
    {
        if (selectedPiece != null) {
            RemoveHighLightPossibleMoves();
        }
        phaseInTurn = PhaseInTurn.END_OF_TURN;
    }

    void PieceSelection()
    {
        if (Input.GetMouseButtonDown(0)) {
            Piece_Data pieceData = Piece_Detection.GetPieceUnderMouse();
            if (pieceData != null) {
                if (selectedPiece != null) {
                    RemoveHighLightPossibleMoves();
                }

                selectedPiece = pieceData;
                UpdateSelectedPiece();

                HighLightPossibleMoves();
                phaseInTurn = PhaseInTurn.MOVE_OR_ROLL_AGAIN;
            }
        }
    }

    void UpdateSelectedPiece()
    {
        int randomMove = UnityEngine.Random.Range(0, chessMoves.Length);

        possibleMoves = boardData.getAllLegalMoves(selectedPiece, chessMoves[randomMove]);

        selectedPiece.type = chessMoves[randomMove].pieceType;
        selectedPiece.gameObject.GetComponent<SpriteRenderer>().sprite =
                chessMoves[randomMove].GetSprite(selectedPiece.getColor(), selectedPiece.IsDamaged);
    }
    void HighLightPossibleMoves()
    {
        foreach (Vector2Int moveLocation in possibleMoves) {
            SpriteRenderer sprite = Board_Display.boardTiles[moveLocation.x, moveLocation.y].GetComponent<SpriteRenderer>();
            sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, .1f);
        }
    }

    void RemoveHighLightPossibleMoves()
    {
        foreach (Vector2Int moveLocation in possibleMoves) {
            SpriteRenderer sprite = Board_Display.boardTiles[moveLocation.x, moveLocation.y].GetComponent<SpriteRenderer>();
            sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, 1f);
        }
    }

    //SHOULD BE ON ANOTHER SCRIPT MAYBE A DISCARD AREA IN THE FUTURE
    void OnPieceRemoved(object sender, PieceRemovedEventArgs e)
    {
        Destroy(e.removedPiece.gameObject);
    }
}

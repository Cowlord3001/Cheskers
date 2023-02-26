using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        boardData = Board_Setup.boardData;
        Input_Controller.instance.OnLeftMouseClick += OnLeftMouseClick;
        Input_Controller.instance.OnRollAgainButtonClicked += OnRollAgainPressed;
        Input_Controller.instance.OnEndTurnButtonClicked += OnEndTurnPressed;

    }

    void OnLeftMouseClick(object sender, EventArgs e)
    {
        switch (phaseInTurn) {
            case PhaseInTurn.PIECE_SELECTION:
                PieceSelection();
                break;
            case PhaseInTurn.MOVE_OR_ROLL_AGAIN:
                MovePiece();
                break;
            case PhaseInTurn.MOVE:
                MovePiece();
                break;
            case PhaseInTurn.APPLY_DAMAGE:
                //MaybeNotNeed probably for playing animation or something
                break;
            case PhaseInTurn.END_OF_TURN:
                //For Testing turn back to start of turn
                phaseInTurn = PhaseInTurn.PIECE_SELECTION;
                break;
            default:
                break;
        }
    }

    void MovePiece()
    {
        Debug.Log("Attempting To Move Piece");
        Vector2Int tileLocation = Piece_Detection.GetTileIndexUnderMouse();
        
        foreach (Vector2Int moveToTile in possibleMoves) {
            if(moveToTile == tileLocation) {

                Board_Setup.boardData.MovePiece(selectedPiece, moveToTile.x, moveToTile.y);
                Board_Setup.UpdatePiecesPositions();
                Debug.Log("Piece Moved, New Position: " + selectedPiece.positionOnBoard);
                RemoveHighLightPossibleMoves();
                phaseInTurn = PhaseInTurn.END_OF_TURN;
                break;
            }
        }
    }

    void OnRollAgainPressed(object sender, EventArgs e)
    {
        if (phaseInTurn == PhaseInTurn.MOVE_OR_ROLL_AGAIN) {
            RemoveHighLightPossibleMoves();

            int randomMove = UnityEngine.Random.Range(0, chessMoves.Length);

            possibleMoves = boardData.getAllLegalMoves(selectedPiece, chessMoves[randomMove]);
            Debug.Log(chessMoves[randomMove].name);
            HighLightPossibleMoves();

            phaseInTurn = PhaseInTurn.MOVE;
        }
    }

    void OnEndTurnPressed(object sender, EventArgs e)
    {
        RemoveHighLightPossibleMoves();
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
                int randomMove = UnityEngine.Random.Range(0, chessMoves.Length);

                possibleMoves = boardData.getAllLegalMoves(pieceData, chessMoves[randomMove]);
                Debug.Log(chessMoves[randomMove].name);
                HighLightPossibleMoves();
                phaseInTurn = PhaseInTurn.MOVE_OR_ROLL_AGAIN;
            }
        }
    }
    void HighLightPossibleMoves()
    {
        foreach (Vector2Int moveLocation in possibleMoves) {
            SpriteRenderer sprite = Board_Setup.boardTiles[moveLocation.x, moveLocation.y].GetComponent<SpriteRenderer>();
            sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, .1f);
        }
    }

    void RemoveHighLightPossibleMoves()
    {
        foreach (Vector2Int moveLocation in possibleMoves) {
            SpriteRenderer sprite = Board_Setup.boardTiles[moveLocation.x, moveLocation.y].GetComponent<SpriteRenderer>();
            sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, 1f);
        }
    }

}

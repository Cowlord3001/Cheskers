using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Board_Data;

public class Piece_Controller : MonoBehaviour
{
    [SerializeField] Chess_Move_SO[] chessMoves;
    [SerializeField] GameObject highLightGraphicPrefab;
    GameObject highLightGraphic;

    Board_Data boardData;
    Piece_Data selectedPiece;

    List<Vector2Int> possibleMoves;

    bool isYourTurn;
    Piece_Data.Color color = Piece_Data.Color.white;

    enum PhaseInTurn
    {
        PIECE_SELECTION,
        PIECE_CONFIRMATION, 
        MOVE_OR_ROLL_AGAIN, //TODO: OnlyReroll Once(AfterDebugging done on movesets)
        MOVE,
        APPLY_DAMAGE,//TODO: Add a reroll option that uses resources(dont exist yet)
        END_OF_TURN // Piece movement should only be finalized here after options to reroll given
    }

    PhaseInTurn phaseInTurn;
    // Start is called before the first frame update
    void Start()
    {
        phaseInTurn = PhaseInTurn.PIECE_SELECTION;
        boardData = Board_Display.Instance.boardData;
        Input_Controller.instance.OnLeftMouseClick += OnLeftMouseClick;
        Input_Controller.instance.OnRollAgainButtonClicked += OnRollAgainPressed;
        Input_Controller.instance.OnEndTurnButtonClicked += OnEndTurnPressed;
        //SHOULD BE ON ANOTHER SCRIPT MAYBE A DISCARD AREA IN THE FUTURE
        boardData.OnPieceRemovedFromBoard += OnPieceRemoved;
        boardData.OnPieceDamaged += OnPieceDamaged;

    }

    void OnLeftMouseClick(object sender, EventArgs e)
    {
        switch (phaseInTurn) {
            case PhaseInTurn.PIECE_SELECTION:
                Debug.Log("Applying PIECE_SELECTION");
                TargetPieceToSelect();
                //PieceSelection();
                break;
            case PhaseInTurn.PIECE_CONFIRMATION:
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

    void TargetPieceToSelect()
    {
        Piece_Data piece = Piece_Detection.GetPieceUnderMouse();
        if(piece != null) {
            selectedPiece = piece;

            HighLightSelectecPiece();

            phaseInTurn = PhaseInTurn.PIECE_CONFIRMATION;

        }
    }

    void MovePiece()
    {
        Vector2Int tileLocation = Piece_Detection.GetTileIndexUnderMouse();
        
        foreach (Vector2Int moveToTile in possibleMoves) {
            if(moveToTile == tileLocation) {

                Debug.Log("Attempting to Move");
                //TODO: Decide between damaging and taking
                //Need and Rerolls... 

                //boardData.MoveAndDamage(selectedPiece, moveToTile.x, moveToTile.y);
                boardData.MoveAndTake(selectedPiece, moveToTile.x, moveToTile.y);
                Debug.Log(Board_Data.debugMessage);
                Board_Display.Instance.UpdatePieces();
                
                RemoveHighLightPossibleMoves();
                RemoveHighlightOnSelectedPiece();
                phaseInTurn = PhaseInTurn.END_OF_TURN;
                break;
            }
        }
    }

    void OnRollAgainPressed(object sender, EventArgs e)
    {

        if(phaseInTurn == PhaseInTurn.MOVE_OR_ROLL_AGAIN) {
            RemoveHighLightPossibleMoves();
            RemoveHighlightOnSelectedPiece();

            phaseInTurn = PhaseInTurn.PIECE_SELECTION;
        }

        /*
        Debug.Log("Roll again press in " + phaseInTurn.ToString());
        if (phaseInTurn == PhaseInTurn.MOVE_OR_ROLL_AGAIN) {
            Debug.Log(phaseInTurn.ToString());
            RemoveHighLightPossibleMoves();
        
            UpdateSelectedPiece();
        
            HighLightPossibleMoves();
        
            phaseInTurn = PhaseInTurn.MOVE;
        }
        */
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
                RemoveHighlightOnSelectedPiece();

                if (selectedPiece == pieceData) {

                    selectedPiece = pieceData;
                    UpdateSelectedPiece();

                    HighLightPossibleMoves();
                    phaseInTurn = PhaseInTurn.MOVE_OR_ROLL_AGAIN;
                }
                else {
                    TargetPieceToSelect();
                }
            }
        }
    }

    void HighLightSelectecPiece()
    {
        if (highLightGraphic == null) {
            highLightGraphic = Instantiate(highLightGraphicPrefab, 
                                           selectedPiece.gameObject.transform.position, 
                                           Quaternion.identity);

        }
    }

    void RemoveHighlightOnSelectedPiece()
    {
        if (highLightGraphic != null) {
            Destroy(highLightGraphic);
        }
    }

    void UpdateSelectedPiece()
    {
        int randomMove = UnityEngine.Random.Range(0, chessMoves.Length);

        possibleMoves = boardData.getAllLegalMoves(selectedPiece, chessMoves[randomMove]);

        selectedPiece.type = chessMoves[randomMove].pieceType;
        selectedPiece.gameObject.GetComponent<SpriteRenderer>().sprite =
                chessMoves[randomMove].GetSprite(selectedPiece.GetColor(), selectedPiece.IsDamaged);
    }
    void HighLightPossibleMoves()
    {
        foreach (Vector2Int moveLocation in possibleMoves) {
            SpriteRenderer spriteRenderer = Board_Display.Instance.boardTiles[moveLocation.x, moveLocation.y].GetComponent<SpriteRenderer>();
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, .1f);
        }
    }

    void RemoveHighLightPossibleMoves()
    {
        if (possibleMoves == null) {
            Debug.LogWarning("Removing Highlighted Tiles with any moves");
            return;
        }

        foreach (Vector2Int moveLocation in possibleMoves) {
            SpriteRenderer spriteRenderer = Board_Display.Instance.boardTiles[moveLocation.x, moveLocation.y].GetComponent<SpriteRenderer>();
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 1f);
        }
    }

    //SHOULD BE ON ANOTHER SCRIPT MAYBE A DISCARD AREA IN THE FUTURE
    void OnPieceRemoved(object sender, PieceRemovedEventArgs e)
    {
        Destroy(e.removedPiece.gameObject);
    }
    
    void OnPieceDamaged(object sender, PieceDamageEventArgs e)
    {
        Board_Display.Instance.UpdatePieces();
    }
}

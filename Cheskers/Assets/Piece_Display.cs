using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece_Display : MonoBehaviour
{
    public static Piece_Display instance;

    public Chess_Move_SO[] chessMoves;

    public GameObject whitePiece;
    public GameObject blackPiece;
    [SerializeField] Sprite blackSprite;
    [SerializeField] Sprite whiteSprite;
    [SerializeField] Sprite blackDamagedSprite;
    [SerializeField] Sprite whiteDamagedSprite;

    public event EventHandler<EventArgsOnPieceTransformed> OnPieceTransformed;
    public class EventArgsOnPieceTransformed { public Piece_Data Piece; public int chessMoveIndex; };

    private void Awake()
    {
        instance = this;
        //Creates Board_Data
        Board_Data.instance = new Board_Data();
        FillBoard_DataWithPieces();
    }

    private void Start()
    {
        Board_Data.instance.OnPieceRemovedFromBoard += OnPieceRemoved;
        Board_Data.instance.OnPieceDamaged += OnPieceDamaged;
    }
    void FillBoard_DataWithPieces()
    {
        //Fill with white or black pieces
        for (int x = 0; x < Board_Data.instance.size; x++) {
            for (int y = 0; y < Board_Data.instance.size; y++) {
                if (Board_Data.instance.boardPieces[x, y] != null) {
                    if (Board_Data.instance.boardPieces[x, y].GetColor() == Piece_Data.Color.white) {
                        Board_Data.instance.boardPieces[x, y].gameObject = Instantiate(whitePiece, transform.position, Quaternion.identity);
                    }
                    else {
                        Board_Data.instance.boardPieces[x, y].gameObject = Instantiate(blackPiece, transform.position, Quaternion.identity);
                    }
                    Board_Data.instance.boardPieces[x, y].gameObject.transform.parent = transform;
                }
            }
        }

        UpdatePieces();

    }
    //Updates the piece display data to game
    public void UpdatePieces()
    {
        for (int x = 0; x < Board_Data.instance.size; x++) {
            for (int y = 0; y < Board_Data.instance.size; y++) {
                if (Board_Data.instance.boardPieces[x, y] != null) {
                    GameObject go = Board_Data.instance.boardPieces[x, y].gameObject;
                    go.transform.position = Piece_Detection.BoardIndextoWorld(x, y);
                    go.GetComponent<SpriteRenderer>().sprite = GetDefaultSprite(Board_Data.instance.boardPieces[x, y].GetColor(), Board_Data.instance.boardPieces[x, y].IsDamaged);
                }
            }
        }
    }
    Sprite GetDefaultSprite(Piece_Data.Color color, bool isDamaged)
    {
        switch (color) {
            case Piece_Data.Color.white:
                if (isDamaged) return whiteDamagedSprite;
                else return whiteSprite;
            case Piece_Data.Color.black:
                if (isDamaged) return blackDamagedSprite;
                else return blackSprite;
            default:
                return null;
        }
    }
    public void TransformSelectedPiece(Piece_Data selectedPiece, int chessMoveIndex)
    {
        Chess_Move_SO chessMove = chessMoves[chessMoveIndex];

        selectedPiece.gameObject.GetComponent<SpriteRenderer>().sprite =
                chessMove.GetSprite(selectedPiece.GetColor(), selectedPiece.IsDamaged);

        EventArgsOnPieceTransformed e = new EventArgsOnPieceTransformed();
        e.Piece = selectedPiece;
        e.chessMoveIndex = chessMoveIndex;
        
        OnPieceTransformed?.Invoke(this, e);
    }

    void OnPieceRemoved(object sender, Board_Data.EventArgsPieceRemoved e)
    {
        Destroy(e.removedPiece.gameObject);
    }

    void OnPieceDamaged(object sender, Board_Data.EventArgsPieceDamaged e)
    {
        UpdatePieces();
    }
}

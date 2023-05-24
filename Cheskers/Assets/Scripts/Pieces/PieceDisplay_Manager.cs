using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceDisplay_Manager : MonoBehaviour
{
    public static PieceDisplay_Manager instance;

    [SerializeField] Chess_Move_SO[] chessMoves;
    Dictionary<Piece.Type, Chess_Move_SO> chessMoves_Dictionary;
    [SerializeField] Chess_Move_SO noneChessMove;

    public GameObject whitePiecePrefab;
    public GameObject blackPiecePrefab;

    List<GameObject> whitePieceGameObjectList;
    List<GameObject> blackPieceGameObjectList;

    public event EventHandler<EventArgsOnPieceTransformed> OnPieceTransformed;
    public class EventArgsOnPieceTransformed { public Piece Piece; public int chessMoveIndex; };

    private void Awake()
    {
        instance = this;
        //Creates Board_Data
        Board.instance = new Board();

        chessMoves_Dictionary = new Dictionary<Piece.Type, Chess_Move_SO>();
        for (int i = 0; i < chessMoves.Length; i++) {
            chessMoves_Dictionary.Add(chessMoves[i].pieceType, chessMoves[i]);
        }
        chessMoves_Dictionary.Add(noneChessMove.pieceType, noneChessMove);


        SpawnBoardPieces();
    }

    private void Start()
    {
        Board.instance.OnPieceRemovedFromBoard += OnPieceRemoved;
        Board.instance.OnPieceDamaged += OnPieceDamaged;
    }
    void SpawnBoardPieces()
    {
        whitePieceGameObjectList = new List<GameObject>();
        blackPieceGameObjectList = new List<GameObject>();

        for (int i = 0; i < 16; i++) {
            whitePieceGameObjectList.Add(Instantiate(whitePiecePrefab, transform));
            blackPieceGameObjectList.Add(Instantiate(blackPiecePrefab, transform));
        }

        UpdatePieces();

    }
    //Updates the piece display data to game
    public void UpdatePieces()
    {
        //Turn off pieces and vip display
        for (int i = 0; i < 16; i++) {
            whitePieceGameObjectList[i].SetActive(false);
            whitePieceGameObjectList[i].transform.GetChild(0).gameObject.SetActive(false);
            blackPieceGameObjectList[i].SetActive(false);
            blackPieceGameObjectList[i].transform.GetChild(0).gameObject.SetActive(false);
        }

        //Loop through board pieces and give each a representation in the game
        int blackIndex = 0;
        int whiteIndex = 0;
        GameObject go;
        foreach (var piece in Board.instance.GetPieces()) {
            //Decide to use black or white
            if(piece.GetColor() == Piece.Color.white) {
                go = whitePieceGameObjectList[whiteIndex];
                whiteIndex++;
            }
            else {
                go = blackPieceGameObjectList[blackIndex];
                blackIndex++;
            }
            //Move to correct position with correct sprite.
            go.transform.position = Piece_Detection.BoardIndextoWorld(piece.positionOnBoard);
            go.SetActive(true);
            go.GetComponent<SpriteRenderer>().sprite = GetSprite(piece);
            if (piece.IsVIP) {
                go.transform.GetChild(0).gameObject.SetActive(true);
            }
            
        }
    }
    Sprite GetSprite(Piece piece)
    {
        //TODO: Health could be bigger than 2 in the future
        switch (piece.GetColor()) {
            case Piece.Color.white:
                if (piece.health == 1) return chessMoves_Dictionary[piece.type].spriteWhiteDamaged;
                else return chessMoves_Dictionary[piece.type].spriteWhiteNoDamage;
            case Piece.Color.black:
                if (piece.health == 1) return chessMoves_Dictionary[piece.type].spriteBlackDamaged;
                else return chessMoves_Dictionary[piece.type].spriteBlackNoDamage;
            default:
                return null;
        }
    }

    public Chess_Move_SO GetChessMoveByIndex(int i)
    {
        return chessMoves[i];
    }
    public Chess_Move_SO GetChessMoveByType(Piece.Type type)
    {
        return chessMoves_Dictionary[type];
    }

    GameObject GetGameObjectAtPosition(Piece selectedPiece)
    {
        Vector3 targetPosition = Piece_Detection.BoardIndextoWorld(selectedPiece.positionOnBoard);
        if(selectedPiece.GetColor() == Piece.Color.white) {
            foreach (GameObject piece in whitePieceGameObjectList) {
                if(piece.transform.position == targetPosition) {
                    return piece;
                }
            }
        }
        else {
            foreach (GameObject piece in blackPieceGameObjectList) {
                if (piece.transform.position == targetPosition) {
                    return piece;
                }
            }
        }
        return null;
    }

    public void TransformSelectedPiece(Piece selectedPiece)
    {
        GetGameObjectAtPosition(selectedPiece).GetComponent<SpriteRenderer>().sprite = GetSprite(selectedPiece);

        EventArgsOnPieceTransformed e = new EventArgsOnPieceTransformed();
        e.Piece = selectedPiece;
        
        OnPieceTransformed?.Invoke(this, e);
    }
    void OnPieceRemoved(object sender, Board.EventArgsPieceRemoved e)
    {
        GetGameObjectAtPosition(e.removedPiece).SetActive(false);//Probably unecessary
        UpdatePieces();
    }
    void OnPieceDamaged(object sender, Board.EventArgsPieceDamaged e)
    {
        UpdatePieces();
    }
}

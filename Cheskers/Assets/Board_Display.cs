using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class Board_Display : MonoBehaviour
{
    [SerializeField] GameObject blackTile;
    [SerializeField] GameObject whiteTile;

    public static Board_Display Instance;

    public Board_Data boardData { get; private set; }
    public GameObject[,] boardTiles { get; private set; }

    [SerializeField] GameObject whitePiece;
    [SerializeField] GameObject blackPiece;
    [SerializeField] Sprite blackSprite;
    [SerializeField] Sprite whiteSprite;
    [SerializeField] Sprite blackDamagedSprite;
    [SerializeField] Sprite whiteDamagedSprite;


    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;
        CreateBoard_Data();
        CreateBoard();

    }

    void CreateBoard_Data()
    {
        //Creates Board and Piece
        boardData = new Board_Data();

        //Fill with white or black pieces
        for (int x = 0; x < boardData.size; x++) {
            for (int y = 0; y < boardData.size; y++) {
                if (boardData.boardPieces[x,y] != null) {
                    if (boardData.boardPieces[x, y].GetColor() == Piece_Data.Color.white) {
                        boardData.boardPieces[x, y].gameObject = Instantiate(whitePiece, transform.position, Quaternion.identity);
                    }
                    else {
                        boardData.boardPieces[x, y].gameObject = Instantiate(blackPiece, transform.position, Quaternion.identity);
                    }
                    boardData.boardPieces[x, y].gameObject.transform.parent = transform;
                }
            }
        }

        UpdatePieces();

    }

    //Updates the piece display data to game
    public void UpdatePieces()
    {
        for (int x = 0; x < boardData.size; x++) {
            for (int y = 0; y < boardData.size; y++) {
                if (boardData.boardPieces[x, y] != null) {
                    GameObject go = boardData.boardPieces[x, y].gameObject;
                    go.transform.position = Piece_Detection.BoardIndextoWorld(x, y);
                    go.GetComponent<SpriteRenderer>().sprite = GetDefaultSprite(boardData.boardPieces[x, y].GetColor(), boardData.boardPieces[x, y].IsDamaged);
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

    void CreateBoard()
    {

        boardTiles = new GameObject[boardData.size, boardData.size];
        float halfWidth = boardData.size / 2;
        float tileWidth = .5f;
        bool isWhite = true;
        for (int y = 0; y < boardData.size; y++) {
            for (int x = 0; x < boardData.size; x++) {
                if(isWhite == true) {
                    isWhite = false;
                    GameObject go = Instantiate(whiteTile);
                    go.transform.position = new Vector2(x - halfWidth + tileWidth, y - halfWidth + tileWidth);
                    go.transform.parent = transform;
                    boardTiles[x, y] = go;
                }
                else {
                    isWhite = true;
                    GameObject go = Instantiate(blackTile);
                    go.transform.position = new Vector2(x - halfWidth + tileWidth, y - halfWidth + tileWidth);
                    go.transform.parent = transform;
                    boardTiles[x, y] = go;
                }
            }
            isWhite = !isWhite;
        }
        /*
        for (int y = 0; y < 8; y++) {
            for (int x = 0; x < 4; x++) {
                if (y % 2 == 0) //y is even
                {
                    float x1 = (float)(2 * x - 3.5); // -3.5, -1.5, 1.5, 3.5
                    float y1 = (float)(y - 3.5);
                    GameObject go = Instantiate(blackTile, transform.position,Quaternion.identity);
                    go.transform.position = new Vector2(x1, y1);
                    go.transform.parent = transform;
                    boardTiles[x, y] = go;
                    go = Instantiate(whiteTile);
                    go.transform.position = new Vector2(x1 + 1, y1);
                    go.transform.parent = transform;
                    boardTiles[x + 1, y] = go;
                }
                else //y is odd
                {
                    float x1 = (float)(2 * x - 2.5); // -2.5, -0.5, 2.5, 4.5
                    float y1 = (float)(y - 3.5);
                    GameObject go = Instantiate(blackTile, transform.position, Quaternion.identity);
                    go.transform.position = new Vector2(x1, y1);
                    go.transform.parent = transform;
                    boardTiles[x, y] = go;
                    go = Instantiate(whiteTile, transform.position, Quaternion.identity);
                    go.transform.position = new Vector2(x1 - 1, y1);
                    go.transform.parent = transform;
                    boardTiles[x - 1, y] = go;
                }
            }
        }
        */
    }
    // Update is called once per frame
    void Update()
    {

    }
}
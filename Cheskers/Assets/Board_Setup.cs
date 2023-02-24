using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class Board_Setup : MonoBehaviour
{
    [SerializeField] GameObject blackTile;
    [SerializeField] GameObject whiteTile;
    Board_Data boardData;

    [SerializeField] GameObject whitePiece;
    [SerializeField] GameObject blackPiece;


    // Start is called before the first frame update
    void Start()
    {
        CreateBoard();
        CreateBoard_Data();
    }

    void CreateBoard_Data()
    {
        //Creates Board and Piece
        boardData = new Board_Data();
        //Fill with white or black pieces
        for (int x = 0; x < boardData.size; x++) {
            for (int y = 0; y < boardData.size; y++) {
                if (boardData.boardPieces[x,y] != null) {
                    if (boardData.boardPieces[x, y].getColor() == Piece_Data.Color.white) {
                        boardData.boardPieces[x, y].gameObject = Instantiate(whitePiece, transform.position, Quaternion.identity);
                    }
                    else {
                        boardData.boardPieces[x, y].gameObject = Instantiate(blackPiece, transform.position, Quaternion.identity);
                    }
                    boardData.boardPieces[x, y].gameObject.transform.parent = transform;
                }
            }
        }

        UpdatePiecesPositions();

    }

    void UpdatePiecesPositions()
    {
        for (int x = 0; x < boardData.size; x++) {
            for (int y = 0; y < boardData.size; y++) {
                if (boardData.boardPieces[x, y] != null) {
                    boardData.boardPieces[x, y].gameObject.transform.position = BoardtoWorld(x, y);
                }
            }
        }
    }
    Vector2 BoardtoWorld(int boardPositionx, int boardPositiony)
    {
        Vector2 worldPosition = Vector2.zero;
        float halfWidth = boardData.size/2;
        float halfPieceSize = .5f;
        worldPosition.x = boardPositionx - halfWidth + halfPieceSize;
        worldPosition.y = boardPositiony - halfWidth + halfPieceSize;

        return worldPosition;
    }
    

    void CreateBoard()
    {
        for (int y = 0; y < 8; y++) {
            for (int x = 0; x < 4; x++) {
                if (y % 2 == 0) //y is even
                {
                    float x1 = (float)(2 * x - 3.5); // -3.5, -1.5, 1.5, 3.5
                    float y1 = (float)(y - 3.5);
                    GameObject go = Instantiate(blackTile, transform.position,Quaternion.identity);
                    go.transform.position = new Vector2(x1, y1);
                    go.transform.parent = transform;
                    go = Instantiate(whiteTile);
                    go.transform.position = new Vector2(x1 + 1, y1);
                    go.transform.parent = transform;
                }
                else //y is odd
                {
                    float x1 = (float)(2 * x - 2.5); // -2.5, -0.5, 2.5, 4.5
                    float y1 = (float)(y - 3.5);
                    GameObject go = Instantiate(blackTile, transform.position, Quaternion.identity);
                    go.transform.position = new Vector2(x1, y1);
                    go.transform.parent = transform;
                    go = Instantiate(whiteTile, transform.position, Quaternion.identity);
                    go.transform.position = new Vector2(x1 - 1, y1);
                    go.transform.parent = transform;
                }
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}

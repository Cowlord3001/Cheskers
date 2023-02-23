using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board_Data
{
    Piece_Data[,] boardPieces;
    int boardSize;

    public Board_Data()
    {
        boardSize = 8;
        boardPieces = new Piece_Data[boardSize, boardSize];
        for (int y = 0; y < boardSize; y++)
        {
            for (int x = 0; x < boardSize; x++)
            {
                if (y <= 1)
                {
                    boardPieces[x, y] = new Piece_Data(true, new Vector2Int(x, y) );
                }
                else if (y >= boardSize - 2)
                {
                    boardPieces[x, y] = new Piece_Data(false, new Vector2Int(x, y));
                }
                else boardPieces[x,y] = null;
            }
        }
    }

    public Piece_Data GetPiece(int x, int y)
    {
        return boardPieces[x,y];
    }

    public bool MovePiece(Piece_Data piece, int moveX, int moveY)
    {
        int newXPos = piece.positionOnBoard.x + moveX;
        int newYPos = piece.positionOnBoard.y + moveY;

        if (ValidMove(newXPos, newYPos) == false) return false;

        boardPieces[newXPos,newYPos] = piece;
        boardPieces[piece.positionOnBoard.x, piece.positionOnBoard.y] = null;
        piece.positionOnBoard = new Vector2Int(newXPos, newYPos);


        return true;
    }

    private bool ValidMove(int newXPos, int newYPos)
    {
        if (newXPos > boardSize || newXPos < 0) return false;
        if (newYPos > boardSize || newYPos < 0) return false;

        if (boardPieces[newXPos, newYPos] == null) {
            return true;
        }
        else {
            return false;
        }

    }

}

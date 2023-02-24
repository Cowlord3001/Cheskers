using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board_Data
{
    Piece_Data[,] boardPieces;
    int boardSize;

    const int MOVING_TO_EMPTY = 0;
    const int MOVING_TO_ENEMYPIECE = 1;
    const int MOVING_TO_ILLIGAL_SPACE = 2;

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
                    boardPieces[x, y] = new Piece_Data(Piece_Data.Color.white, new Vector2Int(x, y) );
                }
                else if (y >= boardSize - 2)
                {
                    boardPieces[x, y] = new Piece_Data(Piece_Data.Color.black, new Vector2Int(x, y));
                }
                else boardPieces[x,y] = null;
            }
        }
    }

    public Piece_Data GetPiece(int x, int y)
    {
        return boardPieces[x,y];
    }

    public List<Vector2Int> getAllLegalMoves(Piece_Data piece, Move[] moves, SlidingMove[] slidingmoves)
    {
        List<Vector2Int> legalMoves = new List<Vector2Int>();

        //Handle Preset Moves
        foreach (Move move in moves) {
            int newXPos = piece.positionOnBoard.x + move.changeInX;
            int newYPos = piece.positionOnBoard.y + move.changeInY;

            int moveCheckResult = ValidMove(piece.getColor(), newXPos, newYPos);

            if (move.requiresTargetPiece == false) {
                if (moveCheckResult != MOVING_TO_ILLIGAL_SPACE) {
                    legalMoves.Add(new Vector2Int(newXPos, newYPos));
                }
            }
            else {
                if(moveCheckResult  == MOVING_TO_ENEMYPIECE) {

                    legalMoves.Add(new Vector2Int(newXPos, newYPos));
                }
            }
        }

        //Sliding Move
        foreach(SlidingMove move in slidingmoves) {
            //TODO: Calculate all possible positiions to slide to.
        }

        return legalMoves;
    }

    public bool MovePiece(Piece_Data piece, int moveX, int moveY)
    {
        int newXPos = piece.positionOnBoard.x + moveX;
        int newYPos = piece.positionOnBoard.y + moveY;

        int moveCheckResult = ValidMove(piece.getColor(), newXPos, newYPos);

        if (moveCheckResult == MOVING_TO_ILLIGAL_SPACE) return false;

        if (moveCheckResult == MOVING_TO_ENEMYPIECE) {
            //TODO: Trigger event that says a piece taken
        }

        boardPieces[newXPos,newYPos] = piece;
        boardPieces[piece.positionOnBoard.x, piece.positionOnBoard.y] = null;
        piece.positionOnBoard = new Vector2Int(newXPos, newYPos);


        return true;
    }

    private int ValidMove(Piece_Data.Color color, int newXPos, int newYPos)
    {
        if (newXPos > boardSize || newXPos < 0) return MOVING_TO_ILLIGAL_SPACE;
        if (newYPos > boardSize || newYPos < 0) return MOVING_TO_ILLIGAL_SPACE;

        if (boardPieces[newXPos, newYPos] == null) {
            return MOVING_TO_EMPTY;
        }
        else if(boardPieces[newXPos, newYPos].getColor() != color) {
            return MOVING_TO_ENEMYPIECE;
        }
        else {
            return MOVING_TO_ILLIGAL_SPACE;
        }

    }

}

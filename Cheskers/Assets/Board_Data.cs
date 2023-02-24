using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.tvOS;

public class Board_Data
{
    Piece_Data[,] boardPieces;
    int boardSize;

    //Event for when pieces are removed from board in case we want some effect to play in another script
    public event EventHandler<PieceRemovedEventArgs> OnPieceRemovedFromBoard;
    public class PieceRemovedEventArgs : EventArgs {
        public Piece_Data removedPiece;
    }

    //Event for when a piece is damaged in case we want some effect to play in another script.
    public event EventHandler<PieceDamageEventArgs> OnPieceDamaged;
    public class PieceDamageEventArgs : EventArgs
    {
        public Piece_Data damagedPiece;
    }

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

            int moveValidateResult = ValidMove(piece.getColor(), newXPos, newYPos);

            if (move.requiresTargetPiece == false) {
                if (moveValidateResult != MOVING_TO_ILLIGAL_SPACE) {
                    legalMoves.Add(new Vector2Int(newXPos, newYPos));
                }
            }
            else {
                if(moveValidateResult  == MOVING_TO_ENEMYPIECE) {

                    legalMoves.Add(new Vector2Int(newXPos, newYPos));
                }
            }
        }

        //Sliding Move
        foreach(SlidingMove move in slidingmoves) {
            int moveValidateResult = -1;

            int x = piece.positionOnBoard.x;
            int endX = piece.positionOnBoard.x + move.changeInX;
            int y = piece.positionOnBoard.y;
            int endY = piece.positionOnBoard.x + move.changeInY;

            while (x != endX && y != endY) {
                //Normalize move to -1 or 1
                //Could probably just use -1 and 1 in the slidingmove rules
                //However what if some affect halfs movement???
                x += move.changeInX/Mathf.Abs(move.changeInX); 
                y += move.changeInY/Mathf.Abs(move.changeInY);

                //Validate position
                moveValidateResult = ValidMove(piece.getColor(), x, y);

                //If move is not illegal
                if (moveValidateResult == MOVING_TO_EMPTY) {
                    legalMoves.Add(new Vector2Int(x, y));
                }
                else if(moveValidateResult == MOVING_TO_ENEMYPIECE) {
                    //Movement Stops if you reach an enemy but move is still added
                    legalMoves.Add(new Vector2Int(x, y));
                    break;
                }
                else {
                    //One illegal move stops the entire slide.
                    break;
                }
            }

         }
        return legalMoves;
    }

    public bool MovePiece(Piece_Data piece, int moveX, int moveY)
    {
        int newXPos = piece.positionOnBoard.x + moveX;
        int newYPos = piece.positionOnBoard.y + moveY;

        int moveValidateResult = ValidMove(piece.getColor(), newXPos, newYPos);

        //If move is illigal return false for failed move
        if (moveValidateResult == MOVING_TO_ILLIGAL_SPACE) return false;

        //Moving to enemy piece it should be damaged or removed
        if (moveValidateResult == MOVING_TO_ENEMYPIECE) {

            if(boardPieces[newXPos, newYPos].IsDamaged == true) {
                RemovePiece(piece, newXPos, newYPos);
            }
            else {
                //Roll to see if piece is damage or removed
                int coinFlip = UnityEngine.Random.Range(0, 2);
                if(coinFlip == 0) {
                    RemovePiece(piece, newXPos, newYPos);
                }
                else {
                    //Damage Piece and ?Don't move?
                    boardPieces[newXPos, newYPos].IsDamaged = true;
                    
                    //Fire the piece damaged event
                    PieceDamageEventArgs e = new PieceDamageEventArgs();
                    e.damagedPiece = boardPieces[newXPos, newYPos];
                    OnPieceDamaged?.Invoke(this, e);
                }
            }
        }


        return true;
    }

    private void RemovePiece(Piece_Data pieceThatisTaking, int removeX, int removeY)
    {
        //Fire Remove Piece Event
        PieceRemovedEventArgs e = new PieceRemovedEventArgs();
        e.removedPiece = boardPieces[removeX, removeY];
        OnPieceRemovedFromBoard?.Invoke(this, e);

        //Actually remove the piece
        boardPieces[removeX, removeY] = pieceThatisTaking;
        boardPieces[pieceThatisTaking.positionOnBoard.x, pieceThatisTaking.positionOnBoard.y] = null;
        pieceThatisTaking.positionOnBoard = new Vector2Int(removeX, removeY);

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

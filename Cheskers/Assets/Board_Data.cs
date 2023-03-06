using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board_Data
{
    //Singleton
    public static Board_Data instance;

    public string debugMessage;
    public Piece_Data[,] boardPieces { get; private set; }
    public int size { get; private set; }

    //Event for when pieces are removed from board in case we want some effect to play in another script
    public event EventHandler<EventArgsPieceRemoved> OnPieceRemovedFromBoard;
    public class EventArgsPieceRemoved : EventArgs {
        public Piece_Data removedPiece;
    }

    //Event for when a piece is damaged in case we want some effect to play in another script.
    public event EventHandler<EventArgsPieceDamaged> OnPieceDamaged;
    public class EventArgsPieceDamaged : EventArgs
    {
        public Piece_Data damagedPiece;
    }
    //Add Event for when piece is moved
    public event EventHandler<EventArgsPieceMoved> OnPieceMoved;
    public class EventArgsPieceMoved : EventArgs
    {
        public Vector2Int pieceStartBoardCordintaes;
        public Vector2Int pieceEndBoardCordintaes;
    }

    //TODO: Add Event for piece moved

    const int MOVING_TO_EMPTY = 0;
    const int MOVING_TO_ENEMYPIECE = 1;
    const int MOVING_TO_ILLIGAL_SPACE = 2;

    public Board_Data()
    {
        debugMessage = "";
        size = 8;
        boardPieces = new Piece_Data[size, size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                if (y <= 1)
                {
                    boardPieces[x, y] = new Piece_Data(Piece_Data.Color.white, new Vector2Int(x, y));
                }
                else if (y >= size - 2)
                {
                    boardPieces[x, y] = new Piece_Data(Piece_Data.Color.black, new Vector2Int(x, y));
                }
                else boardPieces[x,y] = null;
            }
        }
    }

    public List<Vector2Int> getAllLegalMoves(Piece_Data piece, Chess_Move_SO chessMoves)
    {
        List<Vector2Int> legalMoves = new List<Vector2Int>();

        if (chessMoves.pieceType == Piece_Data.Type.pawn) {
            List<Vector2Int> extraPawnMoves = GetExtraPawnMoves(piece);

            if(extraPawnMoves.Count > 0) {
                foreach (Vector2Int move in extraPawnMoves) {
                    legalMoves.Add(move);
                }
            }
        }

        //Handle Preset Moves
        foreach (Move move in chessMoves.moves) {
            int newXPos = piece.positionOnBoard.x + move.changeInX;
            int newYPos = piece.positionOnBoard.y + move.changeInY;


            int moveValidateResult = ValidMove(piece.GetColor(), newXPos, newYPos);


            //Pawn Logic! They can only move in one direction so skip move if its
            //The wrong direction. Can't move backwards
            if (chessMoves.pieceType == Piece_Data.Type.pawn) {
                if (piece.GetColor() == Piece_Data.Color.white && move.changeInY < 0) {
                    continue;
                }
                else if (piece.GetColor() == Piece_Data.Color.black && move.changeInY > 0) {
                    continue;
                }
                //Only Pawns need this extra check but some checkers rules need this too???
                if (move.requiresTargetPiece == false) {
                    if (moveValidateResult == MOVING_TO_EMPTY) {
                        legalMoves.Add(new Vector2Int(newXPos, newYPos));
                    }
                }
                else {
                    if (moveValidateResult == MOVING_TO_ENEMYPIECE) {
                        legalMoves.Add(new Vector2Int(newXPos, newYPos));
                    }
                }
            }
            else {
                //Not a pawn
                if (moveValidateResult != MOVING_TO_ILLIGAL_SPACE) {
                    legalMoves.Add(new Vector2Int(newXPos, newYPos));
                }
            }

        }

        //Sliding Move
        foreach(SlidingMove move in chessMoves.slidingMoves) {
            int moveValidateResult;
            int x = piece.positionOnBoard.x;
            int endX = piece.positionOnBoard.x + move.changeInX;
            int y = piece.positionOnBoard.y;
            int endY = piece.positionOnBoard.x + move.changeInY;
            while (x != endX || y != endY) {
                //Normalize move to -1 or 1
                //Could probably just use -1 and 1 in the slidingmove rules
                //However what if some effect causes half movement???
                if(Mathf.Abs(move.changeInX) > 0)
                    x += move.changeInX/Mathf.Abs(move.changeInX); 
                if(Mathf.Abs(move.changeInY) > 0)
                    y += move.changeInY/Mathf.Abs(move.changeInY);

                //Validate position
                moveValidateResult = ValidMove(piece.GetColor(), x, y);

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
                    //One illegal move stops the rest of the slide.
                    break;
                }
            }

         }
        return legalMoves;
    }

    List<Vector2Int> GetExtraPawnMoves(Piece_Data piece)
    {
        List<Vector2Int> ExtraMoves = new List<Vector2Int>();

        if(piece.hasMoved == false) {
            int newXpos = piece.positionOnBoard.x;
            int newYpos;
            if (piece.GetColor() == Piece_Data.Color.white) {
                newYpos = piece.positionOnBoard.y + 2;
            }
            else {
                newYpos = piece.positionOnBoard.y - 2;
            }

            int moveValidateResult1 = ValidMove(piece.GetColor(), newXpos, newYpos);
            int moveValidateResult2 = ValidMove(piece.GetColor(), newXpos, newYpos - 1);

            if (moveValidateResult1 == MOVING_TO_EMPTY && moveValidateResult2 == MOVING_TO_EMPTY) {
                ExtraMoves.Add(new Vector2Int(newXpos, newYpos));
            }
        }

        //TODO: Detect en pessant

        return ExtraMoves;
    }

    public void MoveAndDamage(Piece_Data piece, int newXPos, int newYPos)
    {
        if (boardPieces[newXPos,newYPos] == null) {
            MovePiece(piece, newXPos, newYPos);
            return;
        }
        if(boardPieces[newXPos, newYPos].IsDamaged == true) {
            MoveAndTake(piece, newXPos, newYPos);
            return;
        }

        if(piece.type == Piece_Data.Type.knight ||
           piece.type == Piece_Data.Type.pawn   ||
           piece.type == Piece_Data.Type.king) {
           DamagePiece(newXPos, newYPos);
        }
        else {
            //Calculate where to land.
            int deltaX = newXPos - piece.positionOnBoard.x;
            int deltaY = newYPos - piece.positionOnBoard.y;

            if(deltaX != 0)
                deltaX = deltaX / Mathf.Abs(deltaX);
            if(deltaY != 0)
                deltaY = deltaY / Mathf.Abs(deltaY);
            if(newXPos - deltaX == piece.positionOnBoard.x && newYPos - deltaY == piece.positionOnBoard.y)
            {

            }
            else
            {
                MovePiece(piece, newXPos - deltaX, newYPos - deltaY);
            }

            DamagePiece(newXPos, newYPos);

        }

    }

    public void DamagePiece(int pieceToDamagePositionX, int pieceToDamagePositionY)
    {
        boardPieces[pieceToDamagePositionX, pieceToDamagePositionY].IsDamaged = true;
        EventArgsPieceDamaged e = new EventArgsPieceDamaged();
        e.damagedPiece = boardPieces[pieceToDamagePositionX, pieceToDamagePositionY];
        OnPieceDamaged(this, e);
    }

    private void MovePiece(Piece_Data piece, int newXPos, int newYPos)
    {
        EventArgsPieceMoved e = new EventArgsPieceMoved();
        e.pieceStartBoardCordintaes = piece.positionOnBoard;
        e.pieceEndBoardCordintaes = new Vector2Int(newXPos, newYPos);
        OnPieceMoved?.Invoke(this, e);

        boardPieces[newXPos, newYPos] = piece;
        boardPieces[piece.positionOnBoard.x, piece.positionOnBoard.y] = null;
        piece.positionOnBoard = new Vector2Int(newXPos, newYPos);

    }

    public void MovePieceExternal(int oldXPos, int oldYPos, int newXPos, int newYPos)
    {
        if(boardPieces[oldXPos, oldYPos] == null) { return; }

        MovePiece(boardPieces[oldXPos, oldYPos], newXPos, newYPos);
    }

    public bool MoveAndTake(Piece_Data piece, int newXPos, int newYPos)
    {

        int moveValidateResult = ValidMove(piece.GetColor(), newXPos, newYPos);
        //If move is illigal return false for failed move shouldnt happen
        if (moveValidateResult == MOVING_TO_ILLIGAL_SPACE) return false;
        //Moving to enemy piece it should be damaged or removed

        piece.hasMoved = true;

        if (moveValidateResult == MOVING_TO_ENEMYPIECE) {
            RemovePiece(newXPos, newYPos);
            MovePiece(piece, newXPos, newYPos);
        }

        //Moving to EmptySpace
        if(moveValidateResult == MOVING_TO_EMPTY) {
            MovePiece(piece, newXPos, newYPos);
        }

        return true;
    }

    public void RemovePiece(int removeX, int removeY)
    {
        //Fire Remove Piece Event
        EventArgsPieceRemoved e = new EventArgsPieceRemoved();
        e.removedPiece = boardPieces[removeX, removeY];
        OnPieceRemovedFromBoard?.Invoke(this, e);

        boardPieces[removeX, removeY] = null;
    }

    private int ValidMove(Piece_Data.Color color, int newXPos, int newYPos)
    {
        if (newXPos > size - 1 || newXPos < 0) return MOVING_TO_ILLIGAL_SPACE;
        if (newYPos > size - 1 || newYPos < 0) return MOVING_TO_ILLIGAL_SPACE;

        if (boardPieces[newXPos, newYPos] == null) {
            return MOVING_TO_EMPTY;
        }
        else if(boardPieces[newXPos, newYPos].GetColor() != color) {
            return MOVING_TO_ENEMYPIECE;
        }
        else {
            return MOVING_TO_ILLIGAL_SPACE;
        }

    }

}

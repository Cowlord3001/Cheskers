using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.tvOS;

public class Board_Data
{
    public static string debugMessage;
    public Piece_Data[,] boardPieces { get; private set; }
    public int size { get; private set; }

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
        debugMessage = "";
        size = 8;
        boardPieces = new Piece_Data[size, size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                if (y <= 1)
                {
                    boardPieces[x, y] = new Piece_Data(Piece_Data.Color.white, new Vector2Int(x, y) );
                }
                else if (y >= size - 2)
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


            int moveValidateResult = ValidMove(piece.getColor(), newXPos, newYPos);


            //Pawn Logic! They can only move in one direction so skip move if its
            //The wrong direction.
            if (chessMoves.pieceType == Piece_Data.Type.pawn) {
                if(piece.getColor() == Piece_Data.Color.white && move.changeInY < 0) {
                    continue;
                }
                else if(piece.getColor() == Piece_Data.Color.black && move.changeInY > 0) {
                    continue;
                }
            }

            //Only Pawns need this extra check but some checkers rules need this too???
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
        int i = 0;
        //Sliding Move
        foreach(SlidingMove move in chessMoves.slidingMoves) {
            int moveValidateResult = -1;
            i++;
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
            if (piece.getColor() == Piece_Data.Color.white) {
                newYpos = piece.positionOnBoard.y + 2;
            }
            else {
                newYpos = piece.positionOnBoard.y - 2;
            }

            int moveValidateResult = ValidMove(piece.getColor(), newXpos, newYpos);

            if(moveValidateResult == MOVING_TO_EMPTY) {
                ExtraMoves.Add(new Vector2Int(newXpos, newYpos));
            }
        }


        return ExtraMoves;
    }

    public bool MovePiece(Piece_Data piece, int newXPos, int newYPos)
    {

        int moveValidateResult = ValidMove(piece.getColor(), newXPos, newYPos);

        //If move is illigal return false for failed move shouldnt happen
        if (moveValidateResult == MOVING_TO_ILLIGAL_SPACE) return false;
        //Moving to enemy piece it should be damaged or removed

        piece.hasMoved = true;

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

        //Moving to EmptySpace
        if(moveValidateResult == MOVING_TO_EMPTY) {
            boardPieces[newXPos, newYPos] = piece;
            boardPieces[piece.positionOnBoard.x, piece.positionOnBoard.y] = null;
            piece.positionOnBoard = new Vector2Int(newXPos, newYPos);
        }

        return true;
    }

    public Vector2 BoardIndextoWorld(int boardPositionx, int boardPositiony)
    {
        Vector2 worldPosition = Vector2.zero;
        float halfWidth = size / 2;
        float halfTileSize = .5f;
        worldPosition.x = boardPositionx - halfWidth + halfTileSize;
        worldPosition.y = boardPositiony - halfWidth + halfTileSize;

        return worldPosition;
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
        if (newXPos > size - 1 || newXPos < 0) return MOVING_TO_ILLIGAL_SPACE;
        if (newYPos > size - 1 || newYPos < 0) return MOVING_TO_ILLIGAL_SPACE;

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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardFunction_MoveAndDamage
{
    public void MoveAndDamage(Piece piece, Vector2Int newPosition)
    {
        if (Board.instance.boardPieces[newPosition] == null) {
            Board.instance.MovePiece(piece, newPosition);
            Board.instance.ResetPieceType(piece);
            return;
        }
        if (Board.instance.boardPieces[newPosition].health == 1) {
            Board.instance.MoveAndTake(piece, newPosition);
            Board.instance.ResetPieceType(piece);
            return;
        }
        if (piece.type == Piece.Type.knight ||
            piece.type == Piece.Type.pawn ||
            piece.type == Piece.Type.king) {
            CallDamagePiece(piece, newPosition);
        }
        else {
            //Calculate where to land.
            Vector2Int deltaPosition = newPosition - piece.positionOnBoard;
            if (deltaPosition.x != 0)
                deltaPosition.x = deltaPosition.x / Mathf.Abs(deltaPosition.x);
            if (deltaPosition.y != 0)
                deltaPosition.y = deltaPosition.y / Mathf.Abs(deltaPosition.y);
            if (newPosition - deltaPosition == piece.positionOnBoard) {

            }
            else {
                Board.instance.MovePiece(piece, newPosition - deltaPosition);
            }

            CallDamagePiece(piece, newPosition);
        }
        Board.instance.ResetPieceType(piece);
    }

    protected virtual void CallDamagePiece(Piece piece, Vector2Int targetPiecePosition)
    {
        Board.instance.DamagePiece(targetPiecePosition);
    }

}

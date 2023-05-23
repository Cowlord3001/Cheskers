using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardFunction_MoveAndDamage
{
    public void MoveAndDamage(Piece_Data piece, Vector2Int newPosition)
    {
        if (Board_Data.instance.boardPieces[newPosition] == null) {
            Board_Data.instance.MovePiece(piece, newPosition);
            Board_Data.instance.ResetPieceType(piece);
            return;
        }
        if (Board_Data.instance.boardPieces[newPosition].health == 1) {
            Board_Data.instance.MoveAndTake(piece, newPosition);
            Board_Data.instance.ResetPieceType(piece);
            return;
        }
        if (piece.type == Piece_Data.Type.knight ||
            piece.type == Piece_Data.Type.pawn ||
            piece.type == Piece_Data.Type.king) {
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
                Board_Data.instance.MovePiece(piece, newPosition - deltaPosition);
            }

            CallDamagePiece(piece, newPosition);
        }
        Board_Data.instance.ResetPieceType(piece);
    }

    protected virtual void CallDamagePiece(Piece_Data piece, Vector2Int targetPiecePosition)
    {
        Board_Data.instance.DamagePiece(targetPiecePosition);
    }

}

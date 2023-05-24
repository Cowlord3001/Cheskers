using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardFunction_MoveAndTake
{
    public void MoveAndTake(Piece piece, Vector2Int newPosition)
    {
        int moveValidateResult = Board.instance.ValidMove(piece.GetColor(), newPosition);
        //If move is illigal return false for failed move shouldnt happen
        if (moveValidateResult == Board.instance.MOVING_TO_ILLIGAL_SPACE) return;

        piece.hasMoved = true;

        if (moveValidateResult == Board.instance.MOVING_TO_ENEMYPIECE) {
            CallRemovePiece(piece, newPosition);
            Board.instance.MovePiece(piece, newPosition);
        }

        //Moving to EmptySpace
        if (moveValidateResult == Board.instance.MOVING_TO_EMPTY) {
            Board.instance.MovePiece(piece, newPosition);
        }
        Board.instance.ResetPieceType(piece);
    }

    protected virtual void CallRemovePiece(Piece piece, Vector2Int newPosition)
    {
        Board.instance.RemovePiece(newPosition);
    }
}

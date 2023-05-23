using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardFunction_MoveAndTake
{
    public void MoveAndTake(Piece_Data piece, Vector2Int newPosition)
    {
        int moveValidateResult = Board_Data.instance.ValidMove(piece.GetColor(), newPosition);
        //If move is illigal return false for failed move shouldnt happen
        if (moveValidateResult == Board_Data.instance.MOVING_TO_ILLIGAL_SPACE) return;

        piece.hasMoved = true;

        if (moveValidateResult == Board_Data.instance.MOVING_TO_ENEMYPIECE) {
            CallRemovePiece(piece, newPosition);
            Board_Data.instance.MovePiece(piece, newPosition);
        }

        //Moving to EmptySpace
        if (moveValidateResult == Board_Data.instance.MOVING_TO_EMPTY) {
            Board_Data.instance.MovePiece(piece, newPosition);
        }
        Board_Data.instance.ResetPieceType(piece);
    }

    protected virtual void CallRemovePiece(Piece_Data piece, Vector2Int newPosition)
    {
        Board_Data.instance.RemovePiece(newPosition);
    }
}

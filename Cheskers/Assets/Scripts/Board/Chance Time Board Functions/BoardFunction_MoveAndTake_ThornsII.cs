using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardFunction_MoveAndTake_ThornsII : BoardFunction_MoveAndTake
{
    protected override void CallRemovePiece(Piece piece, Vector2Int newPosition)
    {
        Board.instance.RemovePiece(newPosition);
        Board.instance.DamagePiece(piece.positionOnBoard);
    }
}

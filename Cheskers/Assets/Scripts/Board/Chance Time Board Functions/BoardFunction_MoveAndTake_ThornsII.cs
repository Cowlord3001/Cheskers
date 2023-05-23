using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardFunction_MoveAndTake_ThornsII : BoardFunction_MoveAndTake
{
    protected override void CallRemovePiece(Piece_Data piece, Vector2Int newPosition)
    {
        Board_Data.instance.RemovePiece(newPosition);
        Board_Data.instance.DamagePiece(piece.positionOnBoard);
    }
}

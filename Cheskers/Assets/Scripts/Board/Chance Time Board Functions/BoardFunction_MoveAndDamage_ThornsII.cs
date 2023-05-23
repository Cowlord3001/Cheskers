using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardFunction_MoveAndDamage_ThornsII : BoardFunction_MoveAndDamage
{
    protected override void CallDamagePiece(Piece_Data piece, Vector2Int targetPiecePosition)
    {
        Board_Data.instance.DamagePiece(targetPiecePosition);
        Board_Data.instance.DamagePiece(piece.positionOnBoard);
    }
}

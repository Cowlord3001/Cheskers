using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardFunction_MoveAndDamage_ThornsII : BoardFunction_MoveAndDamage
{
    protected override void CallDamagePiece(Piece piece, Vector2Int targetPiecePosition)
    {
        Board.instance.DamagePiece(targetPiecePosition);
        Board.instance.DamagePiece(piece.positionOnBoard);
    }
}

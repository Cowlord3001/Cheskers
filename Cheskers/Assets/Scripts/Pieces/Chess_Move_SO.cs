using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "ChessMoveSet")]
public class Chess_Move_SO : ScriptableObject
{
    public Piece.Type pieceType;

    public Sprite spriteBlackNoDamage;
    public Sprite spriteBlackDamaged;
    public Sprite spriteWhiteNoDamage;
    public Sprite spriteWhiteDamaged;

    public Move[] moves;
    public SlidingMove[] slidingMoves;

    
}

[System.Serializable]
public struct Move
{
    public bool requiresTargetPiece;
    public int changeInX;
    public int changeInY;

}

[System.Serializable]
public struct SlidingMove
{
    public int changeInX;
    public int changeInY;
    public Vector2Int changeInPosition;
}

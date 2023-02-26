using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ChessMoveSet")]
public class Chess_Move_SO : ScriptableObject
{
    public Piece_Data.Type pieceType;

    public Sprite spriteBlackNoDamage;
    public Sprite spriteBlackDamaged;
    public Sprite spriteWhiteNoDamage;
    public Sprite spriteWhiteDamaged;

    public Move[] moves;
    public SlidingMove[] slidingMoves;

    public Sprite GetSprite(Piece_Data.Color color, bool isDamaged)
    {
        switch (color) {
            case Piece_Data.Color.white:
                if (isDamaged) return spriteWhiteDamaged;
                else return spriteWhiteNoDamage;
            case Piece_Data.Color.black:
                if (isDamaged) return spriteBlackDamaged;
                else return spriteBlackNoDamage;
            default:
                return null;
        }
    }

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
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ChessMoveSet")]
public class Chess_Move_SO : ScriptableObject
{
    public Piece_Data.State chessPiece;

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
}

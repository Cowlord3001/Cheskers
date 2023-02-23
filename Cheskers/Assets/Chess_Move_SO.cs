using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ChessMoveSet")]
public class Chess_Move_SO : ScriptableObject
{
    public Piece_Data.State chessPiece;

    public Move[] moves;


}

[System.Serializable]
public struct Move
{
    public bool requiresPiece;
    public int changeInX;
    public int changeInY;


    public bool requiresRow;
    public int[] rowRequired;
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class  Piece_Data
{
    public GameObject gameObject;
    public enum Type {pawn, bishop, knight, rook, queen, king};
    Type state;
    public enum Color { white, black };
    Color color;
    bool IsWhite;
    bool IsKing;
    public bool IsDamaged;
    
    public Vector2Int positionOnBoard;

    public Color getColor()
    {
        return color;
    }

    public Piece_Data(Color color, Vector2Int positionOnBoard)
    {
        this.positionOnBoard = positionOnBoard;
        this.color = color;
        IsKing = false;
        IsDamaged = false;
    }
}

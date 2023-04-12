using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class  Piece_Data
{
    public GameObject gameObject;
    public Vector2Int positionOnBoard;
    public enum Type {pawn, bishop, knight, rook, queen, king, none};
    public Type type = Type.none;
    public enum Color { white, black };
    Color color;
    
    public bool IsVIP;//Not Used Yet
    public int health;
    public bool hasMoved;
    public Color GetColor()
    {
        return color;
    }

    public Piece_Data(Color color, Vector2Int positionOnBoard)
    {
        this.positionOnBoard = positionOnBoard;
        this.color = color;
        IsVIP = false;
        health = 2;
    }
}

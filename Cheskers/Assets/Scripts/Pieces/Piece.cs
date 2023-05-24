using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece
{
    //public GameObject gameObject;
    public Vector2Int positionOnBoard;
    public enum Type {pawn, bishop, knight, rook, queen, king, none};
    public Type type = Type.none;
    public enum Color { white, black };
    Color color;

    //Reset at the start of your turn only
    public bool enPassantTag = false;
    public bool kingTag = false;
    public bool rookTag = false;

    public bool IsVIP;//Not Used Yet
    public int health;
    public bool hasMoved;
    public Color GetColor()
    {
        return color;
    }

    public Piece(Color color, Vector2Int positionOnBoard)
    {
        this.positionOnBoard = positionOnBoard;
        this.color = color;
        IsVIP = false;
        health = 2;
    }

    const char WHITE = 'W';
    const char VIP = 'V';
    const char DAMAGED = 'D';
    const char MOVED = 'M';
    const char NOT = 'N';
    //WVDM white piece that has been damaged and moved
    //WNNN white piece that is not the VIP and has not moved and has not been damaged
    public Piece(string stringCode, Vector2Int positionOnBoard)
    {
        this.positionOnBoard = positionOnBoard;

        if (stringCode[0] == WHITE) color = Color.white;
        else color = Color.black;

        if (stringCode[1] == VIP) IsVIP = true;
        else IsVIP = false;

        if (stringCode[2] == DAMAGED) health = 2;
        else health = 1;

        if (stringCode[3] == MOVED) hasMoved = true;
        else hasMoved = false;
    }
    public string GetPieceString()
    {
        string pieceStringCode = "";

        if (color == Color.white) pieceStringCode += WHITE;
        else pieceStringCode += NOT;

        if (IsVIP) pieceStringCode += VIP;
        else pieceStringCode += NOT;

        if(health < 2) pieceStringCode += DAMAGED; 
        else pieceStringCode += NOT;

        if(hasMoved) pieceStringCode += MOVED;
        else pieceStringCode += NOT;

        return pieceStringCode;
    }
}

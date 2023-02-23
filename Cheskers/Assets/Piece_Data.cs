using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class  Piece_Data
{
    enum State {pawn, bishop, knight, rook, queen, king};
    State state;
    bool IsWhite;
    bool IsKing;
    bool IsDamaged;

    public Piece_Data(bool IsWhite)
    {
        this.IsWhite = IsWhite;
        IsKing = false;
        IsDamaged = false;
    }
}

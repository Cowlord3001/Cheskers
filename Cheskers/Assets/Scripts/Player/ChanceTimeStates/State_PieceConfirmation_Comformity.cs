using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State_PieceConfirmation_Comformity : State_PieceConfirmation
{
    Piece.Type pieceType;

    public State_PieceConfirmation_Comformity(Turn_Manager controller, Piece.Type pieceType) : base(controller)
    {
        this.pieceType = pieceType;
    }

    public override void RunState()
    {
        base.RunState();
    }

    protected override Chess_Move_SO PickChessMove()
    {
        return PieceDisplay_Manager.instance.GetChessMoveByType(pieceType);
    }

}

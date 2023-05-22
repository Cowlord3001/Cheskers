using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State_PieceConfirmation_Comformity : State_PieceConfirmation
{
    Piece_Data.Type pieceType;

    public State_PieceConfirmation_Comformity(Piece_Controller controller, Piece_Data.Type pieceType) : base(controller)
    {
        this.pieceType = pieceType;
    }

    public override void RunState()
    {
        base.RunState();
    }

    protected override Chess_Move_SO PickChessMove()
    {
        return Piece_Display.instance.GetChessMoveByType(pieceType);
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State_PieceConfirmation_PieceOverride : State_PieceConfirmation
{
    public State_PieceConfirmation_PieceOverride(Turn_Manager pieceController, Piece.Type type) : base(pieceController) { 
        this.type = type; 
    }

    Piece.Type type;

    protected override bool ValidatePieceSelection()
    {
        if (pieceController.SelectedPiece != null) {
            pieceController.RemoveHighlightOnSelectedPiece();
            return true;
        }
        else {
            return false;
        }
    }

    protected override Chess_Move_SO PickChessMove()
    {
        return PieceDisplay_Manager.instance.GetChessMoveByType(type);
    }

}

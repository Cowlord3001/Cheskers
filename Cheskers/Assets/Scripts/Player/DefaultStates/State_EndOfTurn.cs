using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Turn_Manager;

public class State_EndOfTurn : PlayerTurnState
{
    public State_EndOfTurn(Turn_Manager pieceController) : base(pieceController) { }

    public override void RunState()
    {
        pieceController.Rerolled = false;
        pieceController.SetPhaseInTurn( PhaseInTurn.WAITING_FOR_TURN );
        pieceController.SelectedPiece.type = Piece.Type.none;

        if(pieceController.PreviouslySelectedPiece != null)
            pieceController.PreviouslySelectedPiece.type = Piece.Type.none;

        pieceController.UpdateBoardAndPieceGraphics();
        pieceController.SendEndOfTurnEvent();
    }
}

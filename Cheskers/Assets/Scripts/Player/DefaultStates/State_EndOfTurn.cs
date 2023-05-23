using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Piece_Controller;

public class State_EndOfTurn : PlayerTurnState
{
    public State_EndOfTurn(Piece_Controller pieceController) : base(pieceController) { }

    public override void RunState()
    {
        pieceController.Rerolled = false;
        pieceController.SetPhaseInTurn( PhaseInTurn.WAITING_FOR_TURN );
        pieceController.SelectedPiece.type = Piece_Data.Type.none;

        if(pieceController.PreviouslySelectedPiece != null)
            pieceController.PreviouslySelectedPiece.type = Piece_Data.Type.none;

        pieceController.UpdateBoardAndPieceGraphics();
        pieceController.SendEndOfTurnEvent();
    }
}
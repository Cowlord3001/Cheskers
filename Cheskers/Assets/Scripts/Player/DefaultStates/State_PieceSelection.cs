using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Piece_Controller;

public class State_PieceSelection : PlayerTurnState
{
    public State_PieceSelection(Piece_Controller pieceController) : base(pieceController) { }

    public override void RunState()
    {
        Piece_Data piece = Piece_Detection.GetPieceUnderMouse();
        if (piece != null) {
            //DEV COMMAND USED
            if (piece.GetColor() != pieceController.GetColor() && Input_Controller.developerCommandsEnabled == false) { return; }
            pieceController.SelectedPiece = piece;

            //Debug.Log(selectedPiece.gameObject.name);
            pieceController.HighLightSelectedPiece();

            pieceController.SetPhaseInTurn(PhaseInTurn.PIECE_CONFIRMATION);
            //Happens on click, Game automatically advances
        }
    }
}

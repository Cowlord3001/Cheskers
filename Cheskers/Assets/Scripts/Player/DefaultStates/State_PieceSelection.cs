using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Turn_Manager;

public class State_PieceSelection : PlayerTurnState
{
    public State_PieceSelection(Turn_Manager pieceController) : base(pieceController) { }

    public override void RunState()
    {
        Piece piece = Piece_Detection.GetPieceUnderMouse();
        if (piece != null) {
            //DEV COMMAND USED
            if (piece.GetColor() != pieceController.GetColor() && Input_Manager.developerCommandsEnabled == false) { return; }
            pieceController.SelectedPiece = piece;

            //Debug.Log(selectedPiece.gameObject.name);
            pieceController.HighLightSelectedPiece();

            pieceController.SetPhaseInTurn(PhaseInTurn.PIECE_CONFIRMATION);
            //Happens on click, Game automatically advances
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Piece_Controller;

public class State_DecideMoveOrReroll : PlayerTurnState
{
    public State_DecideMoveOrReroll(Piece_Controller pieceController) : base(pieceController) { }

    public override void RunState()
    {
        pieceController.DecidedMove = Input_Controller.instance.mouseBoardPosition;
        foreach (var move in pieceController.ValidMoves) {
            if (pieceController.DecidedMove == move) {
                //Starts the first contest
                pieceController.SetPhaseInTurn( PhaseInTurn.CONTEST );
            }
        }

        pieceController.AdvanceGame();
    }
}

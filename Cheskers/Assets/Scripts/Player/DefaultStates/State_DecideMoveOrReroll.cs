using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Turn_Manager;

public class State_DecideMoveOrReroll : PlayerTurnState
{
    public State_DecideMoveOrReroll(Turn_Manager pieceController) : base(pieceController) { }

    public override void RunState()
    {
        pieceController.DecidedMove = Input_Manager.instance.mouseBoardPosition;
        foreach (var move in pieceController.ValidMoves) {
            if (pieceController.DecidedMove == move) {
                //Starts the first contest
                pieceController.SetPhaseInTurn( PhaseInTurn.CONTEST );
                pieceController.AdvanceGame();
            }
        }

    }
}

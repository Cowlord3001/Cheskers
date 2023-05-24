using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Turn_Manager;

public class State_Contest : PlayerTurnState
{
    public State_Contest(Turn_Manager pieceController) : base(pieceController) { }

    public override void RunState()
    {
        if (Board.instance.boardPieces[pieceController.DecidedMove] == null ||
            Board.instance.boardPieces[pieceController.DecidedMove].health == 1) {
            //Skip Contest Phase
            pieceController.CoinFlip = UnityEngine.Random.Range(0, 2);
            pieceController.SetPhaseInTurn(PhaseInTurn.MOVE_AND_UPDATE);
            pieceController.AdvanceGame();
            return;
        }

        if (Token_Manager.instance.WhitePlayerHasTokens() == false &&
            Token_Manager.instance.BlackPlayerHasTokens() == false) {
            pieceController.CoinFlip = UnityEngine.Random.Range(0, 2);
            pieceController.SetPhaseInTurn(PhaseInTurn.MOVE_AND_UPDATE);
            pieceController.AdvanceGame();
            return;
        }

        //Contest code moved to network controller and single player controller
        pieceController.SendContestEvent();
    }
}

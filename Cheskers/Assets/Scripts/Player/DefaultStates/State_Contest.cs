using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Piece_Controller;

public class State_Contest : PlayerTurnState
{
    public State_Contest(Piece_Controller pieceController) : base(pieceController) { }

    public override void RunState()
    {
        if (Board_Data.instance.boardPieces[pieceController.DecidedMove.x, pieceController.DecidedMove.y] == null ||
            Board_Data.instance.boardPieces[pieceController.DecidedMove.x, pieceController.DecidedMove.y].health == 1) {
            //Skip Contest Phase
            pieceController.CoinFlip = UnityEngine.Random.Range(0, 2);
            pieceController.SetPhaseInTurn(PhaseInTurn.MOVE_AND_UPDATE);
            pieceController.AdvanceGame();
            return;
        }

        if (Input_Controller.instance.WhitePlayerHasTokens() == false &&
            Input_Controller.instance.BlackPlayerHasTokens() == false) {
            pieceController.CoinFlip = UnityEngine.Random.Range(0, 2);
            pieceController.SetPhaseInTurn(PhaseInTurn.MOVE_AND_UPDATE);
            pieceController.AdvanceGame();
            return;
        }

        //Contest code moved to network controller and single player controller
        pieceController.SendContestEvent();
    }
}

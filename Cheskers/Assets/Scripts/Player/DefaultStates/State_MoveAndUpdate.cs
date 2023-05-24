using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using static Turn_Manager;

public class State_MoveAndUpdate : PlayerTurnState
{
    public State_MoveAndUpdate(Turn_Manager pieceController) : base(pieceController) { }

    const int CAPTURE = 0;
    const int DAMAGE = 1;
    public override void RunState()
    {
        if (pieceController.CoinFlip == CAPTURE) {
            Board.instance.MoveAndTake(pieceController.SelectedPiece, pieceController.DecidedMove);
        }
        else if (pieceController.CoinFlip == DAMAGE) {
            Board.instance.MoveAndDamage(pieceController.SelectedPiece, pieceController.DecidedMove);
        }
        else {
            Debug.LogWarning("Error: Invalid Coin Flip");
        }
        pieceController.UpdateBoardAndPieceGraphics();
        pieceController.RemoveHighlightOnSelectedPiece();
        pieceController.SetPhaseInTurn(PhaseInTurn.END_OF_TURN);
        pieceController.AdvanceGame();
    }
}

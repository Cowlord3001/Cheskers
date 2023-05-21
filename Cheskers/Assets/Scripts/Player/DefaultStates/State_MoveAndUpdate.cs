using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using static Piece_Controller;

public class State_MoveAndUpdate : PlayerTurnState
{
    public State_MoveAndUpdate(Piece_Controller pieceController) : base(pieceController) { }

    const int CAPTURE = 0;
    const int DAMAGE = 1;
    public override void RunState()
    {
        if (pieceController.CoinFlip == CAPTURE) {
            Board_Data.instance.MoveAndTake(pieceController.SelectedPiece, pieceController.DecidedMove.x, pieceController.DecidedMove.y);
        }
        else if (pieceController.CoinFlip == DAMAGE) {
            Board_Data.instance.MoveAndDamage(pieceController.SelectedPiece, pieceController.DecidedMove.x, pieceController.DecidedMove.y);
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

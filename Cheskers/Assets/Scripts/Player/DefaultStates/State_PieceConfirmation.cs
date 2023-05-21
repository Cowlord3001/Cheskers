using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Piece_Controller;

public class State_PieceConfirmation : PlayerTurnState
{
    public State_PieceConfirmation(Piece_Controller pieceController) : base(pieceController) { }

    public override void RunState()
    {
        Piece_Data pieceData = Piece_Detection.GetPieceUnderMouse();
        if (pieceData != null) {
            pieceController.RemoveHighlightOnSelectedPiece();

            if (pieceController.SelectedPiece == pieceData) {
                //Select Piece
                pieceController.SelectedPiece = pieceData;

                //Get a random move
                int randomChessMoveIndex = UnityEngine.Random.Range(0, Piece_Display.instance.chessMoves.Length);
                Chess_Move_SO chessMove = Piece_Display.instance.chessMoves[randomChessMoveIndex];

                //Generate a randomMove and update piece display
                pieceController.ValidMoves = Board_Data.instance.getAllLegalMoves(pieceController.SelectedPiece, chessMove);

                //Update Display for board and pieces
                Piece_Display.instance.TransformSelectedPiece(pieceController.SelectedPiece, randomChessMoveIndex);
                Board_Display.instance.HighLightPossibleMoves(pieceController.ValidMoves);

                pieceController.SetPhaseInTurn( PhaseInTurn.DECIDE_MOVE_OR_REROLL);
            }
            else {
                pieceController.PieceSelection();
            }
        }
    }
}

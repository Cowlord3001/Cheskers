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
                pieceController.SelectedPiece.type = Piece_Data.Type.none;
                pieceController.SelectedPiece = pieceData;

                //Get a random move
                Chess_Move_SO chessMove = PickChessMove();

                //Generate a randomMove and update piece display
                pieceController.ValidMoves = Board_Data.instance.getAllLegalMoves(pieceController.SelectedPiece, chessMove);

                //Update Display for board and pieces
                Piece_Display.instance.TransformSelectedPiece(pieceController.SelectedPiece);
                Board_Display.instance.HighLightPossibleMoves(pieceController.ValidMoves);

                pieceController.SetPhaseInTurn( PhaseInTurn.DECIDE_MOVE_OR_REROLL);
            }
            else {
                pieceController.PieceSelection();
            }
        }
    }

    protected virtual Chess_Move_SO PickChessMove()
    {
        int randomChessMoveIndex = UnityEngine.Random.Range(0, 6);
        return Piece_Display.instance.GetChessMoveByIndex(randomChessMoveIndex);
    }
}

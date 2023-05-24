using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Turn_Manager;

public class State_PieceConfirmation : PlayerTurnState
{
    public State_PieceConfirmation(Turn_Manager pieceController) : base(pieceController) { }

    public override void RunState()
    {
        Piece pieceData = Piece_Detection.GetPieceUnderMouse();
        if (pieceData != null) {
            pieceController.RemoveHighlightOnSelectedPiece();

            if (pieceController.SelectedPiece == pieceData) {
                //Select Piece
                pieceController.SelectedPiece.type = Piece.Type.none;
                pieceController.SelectedPiece = pieceData;

                //Get a random move
                Chess_Move_SO chessMove = PickChessMove();

                //Generate a randomMove and update piece display
                pieceController.ValidMoves = Board.instance.getAllLegalMoves(pieceController.SelectedPiece, chessMove);

                //Update Display for board and pieces
                PieceDisplay_Manager.instance.TransformSelectedPiece(pieceController.SelectedPiece);
                BoardDisplay_Manager.instance.HighLightPossibleMoves(pieceController.ValidMoves);

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
        return PieceDisplay_Manager.instance.GetChessMoveByIndex(randomChessMoveIndex);
    }
}

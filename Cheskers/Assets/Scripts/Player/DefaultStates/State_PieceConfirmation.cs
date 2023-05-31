using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Turn_Manager;

public class State_PieceConfirmation : PlayerTurnState
{
    public State_PieceConfirmation(Turn_Manager pieceController) : base(pieceController) { }

    public override void RunState()
    {
         if (ValidatePieceSelection() == true) {
             //Select Piece
             pieceController.SelectedPiece.type = Piece.Type.none;
             //pieceController.SelectedPiece = pieceData;

             //Get a random move
             Chess_Move_SO chessMove = PickChessMove();

             //Generate a randomMove and update piece display
             pieceController.ValidMoves = Board.instance.getAllLegalMoves(pieceController.SelectedPiece, chessMove);

             //Update Display for board and pieces
             PieceDisplay_Manager.instance.TransformSelectedPiece(pieceController.SelectedPiece);
             BoardDisplay_Manager.instance.HighLightPossibleMoves(pieceController.ValidMoves);

             pieceController.SetPhaseInTurn( PhaseInTurn.DECIDE_MOVE_OR_REROLL);
         }
    }

    protected virtual bool ValidatePieceSelection()
    {
        Piece pieceData = Piece_Detection.GetPieceUnderMouse();
        if (pieceData != null) {
            //Clicked on a piece
            pieceController.RemoveHighlightOnSelectedPiece();
            if (pieceController.SelectedPiece == pieceData) {
                //confirmed selection
                return true;
            }
            else {
                //Selected a new piece
                pieceController.PieceSelection();
                return false;
            }
        }
        else {
            //Didnt click on a piece
            return false; 
        
        }
    }

    protected virtual Chess_Move_SO PickChessMove()
    {
        int randomChessMoveIndex = UnityEngine.Random.Range(0, 6);
        return PieceDisplay_Manager.instance.GetChessMoveByIndex(randomChessMoveIndex);
    }
}

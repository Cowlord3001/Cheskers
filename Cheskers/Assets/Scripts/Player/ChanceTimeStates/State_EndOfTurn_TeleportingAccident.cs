using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using static Turn_Manager;

public class State_EndOfTurn_TeleportingAccident : State_EndOfTurn
{
    public State_EndOfTurn_TeleportingAccident(Turn_Manager pieceController) : base(pieceController) { }

    public override void RunState()
    {
        if (pieceController.GetColor() == Piece.Color.white) {
            //It's whites end of turn so black VIP must be teleported.
            SwapVIP(Piece.Color.black);
        }
        else {
            SwapVIP(Piece.Color.white);
        }

        base.RunState();
    }

    public void SwapVIP(Piece.Color color)
    {
        //It's whites end of turn so black VIP must be teleported.
        Piece targetPiece = null;
        Piece VIP = null;
        List<Piece> pieces = Board.instance.GetPieces();

        Debug.Log("Turn Color: " + pieceController.GetColor());
        Debug.Log("Search Color: " + color);

        //Find Black VIP
        foreach (var piece in pieces) {
            if (piece.IsVIP == true && piece.GetColor() == color) {
                VIP = piece;
                Debug.Log("FOUNDVIP COLOR: " + VIP.GetColor() + " Position: " + VIP.positionOnBoard.ToString());
                //break;
            }
        }
        //Find Ranomd piece to swap with
        while (targetPiece == null) {
            int randomIndex = Random.Range(0, pieces.Count);
            if (pieces[randomIndex] != VIP) {
                targetPiece = pieces[randomIndex];
                Debug.Log("FOUND NOT VIP: " + targetPiece.positionOnBoard.ToString());
            }
        }


        Board.instance.SwapPiecesExternal(targetPiece, VIP);
        PieceDisplay_Manager.instance.UpdatePieces();
    }
}

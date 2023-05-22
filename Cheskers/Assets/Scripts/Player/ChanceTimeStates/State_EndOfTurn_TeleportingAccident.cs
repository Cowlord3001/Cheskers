using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using static Piece_Controller;

public class State_EndOfTurn_TeleportingAccident : State_EndOfTurn
{
    public State_EndOfTurn_TeleportingAccident(Piece_Controller pieceController) : base(pieceController) { }

    public override void RunState()
    {
        if (pieceController.GetColor() == Piece_Data.Color.white) {
            //It's whites end of turn so black VIP must be teleported.
            SwapVIP(Piece_Data.Color.black);
        }
        else {
            SwapVIP(Piece_Data.Color.white);
        }

        base.RunState();
    }

    public void SwapVIP(Piece_Data.Color color)
    {
        //It's whites end of turn so black VIP must be teleported.
        Piece_Data targetPiece = null;
        Piece_Data VIP = null;
        List<Piece_Data> pieces = Board_Data.instance.GetPieces();

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


        Board_Data.instance.SwapPiecesExternal(targetPiece, VIP);
        Piece_Display.instance.UpdatePieces();
    }
}

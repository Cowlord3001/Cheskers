using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerTurnState 
{
    protected Piece_Controller pieceController;
    public PlayerTurnState(Piece_Controller pieceController)
    {
        this.pieceController = pieceController;
    }

    public abstract void RunState();
}

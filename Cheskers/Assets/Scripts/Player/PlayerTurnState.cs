using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerTurnState 
{
    protected Turn_Manager pieceController;
    public PlayerTurnState(Turn_Manager pieceController)
    {
        this.pieceController = pieceController;
    }

    public abstract void RunState();
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinglePlayer_Controller : MonoBehaviour
{
    int coinFlip = 0;
    bool decline = false;
    bool decline2 = false;

    // Start is called before the first frame update
    void Start()
    {
        if(Network_Controller.instance.isMultiplayerGame == true) { return; }

        Input_Controller.instance.OnContestButtonClicked  += OnNewContest;
        Input_Controller.instance.OnContestButton2Clicked += OnNewContest;
        Input_Controller.instance.OnDeclineButtonClicked  += OnDeclinedPressed;
        Input_Controller.instance.OnDeclineButton2Clicked += OnDeclined2Pressed;

        Invoke("SetUpPlayerListeners", .5f);
    }

    void SetUpPlayerListeners()
    {
        Piece_Controller.instance.OnContestStarted += OnContestStarting;
        Piece_Controller.instance.OnEndOfTurn += OnEndTurn;

    }

    //This happens when a player tries to take or damaeg a piece
    void OnContestStarting(object sender, EventArgs e)
    {
        //Multiplayer needs these but not singleplayer
        Input_Controller.EventArgsOnContestButtonClicked f = new Input_Controller.EventArgsOnContestButtonClicked();
        OnNewContest(sender, f);
    }

    //This listens for the contest button to be clicked and is also called when a fresh contest starts
    void OnNewContest(object sender, Input_Controller.EventArgsOnContestButtonClicked e)
    {
        coinFlip = UnityEngine.Random.Range(0, 2);

        Input_Controller.instance.ResetDeclineButtons();
        decline = false;
        decline2 = false;
        Input_Controller.instance.UpdateDisplayBasedOnCoinFlip(coinFlip);

        Debug.Log("New Contest Started");
        //Debug.Log("WHITE: " + Input_Controller.instance.whiteButtonHolder.transform.childCount);
        //Debug.Log("BLACK: " + Input_Controller.instance.blackButtonHolder.transform.childCount);

        //Check if contest should end
        if (Input_Controller.instance.WhitePlayerHasTokens() == false) {
            //Simulate pressing decline button
            Input_Controller.instance.ButtonPressedDecline();
            Debug.Log("Simulating white button decline");
        }
        if (Input_Controller.instance.BlackPlayerHasTokens() == false) {
            //Simulate pressing decline button
            Input_Controller.instance.Button2PressedDecline();
            Debug.Log("Simulating black button decline");
        }
    }

    void OnDeclinedPressed(object sender, EventArgs e)
    {
        decline = true;
        CheckEndContest();
    }
    void OnDeclined2Pressed(object sender, EventArgs e)
    {
        decline2 = true;
        CheckEndContest();
    }

    void CheckEndContest()
    {
        if(decline && decline2) {
            Piece_Controller.instance.EndContest(coinFlip);
        }
    }
    
    void OnEndTurn(object sender, EventArgs e)
    {
        Debug.LogWarning("SingPlayer Ender of Turn Changing Colors");
        if (Piece_Controller.instance.GetPlayerColor() == Piece_Data.Color.white) {
            Piece_Controller.instance.SetPlayerColor(Piece_Data.Color.black);
        }
        else {
            Piece_Controller.instance.SetPlayerColor(Piece_Data.Color.white);
        }
        Piece_Controller.instance.StartTurn();
    }


}

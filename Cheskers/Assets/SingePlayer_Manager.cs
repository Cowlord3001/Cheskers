using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingePlayer_Manager : MonoBehaviour
{
    int coinFlip = 0;
    bool decline = false;
    bool decline2 = false;

    // Start is called before the first frame update
    void Start()
    {
        if(Multiplayer_Manager.instance.isMultiplayerGame == true) { return; }

        Input_Manager.instance.OnContestButtonClicked  += OnNewContest;
        Input_Manager.instance.OnContestButton2Clicked += OnNewContest;
        Input_Manager.instance.OnDeclineButtonClicked  += OnDeclinedPressed;
        Input_Manager.instance.OnDeclineButton2Clicked += OnDeclined2Pressed;

        Invoke("SetUpPlayerListeners", .5f);
    }

    void SetUpPlayerListeners()
    {
        Turn_Manager.instance.OnContestStarted += OnContestStarting;
        Turn_Manager.instance.OnEndOfTurn += OnEndTurn;

    }

    //This happens when a player tries to take or damaeg a piece
    void OnContestStarting(object sender, EventArgs e)
    {
        //Multiplayer needs these but not singleplayer
        Input_Manager.EventArgsOnContestButtonClicked f = new Input_Manager.EventArgsOnContestButtonClicked();
        OnNewContest(sender, f);
    }

    //This listens for the contest button to be clicked and is also called when a fresh contest starts
    void OnNewContest(object sender, Input_Manager.EventArgsOnContestButtonClicked e)
    {
        coinFlip = UnityEngine.Random.Range(0, 2);

        Input_Manager.instance.ResetDeclineButtons();
        decline = false;
        decline2 = false;
        Input_Manager.instance.UpdateDisplayBasedOnCoinFlip(coinFlip);

        //Debug.Log("New Contest Started");
        //Debug.Log("WHITE: " + Input_Controller.instance.whiteButtonHolder.transform.childCount);
        //Debug.Log("BLACK: " + Input_Controller.instance.blackButtonHolder.transform.childCount);

        //Check if contest should end
        if (Token_Manager.instance.WhitePlayerHasTokens() == false) {
            //Simulate pressing decline button
            Input_Manager.instance.ButtonPressedDecline();
            //Debug.Log("Simulating white button decline");
        }
        if (Token_Manager.instance.BlackPlayerHasTokens() == false) {
            //Simulate pressing decline button
            Input_Manager.instance.Button2PressedDecline();
            //Debug.Log("Simulating black button decline");
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
            Turn_Manager.instance.EndContest(coinFlip);
        }
    }
    
    void OnEndTurn(object sender, EventArgs e)
    {
        //Debug.LogWarning("SinglePlayer Ender of Turn Changing Colors");
        if (Turn_Manager.instance.GetPlayerColor() == Piece.Color.white) {
            Turn_Manager.instance.SetPlayerColor(Piece.Color.black);
        }
        else {
            Turn_Manager.instance.SetPlayerColor(Piece.Color.white);
        }
        Turn_Manager.instance.StartTurn();
    }


}

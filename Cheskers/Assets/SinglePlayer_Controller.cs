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

        Input_Controller.instance.OnContestButtonClicked  += OnContestButtonPressed;
        Input_Controller.instance.OnContestButton2Clicked += OnContestButton2Pressed;
        Input_Controller.instance.OnDeclineButtonClicked  += OnDeclinedPressed;
        Input_Controller.instance.OnDeclineButton2Clicked += OnDeclined2Pressed;

        Invoke("SetUpPlayerListeners", .5f);
    }

    void SetUpPlayerListeners()
    {
        Piece_Controller.instance.OnContestStarted += OnContestStarting;
        Piece_Controller.instance.OnEndOfTurn += OnEndTurn;

    }

    void OnContestStarting(object sender, EventArgs e)
    {
        NewContest();
    }

    void NewContest()
    {
        coinFlip = UnityEngine.Random.Range(0, 2);

        Input_Controller.instance.ResetDeclineButtons();
        decline = false;
        decline2 = false;
        Input_Controller.instance.UpdateDisplayBasedOnCoinFlip(coinFlip);

        Debug.Log("New Contest Started");
        Debug.Log("WHITE: " + Input_Controller.instance.whiteButtonHolder.transform.childCount);
        Debug.Log("BLACK: " + Input_Controller.instance.blackButtonHolder.transform.childCount);

        //Check if contest should end
        if (Input_Controller.instance.whiteButtonHolder.transform.childCount == 0) {
            //Simulate pressing decline button
            Input_Controller.instance.ButtonPressedDecline();
            Debug.Log("Simulating white button decline");
        }
        if (Input_Controller.instance.blackButtonHolder.transform.childCount == 0) {
            //Simulate pressing decline button
            Input_Controller.instance.Button2PressedDecline();
            Debug.Log("Simulating black button decline");
        }
    }

    void OnContestButtonPressed(object sender, Input_Controller.EventArgsOnContestButtonClicked e)
    {
        Debug.Log("Destroying white Button Child");
        Destroy(Input_Controller.instance.whiteButtonHolder.transform.GetChild(0).gameObject);

        NewContest();
    }
    void OnContestButton2Pressed(object sender, Input_Controller.EventArgsOnContestButtonClicked e)
    {
        Debug.Log("Destroying black Button Child");
        Destroy(Input_Controller.instance.blackButtonHolder.transform.GetChild(0).gameObject);

        NewContest();
        
    }
    void OnDeclinedPressed(object sender, EventArgs e)
    {
        decline = true;
        CheckEndContest();
    }
    void OnDeclined2Pressed(object sender, EventArgs e)
    {
        Debug.Log("Decline Button 2 Pressed SinglePlayerController");
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
        if (Piece_Controller.instance.color == Piece_Data.Color.white) {
            Piece_Controller.instance.color = Piece_Data.Color.black;
        }
        else {
            Piece_Controller.instance.color = Piece_Data.Color.white;
        }
        Piece_Controller.instance.StartTurn();
    }


}

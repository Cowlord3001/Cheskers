using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class Input_Manager : MonoBehaviour
{
    //MULTIPLAYER TODO: Inputs only fire when it is your turn
    //^May not be necassary anymore, still not a bad idea
    public static Input_Manager instance;
    [HideInInspector] public Vector3 mouseWorldPosition;
    [HideInInspector] public Vector2Int mouseBoardPosition;

    public event EventHandler OnLeftMouseClick;
    public event EventHandler OnRollAgainButtonClicked;
    public event EventHandler OnEndTurnButtonClicked;

    public event EventHandler<EventArgsOnContestButtonClicked> OnContestButtonClicked;
    public event EventHandler<EventArgsOnContestButtonClicked> OnContestButton2Clicked;
    public class EventArgsOnContestButtonClicked : EventArgs
    {
        public Piece.Color colorOfPresser;
    }

    public event EventHandler OnDeclineButtonClicked;
    public event EventHandler OnDeclineButton2Clicked;

    [SerializeField] Button endTurn;
    [SerializeField] Button rerollPieceButton;

    [Header("ContestSettings")]
    [SerializeField] GameObject contestHolder;
    [SerializeField] Button contestButton;
    [SerializeField] Button declineButton;
    [SerializeField] GameObject contest2Holder;
    [SerializeField] Button contest2Button;
    [SerializeField] Button decline2Button;

    [SerializeField] Text contestText;
    [SerializeField] Text contest2Text;

    [Header("DeveloperButtons")]
    [SerializeField] Button pawnButton;
    [SerializeField] Button bishopButton;
    [SerializeField] Button knightButton;
    [SerializeField] Button rookButton;
    [SerializeField] Button queenButton;
    [SerializeField] Button kingButton;

    [SerializeField] GameObject pieceDecider;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        contestHolder.SetActive(false);
        contest2Holder.SetActive(false);

        rerollPieceButton.onClick.AddListener(() => ButtonPressedReroll());
        endTurn.onClick.AddListener(() => ButtonPressedEndTurn());

        contestButton.onClick.AddListener(() => ButtonPressedContest());
        declineButton.onClick.AddListener(() => ButtonPressedDecline());
        if (Multiplayer_Manager.instance.isMultiplayerGame == true) { return; }

        //Debug.Log("Setting up second contest buttons");
        contest2Button.onClick.AddListener(() => Button2PressedContest());
        decline2Button.onClick.AddListener(() => Button2PressedDecline());

        pawnButton.onClick.AddListener(() => setPiece(Piece.Type.pawn));
        bishopButton.onClick.AddListener(() => setPiece(Piece.Type.bishop));
        knightButton.onClick.AddListener(() => setPiece(Piece.Type.knight));
        rookButton.onClick.AddListener(() => setPiece(Piece.Type.rook));
        queenButton.onClick.AddListener(() => setPiece(Piece.Type.queen));
        kingButton.onClick.AddListener(() => setPiece(Piece.Type.king));
    }

    #region DevControls
    
    public static bool developerCommandsEnabled;

    //What do we to do.
    //INFINITE REROLLS (PIECE_CONTROLLER)
    //INFINITE CONTEST - DONE BY ADDING TOKENS

    //CHOOSE PIECE 
    //OVERRIDE TURN (DONE)

    void setPiece(Piece.Type type)
    {
        Debug.Log("AAAAAaa");   // Something broke ;-;
        if(Turn_Manager.instance.phaseInTurn == Turn_Manager.PhaseInTurn.PIECE_CONFIRMATION)
        {
            Turn_Manager.instance.SwapState(Turn_Manager.PhaseInTurn.PIECE_CONFIRMATION, new State_PieceConfirmation_PieceOverride(Turn_Manager.instance, type));
            Turn_Manager.instance.AdvanceGame();
            Turn_Manager.instance.SwapState(Turn_Manager.PhaseInTurn.PIECE_CONFIRMATION, new State_PieceConfirmation(Turn_Manager.instance));
        }
    }

    #endregion

    private void Update()
    {
        if (developerCommandsEnabled) {
            if (Input.GetKeyDown(KeyCode.LeftBracket)) { Token_Manager.instance.GiveContestToken(Piece.Color.black); }
            if (Input.GetKeyDown(KeyCode.RightBracket)) { Token_Manager.instance.GiveContestToken(Piece.Color.white); }
        }
        if (Input.GetKeyDown(KeyCode.R)) { OnRollAgainButtonClicked?.Invoke(this, EventArgs.Empty); }

        if(Input.GetKeyDown(KeyCode.P)) {
            developerCommandsEnabled = !developerCommandsEnabled;
            pieceDecider.SetActive(!pieceDecider.activeSelf);
        }

        //Temporary till hosting scene is seperate.
        if (Camera.main == null) { return; }

        mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseBoardPosition = Piece_Detection.WorldtoBoardIndex(mouseWorldPosition);
        if (Input.GetMouseButtonDown(0)) {
            float halfWidth = Board.instance.size / 2;
            //Only count clicks on the board to prevent button double clicks
            if (mouseWorldPosition.x < halfWidth &&
                mouseWorldPosition.x > -halfWidth &&
                mouseWorldPosition.y < halfWidth &&
                mouseWorldPosition.y > -halfWidth)

                //Send left click to listeners
                OnLeftMouseClick?.Invoke(this, EventArgs.Empty);
        }
    }

    void ButtonPressedReroll()
    {
        OnRollAgainButtonClicked?.Invoke(this, EventArgs.Empty);
    }

    void ButtonPressedContest()
    {
        EventArgsOnContestButtonClicked e = new EventArgsOnContestButtonClicked();
        e.colorOfPresser = Turn_Manager.instance.GetPlayerColor();
        if (Multiplayer_Manager.instance.isMultiplayerGame == false) { 
            e.colorOfPresser = Piece.Color.white; 
        }

        if (e.colorOfPresser == Piece.Color.white && Token_Manager.instance.WhitePlayerHasTokens()) {
            Token_Manager.instance.RemoveBlackToken();
            OnContestButtonClicked?.Invoke(this, e);
        }
        else if (e.colorOfPresser == Piece.Color.black && Token_Manager.instance.BlackPlayerHasTokens()) {
            Token_Manager.instance.RemoveWhiteToken();
            OnContestButtonClicked?.Invoke(this, e);
        }
    }
    public void ButtonPressedDecline()
    {
        //Color green after pressed for visual feedback
        ColorBlock colorBlock = declineButton.colors;
        colorBlock.normalColor = Color.green;
        declineButton.colors = colorBlock;

        OnDeclineButtonClicked?.Invoke(this, EventArgs.Empty);
    }
    //SinglePlayer
    void Button2PressedContest()
    {
        EventArgsOnContestButtonClicked e = new EventArgsOnContestButtonClicked();
        e.colorOfPresser = Piece.Color.black;

        if (e.colorOfPresser == Piece.Color.black && Token_Manager.instance.BlackPlayerHasTokens()) {
            Token_Manager.instance.RemoveWhiteToken();
            OnContestButton2Clicked?.Invoke(this, e);
        }
    }
    public void Button2PressedDecline()
    {
        //Debug.Log("Decline Button 2 Pressed Input");
        //Color green after pressed for visual feedback
        ColorBlock colorBlock = decline2Button.colors;
        colorBlock.normalColor = Color.green;
        decline2Button.colors = colorBlock;

        OnDeclineButton2Clicked?.Invoke(this, EventArgs.Empty);
    }

    public void ResetDeclineButtons()
    {
        ColorBlock colorBlock = declineButton.colors;
        colorBlock.normalColor = Color.white;
        declineButton.colors = colorBlock;

        colorBlock = decline2Button.colors;
        colorBlock.normalColor = Color.white;
        decline2Button.colors = colorBlock;

    }

    void ButtonPressedEndTurn()
    {
        Debug.Log(Turn_Manager.instance.phaseInTurn);
        if (Turn_Manager.instance.phaseInTurn == Turn_Manager.PhaseInTurn.WAITING_FOR_TURN) 
        { 
            return; 
        }

        OnEndTurnButtonClicked?.Invoke(this, EventArgs.Empty);
    }





    const int CAPTURE = 0;
    const int DAMAGE = 1;
    public void UpdateDisplayBasedOnCoinFlip(int coinFlip)
    {
        contestHolder.SetActive(true);
        
        if (coinFlip == CAPTURE) {
            contestText.text = "Piece Capture";
        }
        else {
            contestText.text = "Piece Damage";
        }
        if (Multiplayer_Manager.instance.isMultiplayerGame == true) { return; }

        contest2Holder.SetActive(true);
        if (coinFlip == CAPTURE) {
            contest2Text.text = "Piece Capture";
        }
        else {
            contest2Text.text = "Piece Damage";
        }

    }
    public void TurnOffContestHolders()
    {
        contestHolder.SetActive(false);
        contest2Holder.SetActive(false);
    }

}

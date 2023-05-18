using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class Input_Controller : MonoBehaviour
{
    //MULTIPLAYER TODO: Inputs only fire when it is your turn
    //^May not be necassary anymore, still not a bad idea
    public static Input_Controller instance;
    [HideInInspector] public Vector3 mouseWorldPosition;
    [HideInInspector] public Vector2Int mouseBoardPosition;

    public event EventHandler OnLeftMouseClick;
    public event EventHandler OnRollAgainButtonClicked;
    public event EventHandler OnEndTurnButtonClicked;

    public event EventHandler<EventArgsOnContestButtonClicked> OnContestButtonClicked;
    public event EventHandler<EventArgsOnContestButtonClicked> OnContestButton2Clicked;
    public class EventArgsOnContestButtonClicked : EventArgs
    {
        public Piece_Data.Color colorOfPresser;
    }

    public event EventHandler OnDeclineButtonClicked;
    public event EventHandler OnDeclineButton2Clicked;

    [SerializeField] Button endTurn;
    [SerializeField] Button rerollPieceButton;

    [Header("ReRollButtonSettings")]
    /// <summary>
    /// This holds the black buttons that belong to the whilte player
    /// </summary>
    [SerializeField] GameObject blackTokenImageHolder;
    [SerializeField] GameObject blackTokenImage;
    /// <summary>
    /// This holds the white buttons that belong to the black player
    /// </summary>
    [SerializeField] GameObject whiteTokenImageHolder;
    [SerializeField] GameObject whiteTokenImage;

    [Header("ContestSettings")]
    [SerializeField] GameObject contestHolder;
    [SerializeField] Button contestButton;
    [SerializeField] Button declineButton;
    [SerializeField] GameObject contest2Holder;
    [SerializeField] Button contest2Button;
    [SerializeField] Button decline2Button;

    [SerializeField] Text contestText;
    [SerializeField] Text contest2Text;

    /// <summary>
    /// White Tokens are owned by the black player
    /// </summary>
    int whiteTokens = 0;
    /// <summary>
    /// Black tokens are owned by the white player
    /// </summary>
    int blackTokens = 0;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        Board_Data.instance.OnPieceRemovedFromBoard += OnPieceRemovedFromBoardListener;
        contestHolder.SetActive(false);
        contest2Holder.SetActive(false);

        rerollPieceButton.onClick.AddListener(() => ButtonPressedReroll());
        endTurn.onClick.AddListener(() => ButtonPressedEndTurn());

        contestButton.onClick.AddListener(() => ButtonPressedContest());
        declineButton.onClick.AddListener(() => ButtonPressedDecline());
        if (Network_Controller.instance.isMultiplayerGame == true) { return; }

        Debug.Log("Setting up second contest buttons");
        contest2Button.onClick.AddListener(() => Button2PressedContest());
        decline2Button.onClick.AddListener(() => Button2PressedDecline());
    }

    #region DevControls
    
    public static bool developerCommandsEnabled;

    //What do we to do.
    //INFINITE REROLLS (PIECE_CONTROLLER)
    //INFINITE CONTEST - DONE BY ADDING TOKENS

    //CHOOSE PIECE 
    //OVERRIDE TURN (DONE)


    #endregion

    private void Update()
    {
        if (developerCommandsEnabled) {
            if (Input.GetKeyDown(KeyCode.LeftBracket)) { GiveContestToken(Piece_Data.Color.black); }
            if (Input.GetKeyDown(KeyCode.RightBracket)) { GiveContestToken(Piece_Data.Color.white); }
        }
        if (Input.GetKeyDown(KeyCode.R)) { OnRollAgainButtonClicked?.Invoke(this, EventArgs.Empty); }

        if(Input.GetKeyDown(KeyCode.P)) {
            developerCommandsEnabled = !developerCommandsEnabled;
        }

        //Temporary till hosting scene is seperate.
        if (Camera.main == null) { return; }

        mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseBoardPosition = Piece_Detection.WorldtoBoardIndex(mouseWorldPosition.x, mouseWorldPosition.y);
        if (Input.GetMouseButtonDown(0)) {
            float halfWidth = Board_Data.instance.size / 2;
            //Only count clicks on the board to prevent button double clicks
            if (mouseWorldPosition.x < halfWidth &&
                mouseWorldPosition.x > -halfWidth &&
                mouseWorldPosition.y < halfWidth &&
                mouseWorldPosition.y > -halfWidth)

                //Send left click to listeners
                OnLeftMouseClick?.Invoke(this, EventArgs.Empty);
        }
    }

    void OnPieceRemovedFromBoardListener(object sender, Board_Data.EventArgsPieceRemoved e)
    {
        if(e.removedPiece.GetColor() == Piece_Data.Color.white) {
            GiveContestToken(Piece_Data.Color.black);
        }
        else {
            GiveContestToken(Piece_Data.Color.white);
        }
    }

    public bool WhitePlayerHasTokens()
    {
        if (blackTokens == 0) return false;
        else return true;
    }
    public bool BlackPlayerHasTokens()
    {
        if (whiteTokens == 0) return false;
        else return true;
    }

    void RemoveBlackToken()
    {
        blackTokens--;
        Destroy(blackTokenImageHolder.transform.GetChild(0).gameObject);
    }
    void RemoveWhiteToken()
    {
        whiteTokens--;
        Destroy(whiteTokenImageHolder.transform.GetChild(0).gameObject);
    }
    /// <summary>
    /// Gives a token to the player with the corrsponding color.
    /// </summary>
    /// <param name="color"></param>
    public void GiveContestToken(Piece_Data.Color color)
    {
        if (color == Piece_Data.Color.white) {
            GameObject go = Instantiate(blackTokenImage, blackTokenImageHolder.transform);
            blackTokens++;
        }
        else {
            GameObject go = Instantiate(whiteTokenImage, whiteTokenImageHolder.transform);
            whiteTokens++;
        }
    }
    void ButtonPressedReroll()
    {
        OnRollAgainButtonClicked?.Invoke(this, EventArgs.Empty);
    }

    void ButtonPressedContest()
    {
        EventArgsOnContestButtonClicked e = new EventArgsOnContestButtonClicked();
        e.colorOfPresser = Piece_Controller.instance.GetPlayerColor();
        if (Network_Controller.instance.isMultiplayerGame == false) { 
            e.colorOfPresser = Piece_Data.Color.white; 
        }

        if (e.colorOfPresser == Piece_Data.Color.white && blackTokens > 0) {
            RemoveBlackToken();
            OnContestButtonClicked?.Invoke(this, e);
        }
        else if (e.colorOfPresser == Piece_Data.Color.black && whiteTokens > 0) {
            RemoveWhiteToken();
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
        e.colorOfPresser = Piece_Data.Color.black;

        if (e.colorOfPresser == Piece_Data.Color.black && whiteTokens > 0) {
            RemoveWhiteToken();
            OnContestButton2Clicked?.Invoke(this, e);
        }
    }
    public void Button2PressedDecline()
    {
        Debug.Log("Decline Button 2 Pressed Input");
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
        Debug.Log(Piece_Controller.instance.phaseInTurn);
        if (Piece_Controller.instance.phaseInTurn == Piece_Controller.PhaseInTurn.WAITING_FOR_TURN) 
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
        if (Network_Controller.instance.isMultiplayerGame == true) { return; }

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

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
    public bool developerCommandsEnabled;

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
    public GameObject whiteButtonHolder;
    [SerializeField] GameObject whiteRerollButton;

    public GameObject blackButtonHolder;
    [SerializeField] GameObject blackRerollButton;

    [Header("ContestSettings")]
    [SerializeField] GameObject contestHolder;
    [SerializeField] Button contestButton;
    [SerializeField] Button declineButton;
    [SerializeField] GameObject contest2Holder;
    [SerializeField] Button contest2Button;
    [SerializeField] Button decline2Button;

    [SerializeField] Text contestText;
    [SerializeField] Text contest2Text;

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

    void OnPieceRemovedFromBoardListener(object sender, Board_Data.EventArgsPieceRemoved e)
    {
        if(e.removedPiece.GetColor() == Piece_Data.Color.white) {
            GiveContestToken(Piece_Data.Color.black);
        }
        else {
            GiveContestToken(Piece_Data.Color.white);
        }
    }

    public void GiveContestToken(Piece_Data.Color color)
    {
        if (color == Piece_Data.Color.white) {
            GameObject go = Instantiate(whiteRerollButton, whiteButtonHolder.transform);
            //go.GetComponent<Button>().onClick.AddListener(() => ButtonPressedContest());
        }
        else {
            GameObject go = Instantiate(blackRerollButton, blackButtonHolder.transform);
            //go.GetComponent<Button>().onClick.AddListener(() => ButtonPressedContest());
        }
    }

    void ButtonPressedReroll()
    {
        OnRollAgainButtonClicked?.Invoke(this, EventArgs.Empty);
    }


    void ButtonPressedContest()
    {
        EventArgsOnContestButtonClicked e = new EventArgsOnContestButtonClicked();
        e.colorOfPresser = Piece_Controller.instance.color;
        if (Network_Controller.instance.isMultiplayerGame == true) { 
            e.colorOfPresser = Piece_Data.Color.white; 
        }

        if (e.colorOfPresser == Piece_Data.Color.white && whiteButtonHolder.transform.childCount > 0) {
            OnContestButtonClicked?.Invoke(this, e);
        }
        else if (e.colorOfPresser == Piece_Data.Color.black && blackButtonHolder.transform.childCount > 0) {
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

        if (e.colorOfPresser == Piece_Data.Color.white && whiteButtonHolder.transform.childCount > 0) {
            OnContestButton2Clicked?.Invoke(this, e);
        }
        else if (e.colorOfPresser == Piece_Data.Color.black && blackButtonHolder.transform.childCount > 0) {
            OnContestButton2Clicked?.Invoke(this, e);
        }
    }
    public void Button2PressedDecline()
    {
        Debug.Log("Decline Button 2 Pressed Input");
        //Color green after pressed for visual feedback
        ColorBlock colorBlock = decline2Button.colors;
        colorBlock.normalColor = Color.green;

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


    private void Update()
    {
        if(developerCommandsEnabled)
        {
            if (Input.GetKeyDown(KeyCode.R)) { OnRollAgainButtonClicked?.Invoke(this, EventArgs.Empty); }
            if (Input.GetKeyDown(KeyCode.T)) { Instantiate(whiteRerollButton, whiteButtonHolder.transform); }
        }

        //Temporary till hosting scene is seperate.
        if(Camera.main == null) { return; }

        mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseBoardPosition = Piece_Detection.WorldtoBoardIndex(mouseWorldPosition.x,mouseWorldPosition.y);
        if(Input.GetMouseButtonDown(0)) {
            float halfWidth = Board_Data.instance.size / 2;
            //Only count clicks on the board to prevent button double clicks
            if (mouseWorldPosition.x <  halfWidth &&
                mouseWorldPosition.x > -halfWidth &&
                mouseWorldPosition.y <  halfWidth &&
                mouseWorldPosition.y > -halfWidth)

            //Send left click to listeners
            OnLeftMouseClick?.Invoke(this, EventArgs.Empty);
        }
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

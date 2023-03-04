using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Input_Controller : MonoBehaviour
{
    //MULTIPLAYER TODO: Inputs only fire when it is your turn

    public static Input_Controller instance;
    [HideInInspector] public Vector3 mouseWorldPosition;
    [HideInInspector] public Vector2Int mouseBoardPosition;
    public bool developerCommandsEnabled;

    public event EventHandler OnLeftMouseClick;
    public event EventHandler OnRollAgainButtonClicked;
    public event EventHandler OnEndTurnButtonClicked;

    public event EventHandler OnContestButtonClicked;
    public event EventHandler OnDeclineButtonClicked;

    [SerializeField] Button endTurn;
    [SerializeField] Button rerollPieceButton;

    [Header("ReRollButtonSettings")]
    public GameObject whiteButtonHolder;
    [SerializeField] GameObject whiteRerollButton;

    public GameObject blackButtonHolder;
    [SerializeField] GameObject blackRerollButton;

    [Header("ContestSettings")]
    public GameObject contestHolder;
    [SerializeField] Button contestButton;
    [SerializeField] Button declineButton;
    public Text contestText;

    private void Awake()
    {
        instance = this;
        rerollPieceButton.onClick.AddListener(() => ButtonPressedReroll());
        endTurn.onClick.AddListener(() => ButtonPressedEndTurn());
        contestButton.onClick.AddListener(() => ButtonPressedContest());
        declineButton.onClick.AddListener(() => ButtonPressedDecline());
    }

    private void Start()
    {
        Board_Data.instance.OnPieceRemovedFromBoard += OnPieceRemovedFromBoardListener;
        contestHolder.SetActive(false);
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
            go.GetComponent<Button>().onClick.AddListener(() => ButtonPressedContest());
        }
        else {
            GameObject go = Instantiate(blackRerollButton, blackButtonHolder.transform);
            go.GetComponent<Button>().onClick.AddListener(() => ButtonPressedContest());
        }
    }

    void ButtonPressedReroll()
    {
        OnRollAgainButtonClicked?.Invoke(this, EventArgs.Empty);
    }


    void ButtonPressedContest()
    {
        //Check if in the correct phase to register input...
        if(Piece_Controller.instance.phaseInTurn != Piece_Controller.PhaseInTurn.CONTEST) { return; }

        //TODO: Need check for if player is white or black.
        if (Piece_Controller.instance.color == Piece_Data.Color.white && whiteButtonHolder.transform.childCount > 0) {
            OnContestButtonClicked?.Invoke(this, EventArgs.Empty);
            Destroy(whiteButtonHolder.transform.GetChild(0).gameObject);
        }
        else if (Piece_Controller.instance.color == Piece_Data.Color.black && blackButtonHolder.transform.childCount > 0) {
            OnContestButtonClicked?.Invoke(this, EventArgs.Empty);
            Destroy(blackButtonHolder.transform.GetChild(0).gameObject);
        }
    }

    void ButtonPressedDecline()
    {
        if (Piece_Controller.instance.phaseInTurn != Piece_Controller.PhaseInTurn.CONTEST) { return; }

        OnDeclineButtonClicked?.Invoke(this, EventArgs.Empty);
    }

    void ButtonPressedEndTurn()
    {
        OnEndTurnButtonClicked?.Invoke(this, EventArgs.Empty);
    }


    private void Update()
    {


        if(developerCommandsEnabled)
        {
            if (Input.GetKeyDown(KeyCode.R)) { OnRollAgainButtonClicked?.Invoke(this, EventArgs.Empty); }
            if(Input.GetKeyDown(KeyCode.T)) { Instantiate(whiteRerollButton, whiteButtonHolder.transform); }
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

}

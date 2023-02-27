using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Board_Data;

public class Input_Controller : MonoBehaviour
{
    public static Input_Controller instance;
    [HideInInspector] public Vector3 mouseWorldPosition;
    public bool developerCommandsEnabled;

    public event EventHandler OnLeftMouseClick;
    public event EventHandler OnRollAgainButtonClicked;
    public event EventHandler OnEndTurnButtonClicked;

    [SerializeField] Button endTurn;

    [Header("ReRollButtonSettings")]
    [SerializeField] GameObject whiteButtonHolder;
    [SerializeField] GameObject whiteRerollButton;

    [SerializeField] GameObject blackButtonHolder;
    [SerializeField] GameObject blackRerollButton;

    private void Awake()
    {
        instance = this;
        //rollPieceAgain.onClick.AddListener(() => RollPieceAgain());
        endTurn.onClick.AddListener(() => EndTurn());
    }

    private void Start()
    {
        Board_Data.instance.OnPieceRemovedFromBoard += OnPieceRemovedFromBoardListener;
    }

    void OnPieceRemovedFromBoardListener(object sender, PieceRemovedEventArgs e)
    {
        if(e.removedPiece.GetColor() == Piece_Data.Color.black) {
            GameObject go = Instantiate(whiteRerollButton, whiteButtonHolder.transform);
            go.GetComponent<Button>().onClick.AddListener(() => RollPieceAgain());
        }
        else {
            GameObject go = Instantiate(blackRerollButton, blackButtonHolder.transform);
            go.GetComponent<Button>().onClick.AddListener(() => RollPieceAgain());
        }
    }

    void RollPieceAgain()
    {
        //TODO: Need check for if player is white or black.
        if (Piece_Controller.color == Piece_Data.Color.white && whiteButtonHolder.transform.childCount > 0) {
            OnRollAgainButtonClicked?.Invoke(this, EventArgs.Empty);
            Destroy(whiteButtonHolder.transform.GetChild(0).gameObject);
        }
        else if (Piece_Controller.color == Piece_Data.Color.black && blackButtonHolder.transform.childCount > 0) {
            OnRollAgainButtonClicked?.Invoke(this, EventArgs.Empty);
            Destroy(blackButtonHolder.transform.GetChild(0).gameObject);
        }
    }

    void EndTurn()
    {
        OnEndTurnButtonClicked?.Invoke(this, EventArgs.Empty);
    }


    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.R) && developerCommandsEnabled) {
            OnRollAgainButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
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

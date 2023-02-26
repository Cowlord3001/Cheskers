using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Input_Controller : MonoBehaviour
{
    public static Input_Controller instance;

    public Vector3 mouseWorldPosition;

    public event EventHandler OnLeftMouseClick;
    public event EventHandler OnRollAgainButtonClicked;
    public event EventHandler OnEndTurnButtonClicked;

    [SerializeField] Button rollPieceAgain;
    [SerializeField] Button endTurn;

    private void Awake()
    {
        instance = this;
        rollPieceAgain.onClick.AddListener(() => RollPieceAgain());
        endTurn.onClick.AddListener(() => EndTurn());
    }

    void RollPieceAgain()
    {
        OnRollAgainButtonClicked?.Invoke(this, EventArgs.Empty);
    }

    void EndTurn()
    {
        OnEndTurnButtonClicked?.Invoke(this, EventArgs.Empty);
    }


    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.R)) {
            RollPieceAgain();
        }

        mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if(Input.GetMouseButtonDown(0)) {
            float halfWidth = Board_Display.boardData.size / 2;
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

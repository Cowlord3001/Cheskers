using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Token_Manager : MonoBehaviour
{
    [Header("Token Prefabs")]
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

    public static Token_Manager instance;
    // Start is called before the first frame update
    void Awake()
    {
        Token_Manager.instance = this;
    }

    private void Start()
    {
        Board.instance.OnPieceRemovedFromBoard += OnPieceRemovedFromBoardListener;
    }
    /// <summary>
    /// White Tokens are owned by the black player
    /// </summary>
    int whiteTokens = 0;
    /// <summary>
    /// Black tokens are owned by the white player
    /// </summary>
    int blackTokens = 0;

    public int GetBlackTokenNumber() { return blackTokens; }
    public int GetWhiteTokenNumber() { return whiteTokens; }

    public void ClearTokens()
    {
        int currentWhiteTokens = whiteTokens;
        for (int i = 0; i < currentWhiteTokens; i++) {
            whiteTokens--;
            Destroy(whiteTokenImageHolder.transform.GetChild(i).gameObject);
        }
        int currentBlackTokens = blackTokens;
        for (int i = 0; i < currentBlackTokens; i++) {
            blackTokens--;
            Destroy(blackTokenImageHolder.transform.GetChild(i).gameObject);
        }
    }

    /// <summary>
    /// Gives a token to the player with the corrsponding color.
    /// </summary>
    /// <param name="color"></param>
    public void GiveContestToken(Piece.Color color)
    {
        //You can't get a token if its the other players turn.
        //If its white's turn and they lose a piece to something like thorns black should not get a token
        //if color does not equal the turn color no token given
        //This is called from remove piece events and needs protection
        if (Multiplayer_Manager.instance.isMultiplayerGame) {
            //multiplayer
            if (color != Multiplayer_Manager.instance.turnColor.Value) return;
        }
        else {
            //single player
            if (color != Turn_Manager.instance.GetColor()) return;
        }

        GiveToken(color);
    }

    public void GiveToken(Piece.Color color)
    {
        if (color == Piece.Color.white) {
            GameObject go = Instantiate(blackTokenImage, blackTokenImageHolder.transform);
            blackTokens++;
        }
        else {
            GameObject go = Instantiate(whiteTokenImage, whiteTokenImageHolder.transform);
            whiteTokens++;
        }
    }

    void OnPieceRemovedFromBoardListener(object sender, Board.EventArgsPieceRemoved e)
    {
        if (e.removedPiece.GetColor() == Piece.Color.white) {
            GiveContestToken(Piece.Color.black);
        }
        else {
            GiveContestToken(Piece.Color.white);
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

    public void RemoveBlackToken()
    {
        blackTokens--;
        Destroy(blackTokenImageHolder.transform.GetChild(0).gameObject);
    }
    public void RemoveWhiteToken()
    {
        whiteTokens--;
        Destroy(whiteTokenImageHolder.transform.GetChild(0).gameObject);
    }
}

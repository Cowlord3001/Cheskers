using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class BoardDisplay_Manager : MonoBehaviour
{
    [SerializeField] GameObject blackTile;
    [SerializeField] GameObject whiteTile;

    public static BoardDisplay_Manager instance;

    public GameObject[,] boardTiles { get; private set; }

    public event EventHandler<EventArgsOnValidMovesHighlighted> OnValidMovesHighlighted;
    public class EventArgsOnValidMovesHighlighted { public Vector2Int[] validMoves; }

    public event EventHandler<EventArgsOnValidMovesDeHighlighted> OnValidMovesDeHighlighted;
    public class EventArgsOnValidMovesDeHighlighted { public Vector2Int[] validMoves; }

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;

    }

    private void Start()
    {
        CreateBoard();
    }

    void CreateBoard()
    {

        boardTiles = new GameObject[Board.instance.size, Board.instance.size];
        float halfWidth = Board.instance.size / 2;
        float tileWidth = .5f;
        bool isWhite = true;
        for (int y = 0; y < Board.instance.size; y++) {
            for (int x = 0; x < Board.instance.size; x++) {
                if(isWhite == true) {
                    isWhite = false;
                    GameObject go = Instantiate(whiteTile);
                    go.transform.position = new Vector2(x - halfWidth + tileWidth, y - halfWidth + tileWidth);
                    go.transform.parent = transform;
                    boardTiles[x, y] = go;
                }
                else {
                    isWhite = true;
                    GameObject go = Instantiate(blackTile);
                    go.transform.position = new Vector2(x - halfWidth + tileWidth, y - halfWidth + tileWidth);
                    go.transform.parent = transform;
                    boardTiles[x, y] = go;
                }
            }
            isWhite = !isWhite;
        }
        
    }

    public void HighLightPossibleMoves(List<Vector2Int> validMoves)
    {
        foreach (Vector2Int moveLocation in validMoves) {
            SpriteRenderer spriteRenderer = BoardDisplay_Manager.instance.boardTiles[moveLocation.x, moveLocation.y].GetComponent<SpriteRenderer>();
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, .1f);
        }

        EventArgsOnValidMovesHighlighted e = new EventArgsOnValidMovesHighlighted();
        e.validMoves = validMoves.ToArray();
        OnValidMovesHighlighted?.Invoke(this, e);
    }

    public void RemoveHighLightPossibleMoves(List<Vector2Int> validMoves)
    {
        if (validMoves == null) {
            Debug.LogWarning("Removing Highlighted Tiles with any moves");
            return;
        }

        foreach (Vector2Int moveLocation in validMoves) {
            SpriteRenderer spriteRenderer = BoardDisplay_Manager.instance.boardTiles[moveLocation.x, moveLocation.y].GetComponent<SpriteRenderer>();
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 1f);
        }

        EventArgsOnValidMovesDeHighlighted e = new EventArgsOnValidMovesDeHighlighted();
        e.validMoves = validMoves.ToArray();
        OnValidMovesDeHighlighted?.Invoke(this, e);
    }
}

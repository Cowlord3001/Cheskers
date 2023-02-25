using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece_Controller : MonoBehaviour
{
    [SerializeField] Chess_Move_SO[] chessMoves;

    Board_Data boardData;
    Piece_Data selectedPiece;

    List<Vector2Int> possibleMoves;
    
    // Start is called before the first frame update
    void Start()
    {
        boardData = Board_Setup.boardData;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0)) {
            Piece_Data pieceData = Piece_Detection.GetPieceUnderMouse();
            if(pieceData != null) {
                if (selectedPiece != null) {
                    RemoveHighLightPossibleMoves();
                }
                selectedPiece = pieceData;
                int randomMove = Random.Range(0, chessMoves.Length);
                

                possibleMoves = boardData.getAllLegalMoves(pieceData, chessMoves[randomMove]);
                Debug.Log(chessMoves[randomMove].name);
                HighLightPossibleMoves();
            }
        }
    }

    void HighLightPossibleMoves()
    {
        foreach (Vector2Int moveLocation in possibleMoves) {
            SpriteRenderer sprite = Board_Setup.boardTiles[moveLocation.x, moveLocation.y].GetComponent<SpriteRenderer>();
            sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, .1f);
        }
    }

    void RemoveHighLightPossibleMoves()
    {
        foreach (Vector2Int moveLocation in possibleMoves) {
            SpriteRenderer sprite = Board_Setup.boardTiles[moveLocation.x, moveLocation.y].GetComponent<SpriteRenderer>();
            sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, 1f);
        }
    }

}

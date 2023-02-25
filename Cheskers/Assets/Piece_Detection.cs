using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;

public static class Piece_Detection 
{
    public static Piece_Data GetPieceUnderMouse()
    {
        //Get MouseCoordinates -- Move to Input Controller later
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        float raycastdistance = 20f;//Camera is at -10z so 20 units should be good
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector3.forward,  raycastdistance);

        if (hit.collider == null) return null;

        Vector2Int boardIndicies = WorldtoBoardIndex(hit.point.x, hit.point.y);

        return Board_Setup.boardData.boardPieces[boardIndicies.x, boardIndicies.y];

    }

    static Vector2Int WorldtoBoardIndex(float x, float y)
    {
        Vector2Int BoardIndices = Vector2Int.zero;
        float halfWidth = Board_Setup.boardData.size / 2;
        float halfTileSize = .5f;

        BoardIndices.x = Mathf.RoundToInt(x + halfWidth - halfTileSize);
        BoardIndices.y = Mathf.RoundToInt(y + halfWidth - halfTileSize);

        return BoardIndices;
    }

}

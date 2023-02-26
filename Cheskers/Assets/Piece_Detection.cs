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
        Vector3 mousePosition = Input_Controller.instance.mouseWorldPosition;

        float raycastdistance = 20f;//Camera is at -10z so 20 units should be good
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector3.forward, raycastdistance);

        if (hit.collider == null) return null;

        Vector2Int boardIndicies = WorldtoBoardIndex(mousePosition.x, mousePosition.y);

        return Board_Setup.boardData.boardPieces[boardIndicies.x, boardIndicies.y];

    }

    public static Vector2Int GetTileIndexUnderMouse()
    {
        //Get MouseCoordinates -- Move to Input Controller later
        Vector3 mousePosition = Input_Controller.instance.mouseWorldPosition;

        Vector2Int boardIndicies = WorldtoBoardIndex(mousePosition.x, mousePosition.y);

        return boardIndicies;

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

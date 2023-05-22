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

        Vector2Int boardIndicies = WorldtoBoardIndex(mousePosition);

        return Board_Data.instance.boardPieces[boardIndicies];

    }

    public static Vector2Int GetTileIndexUnderMouse()
    {
        //Get MouseCoordinates -- Move to Input Controller later
        Vector3 mousePosition = Input_Controller.instance.mouseWorldPosition;

        Vector2Int boardIndicies = WorldtoBoardIndex(mousePosition);

        return boardIndicies;

    }

    public static Vector2Int WorldtoBoardIndex(Vector3 position)
    {
        Vector2Int BoardIndices = Vector2Int.zero;
        float halfWidth = Board_Data.instance.size / 2;
        float halfTileSize = .5f;

        BoardIndices.x = Mathf.RoundToInt(position.x + halfWidth - halfTileSize);
        BoardIndices.y = Mathf.RoundToInt(position.y + halfWidth - halfTileSize);

        return BoardIndices;
    }

    public static Vector3 BoardIndextoWorld(Vector2Int boardPosition)
    {
        Vector3 worldPosition = Vector3.zero;
        float halfWidth = Board_Data.instance.size / 2;
        float halfTileSize = .5f;
        worldPosition.x = boardPosition.x - halfWidth + halfTileSize;
        worldPosition.y = boardPosition.y - halfWidth + halfTileSize;

        return worldPosition;
    }

}

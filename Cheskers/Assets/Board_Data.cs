using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board_Data
{
    Piece_Data[,] Pieces;

    Board_Data()
    {
        Pieces = new Piece_Data[8,8];
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                if (y <= 1)
                {
                    Pieces[x, y] = new Piece_Data(true);
                }
                else if (y >= 6)
                {
                    Pieces[x, y] = new Piece_Data(false);
                }
                else Pieces[x,y] = null;
            }
        }
    }
}

using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board_Setup : MonoBehaviour
{
    public GameObject Tile;

    // Start is called before the first frame update
    void Start()
    {
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                if (y % 2 == 0) //y is even
                {
                    float x1 = (float)(2 * x - 3.5); // -3.5, -1.5, 1.5, 3.5
                    float y1 = (float)(y - 3.5);
                    Instantiate(Tile);
                    Tile.transform.position = new Vector2(x1, y1);
                }
                else //y is odd
                {
                    float x1 = (float)(2 * x - 2.5); // -2.5, -0.5, 2.5, 4.5
                    float y1 = (float)(y - 3.5);
                    Instantiate(Tile);
                    Tile.transform.position = new Vector2(x1, y1);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

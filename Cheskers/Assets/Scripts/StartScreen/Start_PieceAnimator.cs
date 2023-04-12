using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Start_PieceAnimator : MonoBehaviour
{

    [SerializeField] Sprite[] sprites;
    [SerializeField] int changeTime;
    int spriteIndex;
    

    SpriteRenderer renderer;
    // Start is called before the first frame update
    void Start()
    {
        renderer = GetComponent<SpriteRenderer>();
        float startTime = Random.Range(0f, changeTime);
        spriteIndex = sprites.Length - 1;

        InvokeRepeating("ChangeSprite", startTime, changeTime);
    }

    void ChangeSprite()
    {
        int newIndex = Random.Range(0, sprites.Length);
        while(spriteIndex == newIndex) {
            newIndex = Random.Range(0, sprites.Length);
        }
        spriteIndex = newIndex;

        renderer.sprite = sprites[spriteIndex];
    }

}

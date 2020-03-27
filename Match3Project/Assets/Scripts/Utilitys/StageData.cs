using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StageData
{
    public int stageLevel;

    public int horizontal;
    public int vertical;

    public int clearScore;
    public int move;

    public Vector2[] blankSpace;
    public Vector2[] breakableTile;

    public Sprite backgroundSprite;
    public AudioClip backgroundMusic;

    public Sprite[] pieceSprites;
    public Sprite[] itemSprites;

    public Sprite firstNodeSprite;
    public Sprite clearNodeSprite;

    public Sprite emptyStarSprite;
    public Sprite clearStarSprite;

    public int lastClearScore;
    public int lastClearStar;

    public void SaveData(int score, int star)
    {
        lastClearScore = score;
        lastClearStar = star;
    }
}
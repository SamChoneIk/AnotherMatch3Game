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

    public float camPivotX;
    public float camPivotY;
    public float camPivotSize;

    public Vector2[] blankSpace;
    public Vector2[] breakableTile;

    public Sprite backgroundSprite;
    public AudioClip backgroundMusic;

    public Sprite[] pieceSprites;
    public Sprite[] itemSprites;
}
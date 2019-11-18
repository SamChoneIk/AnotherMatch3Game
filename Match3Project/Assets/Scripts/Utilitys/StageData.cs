using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New StageData", menuName = "Stage/Create a StageData")]
public class StageData : ScriptableObject
{
    public int stageLevel;
    public int clearScore;
    public int move;

    public Sprite backgroundSprite;
    public AudioClip backgroundMusic;
}

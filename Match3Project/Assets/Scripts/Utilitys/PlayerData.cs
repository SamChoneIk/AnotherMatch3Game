using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct ClearStageData
{
    public int level;

    public int score;
    public int star;
}

[Serializable]
public class PlayerData
{
    public int totalScore;

    public float bgmVolume = 1;
    public float seVolume = 1;

    public bool destroyAd = false;

    public ClearStageData[] stageDatas;

    public void SetStageData(int idx, int score, int star)
    {
        if (stageDatas[idx].score < score)
            stageDatas[idx].score = score;

        if (stageDatas[idx].star < star)
            stageDatas[idx].star = star;
    }

    public ClearStageData GetStageData(int level)
    {
        return Array.Find(stageDatas, sd => sd.level == level);
    }
}
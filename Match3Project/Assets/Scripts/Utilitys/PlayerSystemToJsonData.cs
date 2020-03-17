using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

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
    //public ArrayLayout lay;

    public Sprite backgroundSprite;
    public AudioClip backgroundMusic;
}

[Serializable]
public class PlayerData
{
    public float bgmVolume;
    public float seVolume;
	public int totalScore;
}

public static class PlayerSystemToJsonData
{
    public static PlayerData playerData = new PlayerData();

    public static void SavePlayerSystemData()
    {
        playerData.bgmVolume = StaticVariables.BgmVolume;
        playerData.seVolume = StaticVariables.SeVolume;
        playerData.totalScore = StaticVariables.TotalScore;

        string path = StaticVariables.PlayerDataPath;
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        path += StaticVariables.PlayerDataName;

        string jsonData = JsonUtility.ToJson(playerData);
        File.WriteAllText(path, jsonData);
    }

    public static void LoadPlayerSystemData()
    {
        if (!File.Exists(StaticVariables.PlayerDataPath + StaticVariables.PlayerDataName))
            SavePlayerSystemData();

        string path = StaticVariables.PlayerDataPath + StaticVariables.PlayerDataName;
        string jsonData = File.ReadAllText(path);
        playerData = JsonUtility.FromJson<PlayerData>(jsonData);

        StaticVariables.BgmVolume = playerData.bgmVolume;
        StaticVariables.SeVolume = playerData.seVolume;
        StaticVariables.TotalScore = playerData.totalScore;

		StaticVariables.DataLoad = true;
    }
}
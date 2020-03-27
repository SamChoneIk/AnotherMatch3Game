using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


[Serializable]
public class PlayerData
{
    public int lastStageClearLevel;
    public int totalScore;

    public float bgmVolume = 1;
    public float seVolume = 1;

    public bool destroyAd = false;
}

public static class PlayerSystemToJsonData
{
    public static PlayerData playerData;

    public static void SavePlayerSystemData()
    {
        playerData.lastStageClearLevel = StaticVariables.LoadLevel;
        playerData.totalScore = StaticVariables.TotalScore;

        playerData.bgmVolume = StaticVariables.BgmVolume;
        playerData.seVolume = StaticVariables.SeVolume;

        playerData.destroyAd = StaticVariables.DestroyAd;

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
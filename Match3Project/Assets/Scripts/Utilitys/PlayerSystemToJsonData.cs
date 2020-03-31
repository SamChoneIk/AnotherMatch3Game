using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class PlayerSystemToJsonData
{
    public static PlayerData playerData;

    public static void SavePlayerSystemData()
    {
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
        {
            playerData = new PlayerData();
            InitializePlayerData();
            SavePlayerSystemData();
        }

        string path = StaticVariables.PlayerDataPath + StaticVariables.PlayerDataName;
        string jsonData = File.ReadAllText(path);
        playerData = JsonUtility.FromJson<PlayerData>(jsonData);

        StaticVariables.TotalScore = playerData.totalScore;

        StaticVariables.BgmVolume = playerData.bgmVolume;
        StaticVariables.SeVolume = playerData.seVolume;

        StaticVariables.DestroyAd = playerData.destroyAd;

        StaticVariables.DataLoad = true;
    }

    public static void InitializePlayerData()
    {
        playerData.stageDatas = new ClearStageData[GameManager.Instance.stageData_SO.stageDatas.Count];
        for(int i = 0; i < playerData.stageDatas.Length; ++i)
        {
            playerData.stageDatas[i] = new ClearStageData();
            playerData.stageDatas[i].level = i + 1;
        }
    }
}
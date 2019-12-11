using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class PlayerData
{
    public float bgmVolume;
    public float seVolume;
}

public class PlayerSystemToJsonData : MonoBehaviour
{
    private PlayerData playerData = new PlayerData();
    public static PlayerSystemToJsonData instance;
    public void Awake()
    {
        instance = this;
        LoadPlayerSystemData();
    }

    public void SavePlayerSystemData()
    {
        playerData.bgmVolume = StaticVariables.BgmVolume;
        playerData.seVolume = StaticVariables.SeVolume;

        string path = StaticVariables.PlayerDataPath;
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        path += StaticVariables.PlayerDataName;

        string jsonData = JsonUtility.ToJson(playerData);
        File.WriteAllText(path, jsonData);
    }

    public void LoadPlayerSystemData()
    {
        if (!File.Exists(StaticVariables.PlayerDataPath + StaticVariables.PlayerDataName))
            SavePlayerSystemData();

        string path = StaticVariables.PlayerDataPath + StaticVariables.PlayerDataName;
        string jsonData = File.ReadAllText(path);
        playerData = JsonUtility.FromJson<PlayerData>(jsonData);

        StaticVariables.BgmVolume = playerData.bgmVolume;
        StaticVariables.SeVolume = playerData.seVolume;
    }
}
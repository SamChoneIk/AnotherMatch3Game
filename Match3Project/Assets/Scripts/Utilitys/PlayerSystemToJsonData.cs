using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class PlayerData
{
    public float bgmVolume;
    public float seVolume;
    public int stageTotalClear;
    public int stageTotalFail;
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
        playerData.bgmVolume = Variables.bgmVolume;
        playerData.seVolume = Variables.seVolume;
        playerData.stageTotalClear = Variables.stageTotalClear;
        playerData.stageTotalFail = Variables.stageTotalFail;

        string path = Variables.playerDataPath;
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        path += Variables.playerDataName;

        string jsonData = JsonUtility.ToJson(playerData);
        File.WriteAllText(path,jsonData);
    }

    public void LoadPlayerSystemData()
    {
        if (!File.Exists(Variables.playerDataPath + Variables.playerDataName))
        {
            SavePlayerSystemData();
        }

        string path = Variables.playerDataPath + Variables.playerDataName;
        string jsonData = File.ReadAllText(path);
        playerData = JsonUtility.FromJson<PlayerData>(jsonData);

        Variables.bgmVolume = playerData.bgmVolume;
        Variables.seVolume = playerData.seVolume;
        Variables.stageTotalClear = playerData.stageTotalClear;
        Variables.stageTotalFail = playerData.stageTotalFail;
    }
}
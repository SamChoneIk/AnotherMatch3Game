using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class PlayerData
{
    public float bgmVolume;
    public float seVolume;
    public int totalScore;
}

public class PlayerSystemToJsonData : MonoBehaviour
{
    private PlayerData playerData = new PlayerData();
    private static PlayerSystemToJsonData instance;
    public static PlayerSystemToJsonData Instance
    {
        get
        {
            if (instance != null)
                return instance;

            instance = FindObjectOfType<PlayerSystemToJsonData>();

            if (instance == null)
                instance = new GameObject(name : "PlayerSystemToJsonData").AddComponent<PlayerSystemToJsonData>();

            return instance;
        }
    }

    public void SavePlayerSystemData()
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

    public void LoadPlayerSystemData()
    {
        if (!File.Exists(StaticVariables.PlayerDataPath + StaticVariables.PlayerDataName))
            SavePlayerSystemData();

        string path = StaticVariables.PlayerDataPath + StaticVariables.PlayerDataName;
        string jsonData = File.ReadAllText(path);
        playerData = JsonUtility.FromJson<PlayerData>(jsonData);

        StaticVariables.BgmVolume = playerData.bgmVolume;
        StaticVariables.SeVolume = playerData.seVolume;
        StaticVariables.TotalScore = playerData.totalScore;
    }
}
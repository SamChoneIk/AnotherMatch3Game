using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class PlayerData
{
    public float bgmVolume;
    public float seVolume;

    public bool stageFirst;
    public bool stageAll;
    public bool stage1;
    public bool stage2;
    public bool stage3;
    public bool stage4;
    public bool stage5;
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
        playerData.bgmVolume = StaticVariables.bgmVolume;
        playerData.seVolume = StaticVariables.seVolume;

        playerData.stageFirst = StaticVariables.stageFirst;
        playerData.stageAll = StaticVariables.stageAll;
        playerData.stage1 = StaticVariables.stage1;
        playerData.stage2 = StaticVariables.stage2;
        playerData.stage3 = StaticVariables.stage3;
        playerData.stage4 = StaticVariables.stage4;
        playerData.stage5 = StaticVariables.stage5;

        string path = StaticVariables.playerDataPath;
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        path += StaticVariables.playerDataName;

        string jsonData = JsonUtility.ToJson(playerData);
        File.WriteAllText(path, jsonData);
    }

    public void LoadPlayerSystemData()
    {
        if (!File.Exists(StaticVariables.playerDataPath + StaticVariables.playerDataName))
        {
            SavePlayerSystemData();
        }

        string path = StaticVariables.playerDataPath + StaticVariables.playerDataName;
        string jsonData = File.ReadAllText(path);
        playerData = JsonUtility.FromJson<PlayerData>(jsonData);

        StaticVariables.bgmVolume = playerData.bgmVolume;
        StaticVariables.seVolume = playerData.seVolume;

        StaticVariables.stageFirst = playerData.stageFirst;
        StaticVariables.stageAll = playerData.stageAll;
        StaticVariables.stage1 = playerData.stage1;
        StaticVariables.stage2 = playerData.stage2;
        StaticVariables.stage3 = playerData.stage3;
        StaticVariables.stage4 = playerData.stage4;
        StaticVariables.stage5 = playerData.stage5;
    }
}
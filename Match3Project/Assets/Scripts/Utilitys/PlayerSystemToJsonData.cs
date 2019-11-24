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

    public bool stageFirstClear;
    public bool stageAllClear;
    public bool stage1Clear;
    public bool stage2Clear;
    public bool stage3Clear;
    public bool stage4Clear;
    public bool stage5Clear;
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

        playerData.stageFirstClear = Variables.stageFirstClear;
        playerData.stageAllClear = Variables.stageAllClear;
        playerData.stage1Clear = Variables.stage1Clear;
        playerData.stage2Clear = Variables.stage2Clear;
        playerData.stage3Clear = Variables.stage3Clear;
        playerData.stage4Clear = Variables.stage4Clear;
        playerData.stage5Clear = Variables.stage5Clear;

        string path = Variables.playerDataPath;
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        path += Variables.playerDataName;

        string jsonData = JsonUtility.ToJson(playerData);
        File.WriteAllText(path, jsonData);
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

        Variables.stageFirstClear = playerData.stageFirstClear;
        Variables.stageAllClear = playerData.stageAllClear;
        Variables.stage1Clear = playerData.stage1Clear;
        Variables.stage2Clear = playerData.stage2Clear;
        Variables.stage3Clear = playerData.stage3Clear;
        Variables.stage4Clear = playerData.stage4Clear;
        Variables.stage5Clear = playerData.stage5Clear;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;





public enum SceneIndex
{
    MAIN = 0,
    GAME = 1,
}





public static class StaticVariables
{
    public static int StageLevel;
    public static int TotalScore;

    public static float BgmVolume = 1;
    public static float SeVolume = 1;

    public static bool DataLoad = false;
    public static bool loginSuccess = false;
    public static bool DestroyAd = false;

    public static string StageController = "StageController";

    public static string DisabledPieceName = "DefaultPiece";
    public static string PieceSpritesPath = "Arts/PieceSprite";
    public static string ItemSpritesPath = "Arts/ItemSprite";
    public static string StageSoundEffectPath = "Sounds/SE";
    public static string StageDataPath = "Data/stage";
    public static string PlayerDataPath = Application.persistentDataPath + "/PlayerData/";
    public static string PlayerDataName = "CEMPlayerdata.json";
}
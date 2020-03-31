using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StaticVariables
{
    public static int LoadLevel;
    public static int TotalScore;

    public static float BgmVolume = 1;
    public static float SeVolume = 1;

    public static bool LoginSuccess = false;
    public static bool DataLoad = false;
    public static bool DestroyAd = false;
    public static bool GameStarts = false;

    public static string SelectStageName => $"STAGE {LoadLevel}";
    public static string GetStage => $"Stage{LoadLevel}";
    public static string Score = "SCORE : ";
    public static string ClearScore = "CLEAR SCORE : ";
    public static string Clear = "CLEAR !!";
    public static string Fail = "FAIL...";
    public static string GameStart = "GAME START";
    public static string NextStage = "NEXT STAGE";
    public static string Replay = "REPLAY";
    public static string Cancel = "CANCEL";
    public static string Back = "BACK";
    public static string Quit = "QUIT";
    public static string BackToMenu = "BACK TO MENU";

    public static string DisabledPieceName = "DefaultPiece";
    public static string PieceSpritesPath = "Arts/PieceSprite";
    public static string ItemSpritesPath = "Arts/ItemSprite";
    public static string StageSoundEffectPath = "Sounds/SE";
    public static string StageDataPath = "Data/stage";
    public static string PlayerDataPath = Application.persistentDataPath + "/PlayerData/";
    public static string PlayerDataName = "CEMPlayerdata.json";
}
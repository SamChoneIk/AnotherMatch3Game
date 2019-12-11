using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StageState
{
    PLAY,
    CLEAR,
    FAIL,
}

public enum BoardState
{
    ORDER,
    WORK,
}

public enum PieceState
{
    WAIT,
    MOVE
}

public enum SceneIndex
{
    MAIN = 0,
    GAME = 1,
}

public enum PieceEffect
{
    PIECEEXPLOSION = 0,
    COLUMNBOMB = 1,
    CROSSBOMB = 2,
    ROWBOMB = 3,
    HINTEFFECT = 4,
}

public enum SoundEffectList
{
    SWAP = 0,
    TUNNING = 1,
    MATCHED = 2,
}

public static class StaticVariables
{
    public static bool DataLoad = false;

    public static int StageLevel;
    public static float BgmVolume = 1;
    public static float SeVolume = 1;

    public static bool DestroyAd = false;
    public static bool FirstClear = false;

    public static string DisabledPieceName = "DefaultPiece";
    public static string PieceSpritesPath = "Arts/PieceSprite";
    public static string ItemSpritesPath = "Arts/ItemSprite";
    public static string StageSoundEffectPath = "Sounds/SE";
    public static string StageDataPath = "Data/stage";
    public static string PlayerDataPath = Application.persistentDataPath + "/PlayerData/";
    public static string PlayerDataName = "CEMPlayerdata.json";
}
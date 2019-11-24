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

public static class Variables
{
    public static bool dataLoad = false;

    public static int stageLevel;
    public static float bgmVolume = 1;
    public static float seVolume = 1;
    public static int stageTotalClear = 0;
    public static int stageTotalFail = 0;

    public static bool stageFirstClear = false;
    public static bool stageAllClear = false;
    public static bool stage1Clear = false;
    public static bool stage2Clear = false;
    public static bool stage3Clear = false;
    public static bool stage4Clear = false;
    public static bool stage5Clear = false;

    public static string disabledPieceName = "DefaultPiece";
    public static string pieceSpritesPath = "Arts/PieceSprite";
    public static string itemSpritesPath = "Arts/ItemSprite";
    public static string stageSoundEffectPath = "Sounds/SE";
    public static string stageDataPath = "Data/stage";
    public static string playerDataPath = Application.persistentDataPath + "/PlayerData/";
    public static string playerDataName = "CEMPlayerdata.json";
}
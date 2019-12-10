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
    public static bool dataLoad = false;

    public static int stageLevel;
    public static float bgmVolume = 1;
    public static float seVolume = 1;

    public static bool stageFirst= false;
    public static bool stageAll = false;
    public static bool stage1 = false;
    public static bool stage2 = false;
    public static bool stage3 = false;
    public static bool stage4 = false;
    public static bool stage5 = false;

    public static bool isDestroyAd = false;

    public static string disabledPieceName = "DefaultPiece";
    public static string pieceSpritesPath = "Arts/PieceSprite";
    public static string itemSpritesPath = "Arts/ItemSprite";
    public static string stageSoundEffectPath = "Sounds/SE";
    public static string stageDataPath = "Data/stage";
    public static string playerDataPath = Application.persistentDataPath + "/PlayerData/";
    public static string playerDataName = "CEMPlayerdata.json";

    public static void StageClears()
    {
        if(!stageFirst)
        {
            stageFirst = true;
            Social.ReportProgress(GPGSIds.achievement_first_stage_clear, 100f, null);
        }

        switch(stageLevel)
        {
            case 1:
                {
                    if (stage1)
                        break;

                        stage1 = true;
                        Social.ReportProgress(GPGSIds.achievement_1_stage_clear, 100f, null);
                        break;
                }

            case 2:
                {
                    if (stage2)
                        break;

                    stage2 = true;
                    Social.ReportProgress(GPGSIds.achievement_2_stage_clear, 100f, null);
                    break;
                }

            case 3:
                {
                    if (stage3)
                        break;

                    stage3 = true;
                    Social.ReportProgress(GPGSIds.achievement_3_stage_clear, 100f, null);
                    break;
                }

            case 4:
                {
                    if (stage4)
                        break;

                    stage4 = true;
                    Social.ReportProgress(GPGSIds.achievement_4_stage_clear, 100f, null);
                    break;
                }

            case 5:
                {
                    if (stage5)
                        break;

                    stage5 = true;
                    Social.ReportProgress(GPGSIds.achievement_5_stage_clear, 100f, null);
                    break;
                }
        }

        if(!stageAll)
        {
            stageAll = true;
            Social.ReportProgress(GPGSIds.achievement_all_stage_clear, 100f, null);
        }
    }
}
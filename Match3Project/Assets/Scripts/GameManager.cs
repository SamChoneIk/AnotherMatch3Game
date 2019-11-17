using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SceneIndex
{
    MAIN = 0,
    GAME = 1,
}

public enum SoundEffectList
{
    SWAP = 0,
    TUNNING = 1,
    MATCHED = 2,
}

public static class Variables
{
    public static int stageLevel;
    public static float bgmVolume = 1;
    public static float seVolume = 1;

    public static string disabledPieceName = "DefaultPiece";
    public static string pieceSpritesPath = "Arts/PieceSprite";
    public static string itemSpritesPath = "Arts/ItemSprite";
    public static string stageBackgroundPath = "Arts/Background";
    public static string stageBackgroundMusicPath = "Sounds/BGM";
    public static string stageSoundEffectPath = "Sounds/SE";
    public static string stageDataPath = "Data/stage";
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }

        else if (instance != this)
            Destroy(gameObject);
    }

    private void Start()
    {
        
    }
}
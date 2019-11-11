using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class StageData
{
    public int stage;
    public int goalScore;
    public int move;
    public int backgroundIndex;
    public int musicIndex;
}

public class GameManager : MonoBehaviour
{
    public GameObject pause;

    public int stageLevel = 0;

    public static GameManager instance;
    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(this);
    }

    public void PauseMenu()
    {
        pause.SetActive(true);
    }
}
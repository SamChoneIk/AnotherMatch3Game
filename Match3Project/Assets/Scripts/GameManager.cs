using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

[System.Serializable]
public class StageData
{
    public int stage;
    public int goalScore;
    public int move;
    public int bgIdx;
    public int bgmIdx;
}

public class GameManager : MonoBehaviour
{
    public GameObject pauseUI;
    public GameObject clearUI;
    public GameObject failUI;

    public AudioClip[] bg_Ms;
    public Sprite[] bgSprite;

    public int stageLevel = 0;

    public StageData stageData;
    public static GameManager instance;
    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        bg_Ms = Resources.LoadAll<AudioClip>("Music");
        bgSprite = Resources.LoadAll<Sprite>("Art/Background");
    }

    public void PauseMenu()
    {
        pauseUI.SetActive(true);
    }

    public void Resume()
    {

    }

    public void Option()
    {

    }

    public void MainMenu()
    {

    }

    public void LoadGameData()
    {
        string path = Application.streamingAssetsPath + "/Stages/" + "stage" + stageLevel.ToString() + ".json";

        if (File.Exists(path))
        {
            Debug.Log("Load JsonFile");
            string jsonData = File.ReadAllText(path);
            stageData = JsonUtility.FromJson<StageData>(jsonData);
        }
    }

    public void StageClear()
    {
        clearUI.SetActive(true);
    }

    public void StageFail()
    {
        failUI.SetActive(true);
    }
}
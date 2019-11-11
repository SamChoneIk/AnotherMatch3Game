using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class StageManager : MonoBehaviour
{
    public Image background;
    private AudioSource audioSource;
    public Text scoreText;
    public Text goalScoreText;
    public Text moveText;
    public Slider slider;

    public AudioClip[] backgroundMusic;
    public Sprite[] backgroundSprite;

    public int goalScore;

    public float score = 0;
    public float delay = 2f;

    public int combo = 0;

    private int moveValue = 20;
    private float nextScore = 0;

    public Board board;

    private StageData stageData;
    public static StageManager instance;
    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();

        backgroundMusic = Resources.LoadAll<AudioClip>("Music");
        backgroundSprite = Resources.LoadAll<Sprite>("Art/Background");

        LoadGameData();

        moveText.text = moveValue.ToString();
        goalScoreText.text = "GOAL SCORE : " + Mathf.FloorToInt(goalScore).ToString("D8");
        slider.maxValue = goalScore;
    }

    private void Update()
    {
        if (score != nextScore)
        {
            score = Mathf.Lerp(score, nextScore, Time.deltaTime * 0.8f);
            scoreText.text = "SCORE : " + Mathf.FloorToInt(score).ToString("D8");
            slider.value = score;

            if (score >= goalScore)
            {
                if (board.currState != BoardState.CLEAR)
                {
                    Debug.Log("clear");
                    board.currState = BoardState.CLEAR;
                }
            }

            if (moveValue == 0)
            {
                if (board.currState != BoardState.FAIL)
                {
                    Debug.Log("FAIL");
                    board.currState = BoardState.FAIL;
                }
            }

            if (score >= nextScore - 10)
            {
                score = nextScore;
                scoreText.text = "SCORE : " + Mathf.FloorToInt(score).ToString("D8");
                slider.value = score;

                if(board.currState == BoardState.CLEAR)
                {
                    Debug.Log("Clear UI");
                }

                else if (board.currState == BoardState.FAIL)
                {
                    Debug.Log("FAIL UI");
                }
            }
        }
    }

    public void IncreaseScore(int value)
    {
        nextScore += combo > 1 ? value * combo : value;
    }

    public void DecreaseMove(int value)
    {
        moveValue -= value;
        moveText.text = moveValue.ToString();
    }

    public void LoadGameData()
    {
        string path = Application.streamingAssetsPath + "/Stages/" + "stage" + 1.ToString() + ".json";//GameManager.instance.stageLevel;

        if(File.Exists(path))
        {
            Debug.Log("Load JsonFile");
            string jsonData = File.ReadAllText(path);
            stageData = JsonUtility.FromJson<StageData>(jsonData);

            background.sprite = backgroundSprite[stageData.backgroundIndex];
            audioSource.clip = backgroundMusic[stageData.musicIndex];
            audioSource.Play();
            moveValue = stageData.move;
            goalScore = stageData.goalScore;
        }
    }
}
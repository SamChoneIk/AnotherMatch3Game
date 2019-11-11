using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class StageManager : MonoBehaviour
{
    public Image background;
    private AudioSource bg_M;
    public Text scoreText;
    public Text goalScoreText;
    public Text moveText;
    public Slider slider;

    public int goalScore;

    public float score = 0;
    public float scoringSpeed = 0f;

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
        bg_M = GetComponent<AudioSource>();

        StageInit();
    }

    private void StageInit()
    {
        stageData = GameManager.instance.stageData;

        background.sprite = GameManager.instance.bgSprite[stageData.bgIdx];
        bg_M.clip = GameManager.instance.bg_Ms[stageData.bgmIdx];
        bg_M.Play();
        moveValue = stageData.move;
        goalScore = stageData.goalScore;

        moveText.text = moveValue.ToString();
        goalScoreText.text = "GOAL SCORE : " + Mathf.FloorToInt(goalScore).ToString("D8");
        slider.maxValue = goalScore;
    }

    private void Update()
    {
        if (score != nextScore)
        {
            score = Mathf.Lerp(score, nextScore, Time.deltaTime * scoringSpeed);
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
                    GameManager.instance.StageClear();

                else if (board.currState == BoardState.FAIL)
                    GameManager.instance.StageFail();
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
}
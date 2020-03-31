using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum StageState
{
    Play, Clear, Fail,
}

public class GameStageManager : MonoBehaviour
{
    [Header("Stage State")]
    public StageState currStageState = StageState.Play;

    [Header("Stage Components")]
    public Board board;
    public Image stageBG;

    public Text scoreText;
    public Slider scoreSlider;
    public Text stageClearScoreText;
    public Text moveText;

    private UIManager uIMgr;
    [Header("Stage Variables")]
    public int matchedScore = 30;
    public int decreaseMoveValue = 1;

    public float scoringSpeed = 0f;
    public int combo = 0;

    private float score;
    private float nextScore;
    private int clearScore;
    private int moveValue;
    private int stars = 0;

    public void Start()
    {
        board.InitializeBoard(this);
        InitializeGameStage();
    }

    public BGMClip GetStageClip()
    {
        BGMClip clip = (BGMClip)Enum.Parse(typeof(BGMClip), StaticVariables.GetStage);

        return clip;
    }

    private void InitializeGameStage()
    {
        uIMgr = UImenu.manager;

        PauseWindow quitButton = uIMgr.GetWindow(Menus.Pause) as PauseWindow;
        quitButton.ChangedButton(SceneType.Game);

        clearScore = board.stageData.clearScore;
        moveValue = board.stageData.move;

        stageBG.sprite = board.stageData.backgroundSprite;

        moveValue = board.stageData.move;
        clearScore = board.stageData.clearScore;

        stageClearScoreText.text = $"{StaticVariables.ClearScore} {Mathf.FloorToInt(clearScore).ToString("D6")}";
        scoreSlider.maxValue = clearScore;

        moveText.text = moveValue.ToString();

        currStageState = StageState.Play;

        GameManager.Instance.BackgroundMusicPlay(board.stageData.backgroundMusic);
    }

    public void Update()
    {
        if (!uIMgr.onTheMenu)
        {
            if (!IsStageStopped())
                board.BoardStates();

            if (score != nextScore)
                IncreasingScore();
        }
    }

    public void IncreasingScore()
    {
        score = Mathf.Lerp(score, nextScore, Time.deltaTime * scoringSpeed);
        scoreText.text = $"{StaticVariables.Score} {Mathf.FloorToInt(score).ToString("D6")}";
        scoreSlider.value = score;

        CheckTheGameState();

        if (score >= nextScore - 5)
        {
            if (moveValue == 0)
                currStageState = stars > 0 ? StageState.Clear : StageState.Fail;

            score = nextScore;
            scoreText.text = $"{StaticVariables.Score} {Mathf.FloorToInt(score).ToString("D6")}";
            scoreSlider.value = score;

            if (IsStageStopped() && board.currBoardState == BoardState.Order)
            {
                PlayerSystemToJsonData.playerData.SetStageData(board.stageData.stageLevel - 1, (int)score, stars);
                GameManager.Instance.StageResult(currStageState);
            }

            StaticVariables.TotalScore += Mathf.FloorToInt(score);
        }
    }

    public void CheckTheGameState()
    {
        if (IsStageStopped())
            return;

        if (nextScore >= clearScore * 0.5 && stars == 0)
        {
            stars++;
        }

        if (nextScore >= clearScore * 0.5 && stars == 1)
        {
            stars++;
        }

        else if (nextScore >= clearScore && stars == 2)
        {
            stars++;
            currStageState = StageState.Clear;
        }
    }

    public void IncreaseScore(int value)
    {
        nextScore += value * combo;
    }

    public void DecreaseMove(int value)
    {
        moveValue -= value;
        moveText.text = moveValue.ToString();
    }

    public bool IsStageStopped()
    {
        return currStageState == StageState.Clear || 
            currStageState == StageState.Fail;
    }

    public void PauseButton()
    {
        uIMgr.OnTheWindow(Menus.Pause);
    }
}
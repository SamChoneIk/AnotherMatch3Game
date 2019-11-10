using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageMgr : MonoBehaviour
{
    public Text scoreText;
    public Text moveText;
    public Slider slider;

    private int stage;
    private int moveValue = 20;

    public float score = 0;
    public float delay = 2f;

    public float goalScore;

    private float nextScore = 0;
    private float accumTime = 0;

    public static StageMgr instance;
    private void Awake()
    {
        instance = this;
        moveText.text = moveValue.ToString();
        slider.maxValue = goalScore;
    }

    private void Update()
    {
        if (score != nextScore)
        {
            score = Mathf.Lerp(score, nextScore, Time.deltaTime * 0.8f);
            scoreText.text = "SCORE : " + Mathf.FloorToInt(score).ToString("D8");
            slider.value = score;

            if (score >= nextScore - 10)
            {
                score = nextScore;
                scoreText.text = "SCORE : " + Mathf.FloorToInt(score).ToString("D8");
                slider.value = score;
            }
        }
    }

    public void DecreaseMove(int value)
    {
        moveValue -= value;
        moveText.text = moveValue.ToString();
    }

    public void IncreaseScore(int value)
    {
        nextScore += value;
    }
}
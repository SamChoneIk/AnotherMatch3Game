using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageMgr : MonoBehaviour
{
    public Text scoreText;
    public Slider slider;

    public float score = 0;
    public float goalScore;

    private float prevScore;
    public float scoreSpeed = 0;

    private float accumTime = 0;
    public float delay = 2f;

    public static StageMgr scoreMgr;
    private void Awake()
    {
        scoreMgr = this;
    }

    public void IncreseScore(int value)
    {
        prevScore = score;
        score += value;
    }

    private void Update()
    {
        if (prevScore != score)
        {
            accumTime += Time.deltaTime;

            scoreText.text = "SCORE : " + Mathf.FloorToInt(Mathf.Lerp((int)prevScore, (int)score, Time.deltaTime * scoreSpeed)).ToString("D8");

            if(accumTime > delay)
            {
                prevScore = score;
                scoreText.text = "SCORE : " + score;

                accumTime = 0;
            }
        }
    }

}
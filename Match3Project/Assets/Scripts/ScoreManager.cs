using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public Text scoreText;

    public int score = 0;

    public static ScoreManager scoreMgr;

    private void Awake()
    {
        scoreMgr = this;
    }

    public void ScoreRaise(int value)
    {
        score += value;
        scoreText.text = "SCORE : " + score.ToString("D10");
    }
}
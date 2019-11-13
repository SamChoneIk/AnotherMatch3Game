using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int stageLevel;
    public float bgmVolume = 1;
    public float seVolume = 1;

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

    public void NextGame()
    {
        ++stageLevel;
    }

}
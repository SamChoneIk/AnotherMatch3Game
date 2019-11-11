using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StageSelecter : MonoBehaviour
{
    public void Stage1()
    {
        GameManager.instance.stageLevel = 1;
        GameManager.instance.LoadGameData();
        SceneManager.LoadScene("Game");
    }

    public void Stage2()
    {
        GameManager.instance.stageLevel = 2;
        GameManager.instance.LoadGameData();
        SceneManager.LoadScene("Game");
    }

    public void Stage3()
    {
        GameManager.instance.stageLevel = 3;
        GameManager.instance.LoadGameData();
        SceneManager.LoadScene("Game");
    }

    public void Stage4()
    {
        GameManager.instance.stageLevel = 4;
        GameManager.instance.LoadGameData();
        SceneManager.LoadScene("Game");
    }

    public void Stage5()
    {
        GameManager.instance.stageLevel = 5;
        GameManager.instance.LoadGameData();
        SceneManager.LoadScene("Game");
    }
}

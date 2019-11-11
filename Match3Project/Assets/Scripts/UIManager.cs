using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject stageSelect;
    public GameObject option;

    public void GameStart()
    {
        mainMenu.SetActive(false);
        stageSelect.SetActive(true);
    }

    public void StageSelect()
    {

    }

    public void Option()
    {

    }

    public void BackToMainMenu()
    {
        if(SceneManager.GetActiveScene().buildIndex == 0)
        {
            stageSelect.SetActive(false);
            mainMenu.SetActive(true);
        }

        else
        {
            SceneManager.LoadScene(0);
        }
    }

    public void GameExit()
    {
        Application.Quit();
    }
}
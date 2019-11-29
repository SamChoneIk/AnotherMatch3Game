using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [Header("MainMenu UI Menus")]
    public GameObject mainUI;
    public GameObject stageSelectUI;
    public GameObject optionUI;

    private GameObject currMenu;

    [Header("MainMenu UI Text")]
    public Text totalClear;
    public Text totalFail;

    [Header("MainMenu UI Parts")]
    public AudioSource mainMenuBGM;
    public Slider bgmVolume;
    public Slider seVolume;

    private void Start()
    {
        mainMenuBGM.volume = StaticVariables.bgmVolume;

        bgmVolume.value = StaticVariables.bgmVolume;
        seVolume.value = StaticVariables.seVolume;

        currMenu = mainUI;
    }

    private void Update()
    {
        if (optionUI.activeInHierarchy)
            mainMenuBGM.volume = bgmVolume.value;

        if (Application.platform == RuntimePlatform.Android)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (currMenu == optionUI || currMenu == stageSelectUI)
                    BackMenu();

                else if (currMenu == mainUI)
                    QuitGame();
            }
        }
    }

    public void GameStart()
    { 
        currMenu.SetActive(false);
        currMenu = stageSelectUI;
        currMenu.SetActive(true);
    }

    public void SelectStage(int level)
    {
        StaticVariables.stageLevel = level;
        SceneManager.LoadScene((int)SceneIndex.GAME);
    }

    public void Option()
    {
        currMenu.SetActive(false);
        currMenu = optionUI;
        currMenu.SetActive(true);
    }

    public void BackMenu()
    {
        if (currMenu == optionUI)
        {
            StaticVariables.bgmVolume = bgmVolume.value;
            StaticVariables.seVolume = seVolume.value;
        }

        currMenu.SetActive(false);
        currMenu = mainUI;
        currMenu.SetActive(true);
    }

    public void QuitGame()
    {
        PlayerSystemToJsonData.instance.SavePlayerSystemData();
        Application.Quit();
    }
}
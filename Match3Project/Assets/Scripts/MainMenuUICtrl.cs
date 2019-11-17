using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUICtrl : MonoBehaviour
{
    [Header("MainMenu UI Menus")]
    public GameObject mainTitleUI;
    public GameObject stageSelectUI;
    public GameObject optionUI;

    private GameObject currUI;

    [Header("MainMenu UI Parts")]
    public AudioSource mainMenuBGM;
    public Slider bgmVolume;
    public Slider seVolume;

    private void Start()
    {
        mainTitleUI = currUI;

        bgmVolume.value = Variables.bgmVolume;
        seVolume.value = Variables.seVolume;
    }

    private void Update()
    {
        if (optionUI.activeInHierarchy)
        {
            Variables.bgmVolume = bgmVolume.value;
            Variables.seVolume = seVolume.value;
        }
    }

    public void GameStart()
    { 
        mainTitleUI.SetActive(false);
        stageSelectUI.SetActive(true);

        currUI = stageSelectUI;
    }

    public void StageSelect(int level)
    {
        Variables.stageLevel = level;
    }

    public void Option()
    {
        mainTitleUI.SetActive(false);
        optionUI.SetActive(true);

        currUI = optionUI;
    }

    public void BackMenu()
    {
        if (currUI == optionUI)
        {
            Variables.bgmVolume = bgmVolume.value;
            Variables.seVolume = seVolume.value;
        }

        currUI.SetActive(false);
        mainTitleUI.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
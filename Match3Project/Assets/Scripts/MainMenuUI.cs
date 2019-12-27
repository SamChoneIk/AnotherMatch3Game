﻿using System.Collections;
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
    public GameObject shopUI;

    private GameObject currMenu;

    [Header("MainMenu UI Parts")]
    public AudioSource mainMenuBGM;
    public Slider bgmVolume;
    public Slider seVolume;

	private void Start()
	{
		if (Social.localUser.authenticated)
			GooglePlayManager.Instance.RefreshAchievements();

		mainMenuBGM.volume = StaticVariables.BgmVolume;

		bgmVolume.value = StaticVariables.BgmVolume;
		seVolume.value = StaticVariables.SeVolume;

		currMenu = mainUI;
	}

    private void Update()
    {
        if (currMenu == optionUI)
        {
            StaticVariables.BgmVolume = bgmVolume.value;
            mainMenuBGM.volume = bgmVolume.value;

            StaticVariables.SeVolume = seVolume.value;
        }

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
        StaticVariables.StageLevel = level;
        SceneManager.LoadScene((int)SceneIndex.GAME);
    }

    public void Option()
    {
        currMenu.SetActive(false);
        currMenu = optionUI;
        currMenu.SetActive(true);
    }

    public void Shop()
    {
        currMenu.SetActive(false);
        currMenu = shopUI;
        currMenu.SetActive(true);
    }

	public void ShowAchievements()
	{
		if (Social.localUser.authenticated)
			return;

		//GameManager.Instance.WriteLog("업적을 확인합니다.\n");
		Social.ShowAchievementsUI();
	}

	public void ShowLeaderBoard()
	{
		if (Social.localUser.authenticated)
			return;

		//GameManager.Instance.WriteLog("리더보드를 확인합니다.\n");
		Social.ShowLeaderboardUI();
	}

	public void BackMenu()
    {
		PlayerSystemToJsonData.Instance.SavePlayerSystemData();

		currMenu.SetActive(false);
        currMenu = mainUI;
        currMenu.SetActive(true);
    }

	public void GoogleLogin()
	{
		GooglePlayManager.Instance.LogIn();
	}

	public void GoogleLogout()
	{
		GooglePlayManager.Instance.LogOut();
	}

    public void QuitGame()
    {
		//GameManager.Instance.WriteLog("게임을 종료합니다.");
		Application.Quit();
    }
}
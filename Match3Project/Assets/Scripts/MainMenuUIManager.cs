using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUIManager : MonoBehaviour
{
    public GameObject mainMenuUI;
    public GameObject stageSelectUI;
    public GameObject optionUI;

    private GameObject prevMenu;
    private GameObject currMenu;

    public Slider bgmVolume;
    public Slider seVolume;

    private AudioSource bgMusic;

    private void Start()
    {
        bgMusic = GetComponent<AudioSource>();
        bgMusic.volume = GameManager.instance.bgmVolume;

        bgmVolume.value = GameManager.instance.bgmVolume;
        seVolume.value = GameManager.instance.seVolume;
    }

    private void Update()
    {
        if (optionUI.activeInHierarchy)
            bgMusic.volume = bgmVolume.value;
    }

    public void GameStart()
    { 
        mainMenuUI.SetActive(false);
        stageSelectUI.SetActive(true);

        currMenu = stageSelectUI;
    }

    public void Option()
    {
        mainMenuUI.SetActive(false);
        optionUI.SetActive(true);

        currMenu = optionUI;
    }

    public void BackMenu()
    {
        if(currMenu == optionUI)
        {
            GameManager.instance.bgmVolume = bgmVolume.value;
            GameManager.instance.seVolume = seVolume.value;
        }

        currMenu.SetActive(false);
        mainMenuUI.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
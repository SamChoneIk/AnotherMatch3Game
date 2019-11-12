using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameObject mainMenuUI;
    public GameObject stageSelectUI;
    public GameObject optionUI;

    public Slider volumeSlider;

    private GameObject currMenuUI;
    private AudioSource bgMusic;

    private void Start()
    {
        bgMusic = GetComponent<AudioSource>();
        bgMusic.volume = GameManager.instance.bgmVolume;

        volumeSlider.value = bgMusic.volume;
    }

    private void Update()
    {
        if (optionUI.activeInHierarchy)
        {
            bgMusic.volume = volumeSlider.value;
        }
    }

    public void GameStart()
    { 
        mainMenuUI.SetActive(false);
        stageSelectUI.SetActive(true);

        currMenuUI = stageSelectUI;
    }

    public void Option()
    {
        mainMenuUI.SetActive(false);
        optionUI.SetActive(true);

        currMenuUI = optionUI;
    }

    public void MainMenu()
    {
        if(currMenuUI == optionUI)
        {
            GameManager.instance.bgmVolume = bgMusic.volume;
        }

        currMenuUI.SetActive(false);
        mainMenuUI.SetActive(true);
    }

    public void GameExit()
    {
        Application.Quit();
    }
}
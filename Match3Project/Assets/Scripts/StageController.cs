using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

public class StageController : MonoBehaviour
{
    [Header("Stage UI Menus")]
    public GameObject pauseUI;
    public GameObject pauseMenu;
    public GameObject optionMenu;
    public GameObject clearMenu;
    public GameObject failMenu;

    private GameObject currMenu;

    [Header("Stage UI Texts")]
    public Text scoreText;
    public Text moveText;
    public Text stageClearScoreText;
    public Text resultClearScoreText;
    public Text lastStageMessageText;

    [Header("Stage UI Images")]
    public Slider bgmSlider;
    public Slider seSlider;
    public Slider scoreSlider;
    public Button nextLevelButton;
    public Sprite starSprite;
    public Image[] clearStars;

    [Header("Stage Parts")]
    public StageState currStageState = StageState.PLAY;

    public Image stageBG;
    public AudioSource stageBGM;
    public AudioSource stageSE;

    private StageData[] stageDatas;
    private AudioClip[] seClips;

    public float scoringSpeed = 0f;
    public int combo = 0;

    private float score;
    private float nextScore;
    private int clearScore;
    private int moveValue;
    private int stars = 0;

    public Board board;

    public static StageController instance;
    private void Awake()
    {
        instance = this;

#if UNITY_EDITOR || UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE
        Camera.main.orthographicSize = 6;
#endif

#if UNITY_IOS || UNITY_IPHONE || UNITY_ANDROID || UNITY_WEBGL
        Camera.main.orthographicSize = 7;
#endif
    }

    private void Start()
    {
        seClips = Resources.LoadAll<AudioClip>(StaticVariables.stageSoundEffectPath);
        stageDatas = Resources.LoadAll<StageData>(StaticVariables.stageDataPath);

        StageInit();
    }

    private void StageInit()
    {
        LoadStageData();

        stageClearScoreText.text = $"CLEAR SCORE : {Mathf.FloorToInt(clearScore).ToString("D6")}";
        scoreSlider.maxValue = clearScore;

        moveText.text = moveValue.ToString();
    }

    private void LoadStageData()
    {
        StageData stageData = stageDatas.Where(sd => sd.stageLevel == StaticVariables.stageLevel).Single();

        stageBGM.volume = StaticVariables.bgmVolume;
        stageSE.volume = StaticVariables.seVolume;

        bgmSlider.value = StaticVariables.bgmVolume;
        seSlider.value = StaticVariables.seVolume;

        clearScore = stageData.clearScore;
        moveValue = stageData.move;

        stageBG.sprite = stageData.backgroundSprite;
        stageBGM.clip = stageData.backgroundMusic;
        stageBGM.Play();

        moveValue = stageData.move;
        clearScore = stageData.clearScore;

        nextLevelButton.gameObject.SetActive(stageData.stageLevel == stageDatas.Length ? false : true);
        lastStageMessageText.gameObject.SetActive(stageData.stageLevel == stageDatas.Length ? true : false);
    }

    private void Update()
    {
       if (Application.platform == RuntimePlatform.Android)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (currMenu == null)
                    PauseMenu();

                if (currMenu == optionMenu)
                    BackMenu();

                if (currMenu == pauseMenu)
                    Resume();
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currMenu == null)
                PauseMenu();

            if (currMenu == optionMenu)
                BackMenu();

            if (currMenu == pauseMenu)
                Resume();
        }

        if (Input.GetKeyDown(KeyCode.Space))
            Debug.Log(currMenu);

        if (optionMenu.activeInHierarchy)
        {
            stageBGM.volume = bgmSlider.value;
            stageSE.volume = seSlider.value;

            return;
        }

        if (score != nextScore)
        {
            score = Mathf.Lerp(score, nextScore, Time.deltaTime * scoringSpeed);
            scoreText.text = $"SCORE : {Mathf.FloorToInt(score).ToString("D6")}";
            scoreSlider.value = score;

            CheckTheGameState();

            if (score >= nextScore - 1)
            {
                if (moveValue == 0)
                    currStageState = stars > 0 ? StageState.CLEAR : StageState.FAIL;

                score = nextScore;
                scoreText.text = $"SCORE : {Mathf.FloorToInt(score).ToString("D6")}";
                scoreSlider.value = score;


                if (currStageState == StageState.CLEAR)
                {
                    resultClearScoreText.text = $"SCORE : {Mathf.FloorToInt(score).ToString("D6")}";

                    StageClear();
                }

                else if (currStageState == StageState.FAIL)
                    StageFail();
            }
        }
    }

    public void CheckTheGameState()
    {
        if (currStageState == StageState.CLEAR || currStageState == StageState.FAIL)
            return;

        if (nextScore >= clearScore * 0.5 && stars == 0)
            clearStars[stars++].sprite = starSprite;

        else if (nextScore >= clearScore * 0.75 && stars == 1)
            clearStars[stars++].sprite = starSprite;

        else if (nextScore >= clearScore && stars == 2)
        {
            clearStars[stars].sprite = starSprite;
            currStageState = StageState.CLEAR;
        }
    }

    public void IncreaseScore(int value)
    {
        nextScore += value * combo;
    }

    public void DecreaseMove(int value)
    {
        moveValue -= value;
        moveText.text = moveValue.ToString();
    }

    public void SoundEffectPlay(SoundEffectList seClip)
    {
        stageSE.clip = seClips[(int)seClip];
        stageSE.Play();
    }

    public void PauseMenu()
    {
        Time.timeScale = 0;

        pauseUI.SetActive(true);
        currMenu = pauseMenu;
        currMenu.SetActive(true);
    }

    public void Resume()
    {
        Time.timeScale = 1;

        currMenu.SetActive(false);
        currMenu = null;
        pauseUI.SetActive(false);
    }

    public void OptionMenu()
    {
        currMenu.SetActive(false);
        currMenu = optionMenu;
        currMenu.SetActive(true);
    }

    public void BackMenu()
    {
        if (currMenu == optionMenu)
        {
            StaticVariables.bgmVolume = bgmSlider.value;
            StaticVariables.seVolume = seSlider.value;
        }

        currMenu.SetActive(false);
        currMenu = pauseMenu;
        currMenu.SetActive(true);
    }

    public void StageClear()
    {
        ++StaticVariables.stageTotalClear;
        StaticVariables.StageClears();

        pauseUI.SetActive(true);
        clearMenu.SetActive(true);

        currMenu = clearMenu;
    }

    public void StageFail()
    {
        ++StaticVariables.stageTotalFail;

        pauseUI.SetActive(true);
        failMenu.SetActive(true);

        currMenu = failMenu;
    }

    public void BackToMainMenu()
    {
        Resume();
        SceneManager.LoadScene((int)SceneIndex.MAIN);
    }

    public void NextStage()
    {
        Resume();
        ++StaticVariables.stageLevel;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Replay()
    {
        Resume();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
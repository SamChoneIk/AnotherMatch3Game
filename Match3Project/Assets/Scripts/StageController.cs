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

    public int matchedScore = 30;
    public int decreaseMoveValue = 1;

    public float scoringSpeed = 0f;
    public int combo = 0;

    private float score;
    private float nextScore;
    private int clearScore;
    private int moveValue;
    private int stars = 0;

    public Board board;
    private void Awake()
    {
#if UNITY_EDITOR || UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE
        Camera.main.orthographicSize = 6;
#endif

#if UNITY_IOS || UNITY_IPHONE || UNITY_ANDROID || UNITY_WEBGL
        Camera.main.orthographicSize = 7;
#endif


    }

    private void Start()
    {
        seClips = Resources.LoadAll<AudioClip>(StaticVariables.StageSoundEffectPath);
        stageDatas = Resources.LoadAll<StageData>(StaticVariables.StageDataPath);

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
        StageData stageData = stageDatas.Where(sd => sd.stageLevel == StaticVariables.StageLevel).Single();

        stageBGM.volume = StaticVariables.BgmVolume;
        stageSE.volume = StaticVariables.SeVolume;

        bgmSlider.value = StaticVariables.BgmVolume;
        seSlider.value = StaticVariables.SeVolume;

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
        if (!IsStageStopped())
            board.BoardStates();

        if (currMenu == optionMenu)
        {
            stageBGM.volume = bgmSlider.value;
            StaticVariables.BgmVolume = bgmSlider.value;

            stageSE.volume = seSlider.value;
            StaticVariables.SeVolume = seSlider.value;

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

                StaticVariables.TotalScore += Mathf.FloorToInt(score);

                if (currStageState == StageState.CLEAR || currStageState == StageState.FAIL)
                {
                    resultClearScoreText.text = $"SCORE : {Mathf.FloorToInt(score).ToString("D6")}";
                    StageResult(currStageState == StageState.CLEAR ? true : false);
                }
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
		currMenu.SetActive(false);
        currMenu = pauseMenu;
        currMenu.SetActive(true);
    }

    public void StageResult(bool clear)
    {
		pauseUI.SetActive(true);
		
		if (clear)
		{
			GooglePlayManager.Instance.ClearAchievements();
			clearMenu.SetActive(true);
			currMenu = clearMenu;
		}

		else
		{
			if (!StaticVariables.DestroyAd)
				GoogleAdmobManager.Instance.Show();

			failMenu.SetActive(true);
			currMenu = failMenu;
		}
    }

    public void BackToMainMenu()
    {
		Resume();
        SceneManager.LoadScene((int)SceneIndex.MAIN);
    }

    public void NextStage()
    {
        Resume();
        ++StaticVariables.StageLevel;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Replay()
    {
        Resume();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void SoundEffectPlay(SoundEffectList seClip)
    {
        stageSE.Stop();
        stageSE.clip = seClips[(int)seClip];
        stageSE.Play();
    }

    public bool IsStageStopped()
    {
        return currStageState == StageState.CLEAR || 
               currStageState == StageState.FAIL;
    }
}
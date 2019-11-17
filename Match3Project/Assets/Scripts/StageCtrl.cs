using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StageCtrl : MonoBehaviour
{
    [Header("UI Menus")]
    public GameObject pauseUI;
    public GameObject optionUI;
    public GameObject clearUI;
    public GameObject failUI;

    private GameObject currMenuUI;

    [Header("UI Texts")]
    public Text scoreText;
    public Text moveText;
    public Text clearStageScoreText;
    public Text clearUIScoreText;

    [Header("UI Images")]
    public Slider bgmSlider;
    public Slider seSlider;
    public Slider scoreSlider;
    public Sprite starSprite;
    public Image[] clearStars;

    public GameObject nextButton;
    public GameObject thanksText;

    [Header("Stage Parts")]
    public Board board;
    public Image stageBG;
    public AudioSource stageBGM;
    public AudioSource stageSE;

    private StageDatas[] stageDatas;
    private AudioClip[] seClips;

    public float scoringSpeed = 0f;
    public int combo = 0;

    private float score;
    private float nextScore;
    private int clearScore;
    private int moveValue;
    private int stars = 0;

    public static StageCtrl instance;
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
        seClips = Resources.LoadAll<AudioClip>(Variables.stageSoundEffectPath);
        stageDatas = Resources.LoadAll<StageDatas>(Variables.stageDataPath);

        StageInit();
    }

    private void StageInit()
    {
        LoadGameData();

        clearStageScoreText.text = $"CLEAR SCORE : {Mathf.FloorToInt(clearScore).ToString("D6")}";
        scoreSlider.maxValue = clearScore;

        moveText.text = moveValue.ToString();
    }

    public void LoadGameData()
    {
        //stageData = JsonUtility.FromJson<StageData>(stageLevel[GameManager.instance.stageLevel].text);
        var stageData = StageLoad(Variables.stageLevel);

        clearScore = stageData.clearScore;
        moveValue = stageData.move;

        stageBG.sprite = stageData.backgroundSprite;
        stageBGM.clip = stageData.backgroundMusic;

        moveValue = stageData.move;
        clearScore = stageData.clearScore;
    }

    private void Update()
    {
        if (optionUI.activeInHierarchy)
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

            if (score >= nextScore - 10)
            {
                score = nextScore;
                scoreText.text = $"SCORE : {Mathf.FloorToInt(score).ToString("D6")}";
                scoreSlider.value = score;

                if(board.currBoardState == BoardState.CLEAR)
                    StageClear();

                else if (board.currBoardState == BoardState.FAIL)
                    StageFail();
            }
        }
    }

    public void CheckTheGameState()
    {
        if (board.currBoardState == BoardState.CLEAR || board.currBoardState == BoardState.FAIL)
            return;

        if (score >= clearScore * 0.5 && stars == 0)
            clearStars[stars++].sprite = starSprite;

        if (score >= clearScore * 0.75 && stars == 1)
            clearStars[stars++].sprite = starSprite;
        

        if (score >= clearScore && stars == 2)
        {
            clearStars[stars].sprite = starSprite;
            board.currBoardState = BoardState.CLEAR;
        }

        if (moveValue == 0)
        {
            if (board.currBoardState == BoardState.CLEAR && board.currBoardState == BoardState.FAIL)
                return;

            board.currBoardState = stars > 0 ? BoardState.CLEAR : BoardState.FAIL;
        }
    }

    public void IncreaseScore(int value)
    {
        nextScore += combo > 1 ? value * combo : value;
    }

    public void DecreaseMove(int value)
    {
        moveValue -= value;
        moveText.text = moveValue.ToString();
    }

    /// <summary>
    ///         <param name="seClip">
    ///         Stage의 Sound Effect를 재생합니다.
    ///         Effect Play Numbers [ 0 : Swap || 1 : Tunning || 2 : Matched ]
    ///         </param>
    /// </summary>
    public void SoundEffectPlay(SoundEffectList seClip)
    {
        stageSE.Stop();
        stageSE.clip = seClips[(int)seClip];
        stageSE.Play();
    }

    public StageDatas StageLoad(int level)
    {
        StageDatas stageData = stageDatas[level];

        return stageData;
    }

    public void StageClear()
    {
        clearUIScoreText.text = $"SCORE : {Mathf.FloorToInt(score).ToString("D6")}";
        clearUI.SetActive(true);

        currMenuUI = clearUI;
    }

    public void StageFail()
    {
        failUI.SetActive(true);

        currMenuUI = failUI;
    }

    public void NextGame()
    {
        BackMenu();
        ++Variables.stageLevel;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Replay()
    {
        BackMenu();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void BackMenu()
    {
        if (currMenuUI == optionUI)
        {
            currMenuUI.SetActive(false);
            pauseUI.SetActive(true);

            currMenuUI = pauseUI;
        }

        else if (currMenuUI == pauseUI)
        {
            Time.timeScale = 1;
            currMenuUI.SetActive(false);
        }
    }

    public void BackToMainMenu()
    {
        BackMenu();
        SceneManager.LoadScene((int)SceneIndex.MAIN);
    }
}
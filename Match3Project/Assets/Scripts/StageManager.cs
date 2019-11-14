using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;

[System.Serializable]
public class StageData
{
    public int stage;
    public int goalScore;
    public int move;
    public int bgIdx;
    public int bgmIdx;
}

public class StageManager : MonoBehaviour
{
    [Header("UI Menus")]
    public GameObject pauseUI;
    public GameObject optionUI;
    public GameObject clearUI;
    public GameObject failUI;

    private GameObject currMenuUI;

    [Header("UI Texts")]
    public Text scoreText;
    public Text goalScoreText;
    public Text moveText;
    public Text clearScore;

    [Header("UI Images")]
    public Slider scoreSlider;
    public Image[] starImages;
    public Sprite starSprite;

    [Header("Stage Parts")]
    public Image stageBG;
    public AudioSource stageBGM;

    public Slider bgmVolume;
    public Slider seVolume;

    private Sprite[] stageBGs;
    private AudioClip[] stageBGMs;

    public float scoringSpeed = 0f;
    public int combo = 0;

    private int moveValue;
    private int goalScore;
    private int stars = 0;

    private float nextScore;
    private float score;

    [Header("Others")]
    public GameObject nextButton;
    public GameObject thanksText;

    public Board board;

    private StageData stageData;
    public TextAsset[] stageLevel;

    public static StageManager instance;
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
        stageBGM = GetComponent<AudioSource>();

        stageBGMs = Resources.LoadAll<AudioClip>("Sounds/BGM");
        stageBGs = Resources.LoadAll<Sprite>("Arts/Background");

        StageInit();
    }

    private void StageInit()
    {
        LoadGameData();

        stageBGM.volume = GameManager.instance.bgmVolume;
        bgmVolume.value = GameManager.instance.bgmVolume;
        seVolume.value = GameManager.instance.seVolume;

        goalScoreText.text = "GOAL SCORE : " + Mathf.FloorToInt(goalScore).ToString("D8");
        scoreSlider.maxValue = goalScore;

        moveText.text = moveValue.ToString();

        stageBGM.Play();
    }

    public void LoadGameData()
    {
        stageData = JsonUtility.FromJson<StageData>(stageLevel[GameManager.instance.stageLevel].text);

        stageBG.sprite = stageBGs[stageData.bgIdx];
        stageBGM.clip = stageBGMs[stageData.bgmIdx];
        moveValue = stageData.move;
        goalScore = stageData.goalScore;
    }

    private void Update()
    {
        if (optionUI.activeInHierarchy)
        {
            stageBGM.volume = bgmVolume.value;
            return;
        }

        if (score != nextScore)
        {
            score = Mathf.Lerp(score, nextScore, Time.deltaTime * scoringSpeed);
            scoreText.text = "SCORE : " + Mathf.FloorToInt(score).ToString("D8");
            scoreSlider.value = score;

            CheckTheGameState();

            if (score >= nextScore - 10)
            {
                score = nextScore;
                scoreText.text = "SCORE : " + Mathf.FloorToInt(score).ToString("D8");
                scoreSlider.value = score;

                if(board.currState == BoardState.CLEAR)
                    StageClear();

                else if (board.currState == BoardState.FAIL)
                    StageFail();
            }
        }
    }

    public void CheckTheGameState()
    {
        if (board.currState == BoardState.CLEAR || board.currState == BoardState.FAIL)
            return;

        if (score >= goalScore * 0.5 && stars == 0)
            starImages[stars++].sprite = starSprite;

        if (score >= goalScore * 0.75 && stars == 1)
            starImages[stars++].sprite = starSprite;
        

        if (score >= goalScore && stars == 2)
        {
            starImages[stars].sprite = starSprite;
            board.currState = BoardState.CLEAR;
        }

        if (moveValue == 0 && board.currState != BoardState.CLEAR)
        {
            if(stars > 0 )
            { 
                board.currState = BoardState.CLEAR;
                return;
            }

            board.currState = BoardState.FAIL;
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

    public void PauseMenu()
    {
        if (board.currState == BoardState.CLEAR || board.currState == BoardState.FAIL)
            return;

        Time.timeScale = 0;

        pauseUI.SetActive(true);

        currMenuUI = pauseUI;
    }

    public void Option()
    {
        pauseUI.SetActive(false);
        optionUI.SetActive(true);

        currMenuUI = optionUI;
    }

    public void StageClear()
    {
        if (stageData.stage >= 5)
        {
            nextButton.SetActive(false);
            thanksText.SetActive(true);
        }

        clearScore.text = "SCORE : " + Mathf.FloorToInt(score).ToString("D8");
        clearUI.SetActive(true);

        currMenuUI = clearUI;
    }

    public void StageFail()
    {
        failUI.SetActive(true);

        currMenuUI = failUI;
    }

    public void MainMenu()
    {
        BackMenu();
        SceneManager.LoadScene("Main");
    }

    public void NextGame()
    {
        BackMenu();
        GameManager.instance.NextGame();
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
            GameManager.instance.bgmVolume = bgmVolume.value;
            GameManager.instance.seVolume = seVolume.value;

            board.AllPieceVolume();

            currMenuUI.SetActive(false);
            pauseUI.SetActive(true);

            currMenuUI = pauseUI;
        }

        else if(currMenuUI == pauseUI)
        {
            Time.timeScale = 1;
            currMenuUI.SetActive(false);
        }
    }
}
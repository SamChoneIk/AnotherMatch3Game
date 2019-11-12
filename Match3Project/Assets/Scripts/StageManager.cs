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

    public Text scoreText;
    public Text goalScoreText;
    public Text moveText;

    public Slider scoreSlider;
    public Slider volumeSlider;

    public Image[] starImages;
    public Sprite starSprite;


    [Header("Stage Parts")]
    public Image stageBG;
    public AudioSource stageBGM;

    private Sprite[] stageBGs;
    private AudioClip[] stageBGMs;

    public float scoringSpeed = 0f;
    public int combo = 0;

    private int moveValue;
    private int goalScore;
    private int stars = 0;

    private float nextScore;
    private float score;

    public Board board;

    private StageData stageData;
    public static StageManager instance;

    public TextAsset[] stageLevel;

    private void Awake()
    {
#if UNITY_EDITOR || UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE
        Camera.main.orthographicSize = 6;
#endif

#if UNITY_IOS || UNITY_IPHONE || UNITY_ANDROID || UNITY_WEBGL
        Camera.main.orthographicSize = 7;
#endif

        instance = this;

        stageBGMs = Resources.LoadAll<AudioClip>("Sounds/BGM");
        stageBGs = Resources.LoadAll<Sprite>("Arts/Background");
    }

    private void Start()
    {
        StageInit();
    }

    private void StageInit()
    {
        LoadGameData();

        stageBG.sprite = stageBGs[stageData.bgIdx];

        stageBGM = GetComponent<AudioSource>();
        stageBGM.clip = stageBGMs[stageData.bgmIdx];
        stageBGM.volume = GameManager.instance.bgmVolume;
        stageBGM.Play();

        moveValue = stageData.move;
        goalScore = stageData.goalScore;

        moveText.text = moveValue.ToString();
        goalScoreText.text = "GOAL SCORE : " + Mathf.FloorToInt(goalScore).ToString("D8");
        scoreSlider.maxValue = goalScore;
        volumeSlider.value = stageBGM.volume;
    }

    public void LoadGameData()
    {
        stageData = JsonUtility.FromJson<StageData>(stageLevel[GameManager.instance.stageLevel].text);
    }

    private void Update()
    {
        if (optionUI.activeInHierarchy)
        {
            stageBGM.volume = volumeSlider.value;
            return;
        }

        if (score != nextScore)
        {
            score = Mathf.Lerp(score, nextScore, Time.deltaTime * scoringSpeed);
            scoreText.text = "SCORE : " + Mathf.FloorToInt(score).ToString("D8");
            scoreSlider.value = score;

            if(score >= goalScore * 0.5)
                starImages[0].sprite = starSprite;

            if(score >= goalScore * 0.75 && stars == 1)
                starImages[1].sprite = starSprite;

            if (score >= goalScore)
            {
                starImages[2].sprite = starSprite;
                if (board.currState != BoardState.CLEAR)
                {
                    Debug.Log("clear");
                    board.currState = BoardState.CLEAR;
                }
            }

            if (moveValue == 0 && board.currState != BoardState.CLEAR)
            {
                if (stars > 0)
                {
                    Debug.Log("clear");
                    board.currState = BoardState.CLEAR;
                }

                if (board.currState != BoardState.FAIL)
                {
                    Debug.Log("FAIL");
                    board.currState = BoardState.FAIL;
                }
            }

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

    public void Replay()
    {
        BackMenu();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void BackMenu()
    {
        if (currMenuUI == optionUI)
        {
            GameManager.instance.bgmVolume = stageBGM.volume;
            currMenuUI.SetActive(false);
            pauseUI.SetActive(true);

            currMenuUI = pauseUI;
        }

        else
        {
            Time.timeScale = 1;
            currMenuUI.SetActive(false);
        }
    }
}
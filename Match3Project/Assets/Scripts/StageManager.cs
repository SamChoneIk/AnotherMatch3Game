using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
    public Text moveText;
    public Text clearStageScoreText;
    public Text clearUIScoreText;

    [Header("UI Images")]
    public Slider scoreSlider;
    public Slider bgmValue;
    public Slider seValue;

    public Sprite starSprite;
    public Image[] clearStars;

    [Header("Stage Parts")]
    private Image stageBG;
    private Sprite[] stageBGs;

    private AudioSource stageBGM;
    private AudioClip[] stageBGMs;

    private AudioSource stageSE;
    private AudioClip[] stageSEs; // 0 = ItemSwipe, 1 = Matched, 2 = swap, 3 = tunning

    public float scoringSpeed = 0f;
    public int combo = 0;

    private float score;
    private float nextScore;
    private int goalScore;
    private int stars = 0;
    private int moveValue;

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
        stageBG = GameObject.FindWithTag("BG").GetComponent<Image>();
        stageBGs = Resources.LoadAll<Sprite>("Arts/Background");

        stageBGM = GameObject.FindWithTag("BGM").GetComponent<AudioSource>();
        stageBGMs = Resources.LoadAll<AudioClip>("Sounds/BGM");

        stageSE = GameObject.FindWithTag("SE").GetComponent<AudioSource>();
        stageSEs = Resources.LoadAll<AudioClip>("Sounds/SE");

        StageInit();
    }

    private void StageInit()
    {
        LoadGameData();

        stageBGM.volume = GameManager.instance.bgmVolume;
        bgmValue.value = GameManager.instance.bgmVolume;

        stageSE.volume = GameManager.instance.seVolume;
        seValue.value = GameManager.instance.seVolume;

        clearStageScoreText.text = "GOAL SCORE : " + Mathf.FloorToInt(goalScore).ToString("D8");
        scoreSlider.maxValue = goalScore;

        moveText.text = moveValue.ToString();
    }

    public void LoadGameData()
    {
        stageData = JsonUtility.FromJson<StageData>(stageLevel[GameManager.instance.stageLevel].text);

        stageBG.sprite = stageBGs[stageData.bgIdx];

        stageBGM.clip = stageBGMs[stageData.bgmIdx];
        stageBGM.Play();

        moveValue = stageData.move;
        goalScore = stageData.goalScore;
    }

    private void Update()
    {
        if (optionUI.activeInHierarchy)
        {
            stageBGM.volume = bgmValue.value;
            stageSE.volume = seValue.value;

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
            clearStars[stars++].sprite = starSprite;

        if (score >= goalScore * 0.75 && stars == 1)
            clearStars[stars++].sprite = starSprite;
        

        if (score >= goalScore && stars == 2)
        {
            clearStars[stars].sprite = starSprite;
            board.currState = BoardState.CLEAR;
        }

        if (moveValue == 0)
        {
            if (board.currState == BoardState.CLEAR && board.currState == BoardState.FAIL)
                return;

            board.currState = stars > 0 ? BoardState.CLEAR : BoardState.FAIL;
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

        clearUIScoreText.text = "SCORE : " + Mathf.FloorToInt(score).ToString("D8");
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
            GameManager.instance.bgmVolume = bgmValue.value;
            GameManager.instance.seVolume = seValue.value;

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
    /// <summary>
    ///         <param name="index">
    ///         index is Sound Effect elemants.
    ///         Effect Play Numbers [ 0 : Swap || 1 : Tunning || 2 : Matched ]
    ///         </param>
    /// </summary>
    public void SoundEffectPlay(int index)
    {
        stageSE.Stop();
        stageSE.clip = stageSEs[index];
        stageSE.Play();
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public enum SceneIndex
{
    Logo,
    MainMenu,
    StageSelect,
    Game,
}

public enum BGMClip
{
    Title, SelectStage, Stage1, Stage2, Stage3, Stage4, Stage5,
}

public enum SEClip
{
    Swap, Tunning, Matched,
}

public class GameManager : Singleton<GameManager>
{
    [Header("Sounds")]
    public AudioSource gameBgm;
    public AudioClip[] gameBgmClip;

    public AudioSource gameSe;
    public AudioClip[] gameSeClip;

    [Header("Stage Data")]
    public StageDataScriptableObject stageData_SO;

    [Header("Android Plugin")]
    public GooglePlayManager googleMgr;
    public IAPManager iapMgr;
    public GoogleAdmobManager admobMgr;

    [Header("Loading UI")]
    public GameObject loadingBar;

    private SelectStageManager selectStage;
    private UIManager uIMgr;
    private MessageWindow messageWindow;

    private float delta;
    protected override void Awake()
    {
        base.Awake();

        PlayerSystemToJsonData.LoadPlayerSystemData();

        if (PlayerSystemToJsonData.playerData == null)
            PlayerSystemToJsonData.SavePlayerSystemData();

        googleMgr= GetComponentInChildren<GooglePlayManager>(true);
        googleMgr.InitializeGooglePlay();

        iapMgr = GetComponentInChildren<IAPManager>(true);
        iapMgr.InitializeUnityIAP();

        admobMgr = GetComponentInChildren<GoogleAdmobManager>(true);
        admobMgr.InitializeAdmob();

        if (stageData_SO == null)
            stageData_SO = Resources.Load<StageDataScriptableObject>(StaticVariables.StageDataPath);
    }

    public void VolumeControl(float bgm, float se)
    {
        gameBgm.volume = bgm;
        gameSe.volume = se;
    }

    private void Start()
    {
        uIMgr = UImenu.manager;
        messageWindow = uIMgr.GetWindow(Menus.Message) as MessageWindow;

        SceneLoad("Main");
    }

    public StageData GetStageDataWithLevel(int level)
    {
        return stageData_SO.stageDatas.Find(s => s.stageLevel == level);
    }

    public void StageResult(StageData stage, StageState result)
    {
        string title = "";
        string buttonUp = "";
        string buttonDown = "";

        MessageBuilder messageBuilder = new MessageBuilder();

        switch (result)
        {
            case StageState.Clear:
                StaticVariables.LoadLevel = stage.stageLevel + 1;
                messageBuilder.SetMessageTitle($"{StaticVariables.SelectStageName}{StaticVariables.Clear}");
                messageBuilder.SetMessageText(stage.lastClearScore.ToString("D8"));

                if (StaticVariables.LoadLevel < stageData_SO.stageDatas.Count)
                    messageBuilder.SetButtonsInfo(ButtonColor.YellowButton, StaticVariables.NextStage, () => SceneLoad("Game"));

                messageBuilder.SetButtonsInfo(ButtonColor.GreenButton, StaticVariables.Back, () => SceneLoad("StageSelect"));

                break;
            case StageState.Fail:

                break;
        }

        MessageWindow message = uIMgr.OnTheWindow(Menus.Message) as MessageWindow;
        message.m_build = messageBuilder.Build();
        message.OnStageMessage(true, stage);
    }

    public void SaveStageData(int level, int score, int stars)
    {
        GetStageDataWithLevel(level).SaveData(score, stars);
    }

    public void GameQuit()
    {
        MessageWindow message = uIMgr.OnTheWindow(Menus.Message) as MessageWindow;

        message.m_build = new MessageBuilder()
            .SetMessageTitle("게임 종료")
            .SetMessageText("게임을 종료하시겠습니까?")
            .SetButtonsInfo(ButtonColor.YellowButton, "QUIT", () => Application.Quit())
            .SetButtonsInfo(ButtonColor.GreenButton, "CANCEL", () => message.OnPopupMessage(false))
            .Build();

        message.OnPopupMessage(true);
    }

    public void SceneLoad(string scenes)
    {
        StartCoroutine(AsyncLoadScene(scenes));
    }

    IEnumerator AsyncLoadScene(string scenes)
    {
        AsyncOperation async = SceneManager.LoadSceneAsync(scenes, LoadSceneMode.Single);
        async.allowSceneActivation = false;

        //OnMessage(async.allowSceneActivation);

        loadingBar.SetActive(!async.allowSceneActivation);

        yield return new WaitForSeconds(1f);

        while (!async.isDone)
        {
            //Debug.Log($"async Progress : {async.progress}% nasync.allowSceneActivation = {async.allowSceneActivation}");

            if (async.progress >= 0.9f)
            {
                //if(uIMgr.currentMenuID != Menus.None)
                    //uIMgr.MenuClose();

                async.allowSceneActivation = true;
            }

            yield return null;
        }

        loadingBar.SetActive(!async.allowSceneActivation);
        uIMgr.MenuClose();
    }

    public void SoundEffectPlay(SEClip seClip)
    {
        gameSe.Stop();
        gameSe.clip = gameSeClip[(int)seClip];
        gameSe.Play();
    }

    public void SoundEffectPlay(AudioClip seClip)
    {
        gameSe.Stop();
        gameSe.clip = seClip;
        gameSe.Play();
    }

    public void BackgroundMusicPlay(BGMClip bgmClip)
    {
        gameBgm.Stop();
        gameBgm.clip = gameBgmClip[(int)bgmClip];
        gameBgm.Play();
    }

    public void BackgroundMusicPlay(AudioClip bgmClip)
    {
        gameBgm.Stop();
        gameBgm.clip = bgmClip;
        gameBgm.Play();
    }
}
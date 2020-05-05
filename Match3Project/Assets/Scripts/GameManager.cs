using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

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
    [Header("Stage Data")]
    public StageDataScriptableObject stageData_SO;

    [Header("Android Plugin")]
    public GooglePlayManager googleMgr;
    public IAPManager iapMgr;
    public GoogleAdmobManager admobMgr;

    [Header("Audio Components")]
    public AudioSource gameBgm;
    public AudioClip[] gameBgmClip;

    public AudioSource gameSe;
    public AudioClip[] gameSeClip;

    [Header("UI Components")]
    public GameObject loadingBar;

    private SelectStageManager selectStage;
    private UIManager uIMgr;

    private float delta;
    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        uIMgr = UImenu.manager;
        Application.targetFrameRate = 60;
        PlayerSystemToJsonData.LoadPlayerSystemData();

        googleMgr.InitializeGooglePlay();
        iapMgr.InitializeUnityIAP();
        admobMgr.InitializeAdmob();

        VolumeControl(PlayerSystemToJsonData.playerData.bgmVolume, PlayerSystemToJsonData.playerData.seVolume);

        if (stageData_SO == null)
            stageData_SO = Resources.Load<StageDataScriptableObject>(StaticVariables.StageDataPath);

        SceneLoad("Main");
    }

    public void VolumeControl(float bgm, float se)
    {
        gameBgm.volume = bgm;
        gameSe.volume = se;

        OptionWindow option = uIMgr.GetWindow(Menus.Option) as OptionWindow;

        option.bgmVolume.value = gameBgm.volume;
        option.seVolume.value = gameSe.volume;
    }

    public StageData GetStageDataWithLevel(int level)
    {
        return stageData_SO.stageDatas.Find(s => s.stageLevel == level);
    }

    public void StageResult(StageState result)
    {
        ClearStageData stageData = PlayerSystemToJsonData.playerData.GetStageData(StaticVariables.LoadLevel);
        MessageBuilder messageBuilder = new MessageBuilder();
        StringBuilder sb = new StringBuilder();
        sb.Append($"{StaticVariables.Score}{stageData.score.ToString("D8")}");
        
        switch (result)
        {
            case StageState.Clear:
                messageBuilder.SetMessageTitle($"{StaticVariables.SelectStageName} {StaticVariables.Clear}");
                googleMgr.ClearAchievements();
                if (StaticVariables.LoadLevel < stageData_SO.stageDatas.Count)
                {
                    ++StaticVariables.LoadLevel;
                    messageBuilder.SetButtonsInfo(ButtonColor.YellowButton, StaticVariables.NextStage, () => SceneLoad("Game"));
                }

                else
                    sb.Append("\nTHANK YOU FOR PLAY !!");

                messageBuilder.SetButtonsInfo(ButtonColor.GreenButton, StaticVariables.Back, () => SceneLoad("StageSelect"));
                break;

            case StageState.Fail:
                messageBuilder.SetMessageTitle($"{StaticVariables.SelectStageName} {StaticVariables.Fail}");
                messageBuilder.SetMessageText("다시 도전해보세요...");
                messageBuilder.SetButtonsInfo(ButtonColor.YellowButton, StaticVariables.Replay, () => SceneLoad("Game"));
                messageBuilder.SetButtonsInfo(ButtonColor.GreenButton, StaticVariables.Back, () => SceneLoad("StageSelect"));
                break;
        }
        messageBuilder.SetMessageText(sb.ToString());

        MessageWindow message = uIMgr.GetWindow(Menus.Message) as MessageWindow;
        message.m_build = messageBuilder.Build();
        message.OnStageMessage(true, stageData);
    }

    public void GameQuitMessage(bool isGame)
    {
        MessageWindow message = uIMgr.GetWindow(Menus.Message) as MessageWindow;

        if (isGame)
        {
            message.m_build = new MessageBuilder()
                            .SetMessageTitle("나가기")
                            .SetMessageText("메인 메뉴로 나가시겠습니까?")
                            .SetButtonsInfo(ButtonColor.YellowButton, "BACK TO GAME", () => SceneLoad("StageSelect"))
                            .SetButtonsInfo(ButtonColor.GreenButton, "CANCEL", () => message.OnPopupMessage(false))
                            .Build();

            message.OnPopupMessage(true);
        }

        else
        {
            message.m_build = new MessageBuilder()
                            .SetMessageTitle("게임 종료")
                            .SetMessageText("게임을 종료하시겠습니까?")
                            .SetButtonsInfo(ButtonColor.YellowButton, "QUIT", () => Application.Quit())
                            .SetButtonsInfo(ButtonColor.GreenButton, "CANCEL", () => message.OnPopupMessage(false))
                            .Build();

            message.OnPopupMessage(true);
        }
    }

    public void SceneLoad(string scenes)
    {
        StartCoroutine(AsyncLoadScene(scenes));
    }

    IEnumerator AsyncLoadScene(string scenes)
    {
        MessageWindow message = uIMgr.GetWindow(Menus.Message) as MessageWindow;

        if (message.gameObject.activeInHierarchy)
            message.OnStageMessage(false, PlayerSystemToJsonData.playerData.GetStageData(StaticVariables.LoadLevel));

        uIMgr.MenuClose();

        if (SceneManager.GetActiveScene().name != "Loading")
            SceneManager.LoadScene("Loading");
        loadingBar.SetActive(true);

        yield return new WaitForSeconds(2f);
        AsyncOperation async = SceneManager.LoadSceneAsync(scenes, LoadSceneMode.Single);
        async.allowSceneActivation = false;


        while (!async.isDone)
        {
            //Debug.Log($"async Progress : {async.progress}% nasync.allowSceneActivation = {async.allowSceneActivation}");

           if (async.progress >= 0.9f)
                async.allowSceneActivation = true;

            yield return null;
        }

        loadingBar.SetActive(false);
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

    public void OnApplicationQuit()
    {
        PlayerSystemToJsonData.SavePlayerSystemData();
    }
}
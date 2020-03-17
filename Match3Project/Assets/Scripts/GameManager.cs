using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class GameManager : Singleton<GameManager>
{
    [Header("Stage Data")]
    public StageDataScriptableObject stageData_SO;

    [Header("Android Plugin")]
    private GooglePlayManager googleMgr;
    private IAPManager iapMgr;
    private GoogleAdmobManager AdmobMgr;

    [Header("PopupMessage")]
    private PopupMessage popupMessage;
    public Message message => popupMessage.m_build;

    public GameObject loadingBar;
    public Camera cam;

    private float accumTime = 0f;

    protected override void Awake()
    {
        base.Awake();

        if(stageData_SO == null)
        stageData_SO = Resources.Load<StageDataScriptableObject>(StaticVariables.StageDataPath);

        googleMgr = GetComponentInChildren<GooglePlayManager>();
        googleMgr.InitializeGooglePlayManager();

        iapMgr = GetComponentInChildren<IAPManager>();
        iapMgr.InitializeUnityIAP();

        AdmobMgr = GetComponentInChildren<GoogleAdmobManager>();
        AdmobMgr.InitializeAdmob();

        popupMessage = GetComponentInChildren<PopupMessage>(true);
        popupMessage.InitializePopupMessage();
    }

    private void Start()
    {
        SceneLoad("Main");
    }

    public void SceneLoad(string scenes)
    {
        StartCoroutine(AsyncLoadScene(scenes));
    }

    IEnumerator AsyncLoadScene(string scenes)
    {
        AsyncOperation async = SceneManager.LoadSceneAsync(scenes, LoadSceneMode.Single);
        async.allowSceneActivation = false;

        OnMessage(async.allowSceneActivation);

        loadingBar.SetActive(!async.allowSceneActivation);

        while(!async.isDone || accumTime < 5.0f)
        {
            Debug.Log($"async Progress : {async.progress}% nasync.allowSceneActivation = {async.allowSceneActivation}");

            accumTime += Time.deltaTime;

            if (async.progress >= 0.9f)
                async.allowSceneActivation = true;

            yield return null;
        }

        loadingBar.SetActive(!async.allowSceneActivation);
    }

    public void OnMessage(bool display)
    {
        popupMessage.gameObject.SetActive(display);

        if (!display)
        {
            popupMessage.DestroyButton();
            return;
        }

        popupMessage.messageTitle.text = message.MessageTitle;
        popupMessage.messageText.text = message.MessageText;

        popupMessage.messageText.gameObject.SetActive(message.MessageType == MessageTypes.Normal);
        popupMessage.messageText.text = message.MessageType == MessageTypes.Normal ? message.MessageText : null;

        popupMessage.messageStar.gameObject.SetActive(message.MessageType == MessageTypes.Stage);

        popupMessage.CreateButton(message.GetButton());
    }

    public void SetMessage(Message message)
    {
        popupMessage.m_build = message;
    }

    public StageData GetStageLevel(int level)
    {
        StageData stage = stageData_SO.stageDatas.Find(st => st.stageLevel == level);
        return stage;
    }
}
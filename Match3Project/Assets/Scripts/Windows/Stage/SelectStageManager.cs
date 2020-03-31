using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SelectStageManager : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Select Stage Components")]
    public RectTransform stageNodesParent;
    public RectTransform stageBg;

    public Sprite clearStageSprite;

    private StageNode[] stageNodes;
    private StageNode currentNode;
    private StageNode nextNode;

    private Vector2 startPos;
    private UIManager uIMgr;

    [Header("Select Stage Variables")]
    public float duration = 0.02f;
    public float distance = 15.0f;
    public float clampX;

    private bool screenMoved;

    public void Start()
    {
        uIMgr = UImenu.manager;

        PauseWindow quitButton = uIMgr.GetWindow(Menus.Pause) as PauseWindow;
        quitButton.ChangedButton(SceneType.StageSelect);

        SetStageNode();

        GameManager.Instance.BackgroundMusicPlay(BGMClip.SelectStage);
        GameManager.Instance.googleMgr.RefreshAchievements();
    }

    private void SetStageNode()
    {
        stageNodes = stageNodesParent.GetComponentsInChildren<StageNode>();

        for (int i = 0; i < stageNodes.Length; ++i)
        {
            stageNodes[i].InitStageNode(PlayerSystemToJsonData.playerData.GetStageData(i+1));
        }
    }

    public void Update()
    {
        if (screenMoved)
        {
            stageBg.anchoredPosition = new Vector2(Mathf.Clamp(stageBg.anchoredPosition.x - Mathf.Clamp((startPos.x - Input.mousePosition.x) * Time.deltaTime, -clampX, clampX), 0f, stageBg.rect.width - Screen.width), 0f);

            return;
        }
    }

    public void GoogleLoginButton()
    {
        GameManager.Instance.googleMgr.GoogleLogInButton();
    }

    public void ShopButton()
    {
        uIMgr.OnTheWindow(Menus.Shop);
    }

    public void GoogleLeaderBoardButton()
    {
        GameManager.Instance.googleMgr.ShowLeaderBoard();
    }

    public void GoogleAchievementsButton()
    {
        GameManager.Instance.googleMgr.ShowAchievements();
    }

    public void PauseButton()
    {
        uIMgr.OnTheWindow(Menus.Pause);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        startPos = Input.mousePosition;

        screenMoved = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        screenMoved = false;
    }
}
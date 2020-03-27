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
    public RectTransform playerNode;
    public RectTransform stageBg;

    private StageNode[] stageNodes;
    private StageNode currentNode;
    private StageNode nextNode;

    private Vector2 startPos;
    private UIManager uIMgr;

    [Header("Select Stage Variables")]
    public float duration = 0.02f;
    public float distance = 15.0f;
    public float clampX;

    private bool isMove;
    private bool screenMoved;

    public void Start()
    {
        uIMgr = UImenu.manager;

        SetStageNode();
        SetPlayerNode();

        GameManager.Instance.BackgroundMusicPlay(BGMClip.SelectStage);
    }

    private void SetStageNode()
    {
        stageNodes = stageNodesParent.GetComponentsInChildren<StageNode>();

        for (int i = 0; i < stageNodes.Length; ++i)
        {
            stageNodes[i].InitStageNode(GameManager.Instance.GetStageDataWithLevel(i+1));
        }
    }

    private void SetPlayerNode()
    {
        if (StaticVariables.LoadLevel > 0)
        {
            playerNode.localPosition = stageNodes[StaticVariables.LoadLevel].transform.localPosition;

            if (StaticVariables.LoadLevel != 5)
                nextNode = stageNodes[StaticVariables.LoadLevel + 1];
        }

        else
        {
            playerNode.transform.position = stageNodes[0].transform.position;
            nextNode = stageNodes[1];
        }
    }

    public void Update()
    {
        if (screenMoved)
        {
            stageBg.anchoredPosition = new Vector2(Mathf.Clamp(stageBg.anchoredPosition.x - Mathf.Clamp((startPos.x - Input.mousePosition.x) * Time.deltaTime, -clampX, clampX), 0f, stageBg.rect.width - Screen.width), 0f);

            return;
        }

       if(isMove)
        {
            Debug.Log(Vector2.Distance(playerNode.transform.position, nextNode.transform.position));

            if(Vector2.Distance(playerNode.transform.position, nextNode.transform.position) > distance)
               playerNode.position = Vector2.Lerp(playerNode.position, nextNode.transform.position, Time.deltaTime + duration);

            else
            {
                playerNode.position = nextNode.transform.position;
                int idx = Array.FindIndex(stageNodes, n => n == nextNode) + 1;

                if (idx >= stageNodes.Length)
                    idx = 0;

                nextNode = stageNodes[idx];

                isMove = false;
            }
        }

        else
        {

        }
    }

    public void GoogleLoginButton()
    {
        GameManager.Instance.googleMgr.GoogleLogInButton();
    }

    public void ShopButton()
    {

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

    public void PlayerMoveStart()
    {
        StartCoroutine(PlayerMove());
    }

    private IEnumerator PlayerMove()
    {
        if (isMove)
            yield break;

        yield return new WaitForSeconds(1f);

        isMove = true;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isMove)
            return;

        startPos = Input.mousePosition;

        screenMoved = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isMove)
            return;

        screenMoved = false;
    }
}
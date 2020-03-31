using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageNode : MonoBehaviour
{
    public Image[] fillStars;

    private Image stageNodeImage;
    private Button stageNodeButton;

    private UIManager uImgr;
    public void InitStageNode(ClearStageData stage)
    {
        uImgr = UImenu.manager;

        for (int i = 0; i < stage.star; ++i)
        {
            fillStars[i].sprite = uImgr.fillStarSprite;
        }

        stageNodeImage = GetComponent<Image>();
        if (stage.score > 0)
            stageNodeImage.sprite = uImgr.clearStageNodeSprites[stage.level-1];

        else
            stageNodeImage.sprite = uImgr.firstStageNodeSprite;

        stageNodeButton = GetComponent<Button>();
        stageNodeButton.onClick.AddListener(() => SetStageMessage(stage));
    }

    public void SetStageMessage(ClearStageData stage)
    {
        StaticVariables.LoadLevel = stage.level;

        MessageWindow message = uImgr.GetWindow(Menus.Message) as MessageWindow;
        message.m_build = new MessageBuilder().
            SetMessageTitle(StaticVariables.SelectStageName).
            SetMessageText($"{StaticVariables.Score}{stage.score.ToString("D8")}").
            SetButtonsInfo(ButtonColor.YellowButton, StaticVariables.GameStart, () => GameManager.Instance.SceneLoad("Game")).
            SetButtonsInfo(ButtonColor.GreenButton, StaticVariables.Back, () => message.OnStageMessage(false, stage)).
            Build();

        message.OnStageMessage(true, stage);
    }
}
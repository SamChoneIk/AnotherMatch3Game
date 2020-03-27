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
    public void InitStageNode(StageData stage)
    {
        uImgr = UImenu.manager;

        for (int i = 0; i < stage.lastClearStar; ++i)
        {
            fillStars[i].sprite = stage.clearStarSprite;
        }

        stageNodeImage = GetComponent<Image>();
        if (stage.lastClearScore > 0)
            stageNodeImage.sprite = stage.clearNodeSprite;

        else
            stageNodeImage.sprite = stage.firstNodeSprite;

        stageNodeButton = GetComponent<Button>();
        stageNodeButton.onClick.AddListener(() => SetStageMessage(stage));
    }

    public void SetStageMessage(StageData stage)
    {
        StaticVariables.LoadLevel = stage.stageLevel;

        MessageWindow message = uImgr.OnTheWindow(Menus.Message) as MessageWindow;
        message.m_build = new MessageBuilder().
            SetMessageTitle(StaticVariables.SelectStageName).
            SetMessageText(stage.lastClearScore.ToString("D8")).
            SetButtonsInfo(ButtonColor.YellowButton, StaticVariables.GameStart, () => GameManager.Instance.SceneLoad("Game")).
            SetButtonsInfo(ButtonColor.GreenButton, StaticVariables.Back, () => message.OnStageMessage(false, stage)).
            Build();

        message.OnStageMessage(true, stage);
    }
}
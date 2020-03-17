using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageNode : MonoBehaviour
{
    public Image[] fillStars;

    private Button stageButton;

    public void InitStageNode()
    {
        stageButton = GetComponent<Button>();

        GameManager.Instance.SetMessage(StageMessage());
        stageButton.onClick.AddListener(() => GameManager.Instance.OnMessage(true));
    }

    private Message StageMessage()
    {
        return new MessageBuilder()
            .SetMessageType(MessageTypes.Stage)
            .SetMessageTitle("STAGE 1")
            .SetButtonsInfo(ButtonColor.YellowButton, "GAMESTART", () => GameManager.Instance.SceneLoad("Game"))
            .SetButtonsInfo(ButtonColor.GreenButton, "CANCEL", () => GameManager.Instance.OnMessage(false))
            .Build();
    }
}
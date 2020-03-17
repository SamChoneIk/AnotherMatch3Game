using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public enum MessageTypes
{
    Normal, Stage,
}

public enum ButtonColor
{
    BlueButton, GreenButton, YellowButton,
}

public class Message
{
    private MessageTypes messageType;
    public MessageTypes MessageType => messageType;

    private string messageTitle;
    public string MessageTitle => messageTitle;

    private string messageText;
    public string MessageText => messageText;

    private List<ButtonInfo> buttons;

    public Message(MessageTypes m_type, string m_title, string m_text, List<ButtonInfo> btns)
    {
        messageType = m_type;
        messageTitle = m_title;
        messageText = m_text;

        buttons = new List<ButtonInfo>();
        buttons.AddRange(btns);
    }

    public List<ButtonInfo> GetButton()
    {
        return buttons;
    }
}

public class MessageBuilder
{
    private MessageTypes messageType;
    private string messageTitle;
    private string messageText;

    private List<ButtonInfo> buttons;

    public MessageBuilder()
    {
        buttons = new List<ButtonInfo>();
    }

    public MessageBuilder SetMessageType(MessageTypes m_type)
    {
        messageType = m_type;
        return this;
    }

    public MessageBuilder SetMessageTitle(string m_title)
    {
        messageTitle = m_title;
        return this;
    }

    public MessageBuilder SetMessageText(string m_text)
    {
        messageText = m_text;
        return this;
    }

    public MessageBuilder SetButtonsInfo(ButtonColor b_color, string b_text, UnityAction b_action)
    {
        buttons.Add(new ButtonInfo(b_color, b_text, b_action));
        return this;
    }

    public Message Build()
    {
        Message messageBuild = new Message(messageType, messageTitle, messageText, buttons);
        return messageBuild;
    }
}

public class ButtonInfo
{
    public ButtonColor buttonColor;

    public string buttonText;
    public bool buttonOn => string.IsNullOrEmpty(buttonText);

    public UnityAction currentButtonAction;

    public ButtonInfo(ButtonColor b_color, string b_text, UnityAction b_action)
    {
        buttonColor = b_color;
        buttonText = b_text;
        currentButtonAction = b_action;
    }
}

public class PopupMessage : MonoBehaviour
{
    [Header("Message Contents")]
    public Text messageTitle;
    public Text messageText;

    public GameObject messageStar;
    public Image[] clearStar;

    public GameObject[] buttonPrefabs;
    public RectTransform buttonParent;

    private List<Button> currButtons;

    [HideInInspector]
    public Message m_build;

    public void InitializePopupMessage()
    {
        currButtons = new List<Button>();
        OnMessage(false);

        //MessageDebug();
    }

    public void OnMessage(bool display)
    {
        gameObject.SetActive(display);

        if (!display)
        {
            DestroyButton();
            return;
        }

        messageTitle.text = m_build.MessageTitle;

        messageText.gameObject.SetActive(m_build.MessageType == MessageTypes.Normal);
        messageText.text = m_build.MessageType == MessageTypes.Normal ? m_build.MessageText : null;

        messageStar.gameObject.SetActive(m_build.MessageType == MessageTypes.Stage);
        CreateButton(m_build.GetButton());
    }

    public void CreateButton(List<ButtonInfo> btns)
    {
        for (int i = 0; i < btns.Count; ++i)
        {
            Button button = Instantiate(SelectButton(btns[i]), buttonParent).GetComponent<Button>();
            button.onClick.AddListener(btns[i].currentButtonAction);

            Text button_text = button.GetComponentInChildren<Text>();
            button_text.text = btns[i].buttonText;

            currButtons.Add(button);
        }
    }

    public void DestroyButton()
    {
        if (currButtons.Count == 0)
            return;

        for (int i = currButtons.Count - 1; 0 <= i; --i)
        {
            Destroy(currButtons[i].gameObject);
        }

        currButtons.Clear();
    }

    public GameObject SelectButton(ButtonInfo button)
    {
        for(int i = 0; i < buttonPrefabs.Length; ++i)
        {
            string color = Enum.GetName(typeof(ButtonColor), button.buttonColor);

            if (buttonPrefabs[i].name == color)
                return buttonPrefabs[i];
        }

        return null;
    }

    

    public void MessageDebug()
    {
        m_build = new MessageBuilder()
            .SetMessageTitle("디버그 메세지")
            .SetMessageText("디버그 메세지입니다.")
            .SetButtonsInfo(ButtonColor.YellowButton, "확인", () => OnMessage(false))
            .SetButtonsInfo(ButtonColor.GreenButton, "취소", () => OnMessage(false))
            .Build();

        OnMessage(true);
    }
}
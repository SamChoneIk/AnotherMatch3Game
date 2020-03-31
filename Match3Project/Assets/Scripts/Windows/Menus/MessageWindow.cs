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
    BlueButton, GreenButton, YellowButton, PuppleButton, WhiteButton
}

public class Message
{
    private MessageTypes messageType;
    public MessageTypes MessageType => messageType;

    private string messageTitle;
    public string MessageTitle => messageTitle;

    private string messageText;
    public string MessageText => messageText;

    private List<CustomButton> buttons;

    public Message(MessageTypes m_type, string m_title, string m_text, List<CustomButton> btns)
    {
        messageType = m_type;
        messageTitle = m_title;
        messageText = m_text;

        buttons = new List<CustomButton>();
        buttons.AddRange(btns);
    }

    public List<CustomButton> GetButton()
    {
        return buttons;
    }
}

public class MessageBuilder
{
    private MessageTypes messageType;
    private string messageTitle;
    private string messageText;

    private List<CustomButton> buttons;

    public MessageBuilder()
    {
        buttons = new List<CustomButton>();
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
        buttons.Add(new CustomButton(b_color, b_text, b_action));
        return this;
    }

    public Message Build()
    {
        Message messageBuild = new Message(messageType, messageTitle, messageText, buttons);
        return messageBuild;
    }
}

public class CustomButton
{
    public ButtonColor buttonColor;

    public string buttonText;
    public bool buttonOn => string.IsNullOrEmpty(buttonText);

    public UnityAction currentButtonAction;

    public CustomButton(ButtonColor b_color, string b_text, UnityAction b_action)
    {
        buttonColor = b_color;
        buttonText = b_text;
        currentButtonAction = b_action;
    }
}

public class MessageWindow : UImenu
{
    [Header("PopupMessage Contents")]
    public GameObject popupMessage;
    public Text popupMessageText;

    public RectTransform popupMessageButtonParent;

    [Header("StageMessage Contents")]
    public GameObject stageMessage;

    public Image[] stageMessageFillStars;
    public Text stageMessageScore;

    public RectTransform stageMessageButtonParent;

    [HideInInspector]
    public Message m_build;
    public GameObject[] buttonPrefabs;
    private List<Button> currButtons = new List<Button>();

    protected override void Awake()
    {
        base.Awake();
    }

    public void OnPopupMessage(bool display)
    {
        if (display)
        {
            Open();

            if (stageMessage.activeInHierarchy)
                stageMessage.SetActive(false);

            popupMessage.SetActive(display);

            windowsTitleText.text = m_build.MessageTitle;
            popupMessageText.text = m_build.MessageText;

            CreateButton(m_build.GetButton(), false);

        }

        else
        {
            DestroyButton();
            Close();
            return;
        }
    }

    public void OnStageMessage(bool display, ClearStageData stageData)
    {
        if (display)
        {
            Open();

            if (popupMessage.activeInHierarchy)
                popupMessage.SetActive(false);

            stageMessage.gameObject.SetActive(display);

            windowsTitleText.text = m_build.MessageTitle;
            stageMessageScore.text = $"{m_build.MessageText}";

            for (int i = 0; i < stageData.star; ++i)
            {
                stageMessageFillStars[i].sprite = manager.fillStarSprite;
            }

            CreateButton(m_build.GetButton(), true);
        }

        else
        {
            for (int i = 0; i < 3; ++i)
            {
                stageMessageFillStars[i].sprite = manager.emptyStarSprite;
            }

            DestroyButton();
            Close();
        }
    }

    public void CreateButton(List<CustomButton> btns, bool isStage)
    {
        for (int i = 0; i < btns.Count; ++i)
        {
            Button button = Instantiate(SelectButton(btns[i]), isStage == true ? stageMessageButtonParent : popupMessageButtonParent).GetComponent<Button>();
            button.onClick.AddListener(btns[i].currentButtonAction);

            Text button_text = button.GetComponentInChildren<Text>();
            button_text.text = btns[i].buttonText;

            currButtons.Add(button);
        }
    }

    public GameObject SelectButton(CustomButton button)
    {
        for (int i = 0; i < buttonPrefabs.Length; ++i)
        {
            string color = Enum.GetName(typeof(ButtonColor), button.buttonColor);

            if (buttonPrefabs[i].name == color)
                return buttonPrefabs[i];
        }

        return null;
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

    public override void OnFocus()
    {
        firstSelected = gameObject;
        base.OnFocus();
    }

    public override void Open()
    {
        base.Open();
    }

    public override void Close()
    {
        base.Close();
    }
}
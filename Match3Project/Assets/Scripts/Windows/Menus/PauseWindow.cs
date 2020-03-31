using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public enum SceneType
{
    StageSelect,
    Game,
}

public class PauseWindow : UImenu
{
    public Button quitButton;
    public Text buttonName;
    private UnityAction action;

    protected override void Awake()
    {
        base.Awake();
    }

    public void ChangedButton(SceneType Scene)
    {
        if (action != null)
        {
            quitButton.onClick.RemoveListener(action);

            if(Scene == SceneType.Game && action == GameStageQuit)
                action -= GameStageQuit;

            else if (Scene == SceneType.StageSelect && action == SelectStageQuit)
                action -= SelectStageQuit;
        }

        switch (Scene)
        {
            case SceneType.StageSelect:
                    buttonName.text = StaticVariables.Quit;
                    action -= GameStageQuit;
                    action += SelectStageQuit;
                break;

            case SceneType.Game:
                    buttonName.text = StaticVariables.BackToMenu;
                    action -= SelectStageQuit;
                    action += GameStageQuit;
                break;
        }

        quitButton.onClick.AddListener(action);
    }

    private void SelectStageQuit()
    {
        GameManager.Instance.GameQuitMessage(false);
    }

    private void GameStageQuit()
    {
        GameManager.Instance.GameQuitMessage(true);
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

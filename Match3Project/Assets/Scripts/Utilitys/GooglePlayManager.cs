using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GooglePlayGames;
using UnityEngine.UI;

public class GooglePlayManager : MonoBehaviour
{
    public Text logText;

    void Start()
    {
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
        LogIn();
    }

    public void LogIn()
    {
        Social.localUser.Authenticate((bool success) =>
        {
            if (success) logText.text = Social.localUser.id + " \n " + Social.localUser.userName;
            else logText.text = "구글 로그인 실패";
        });
    }

    public void LogOut()
    {
        ((PlayGamesPlatform)Social.Active).SignOut();
        logText.text = "구글 로그아웃";
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SocialPlatforms;
using GooglePlayGames;
using GooglePlayGames.BasicApi;

public class GooglePlayManager : MonoBehaviour
{
    public Text logText;
    private bool tryLogin = false;

    private void Awake()
    {
        PlayGamesPlatform.Activate();
        if (!tryLogin)
        {
            LogIn();
            tryLogin = false;
        }
    }

    private void LogIn()
    {
        Social.localUser.Authenticate((bool success) =>
        {
            if (success) logText.text += "${Social.localUser.id}\nSocial.localUser.userName\n";
            else logText.text += "구글 로그인 실패\n";
        });
    }

    private void LogOut()
    {
        ((PlayGamesPlatform)Social.Active).SignOut();
        logText.text += "구글 로그아웃\n";
    }

    private void ShowAchievements()
    {
        Social.ShowAchievementsUI();
    }
}
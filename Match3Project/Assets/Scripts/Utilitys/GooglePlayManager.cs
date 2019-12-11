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

    private void Awake()
    {
        PlayGamesPlatform.Activate();
        LogIn();
    }

    public void LogIn()
    {
        Social.localUser.Authenticate((bool success) =>
        {
            if (success)
            {
                logText.text += $"환영합니다! {Social.localUser.id}\n";
                IAPManager.Instance.RestorePurchase();
            }

            else
                logText.text += "구글 로그인 실패\n";
        });
    }

    public void LogOut()
    {
        ((PlayGamesPlatform)Social.Active).SignOut();
        logText.text += "구글 로그아웃\n";
    }

    public void ShowAchievements()
    {
        logText.text += "업적을 확인합니다.\n";
       Social.ShowAchievementsUI();
    }

    public void ShowLeaderBoard()
    {
        logText.text += "리더보드를 확인합니다.\n";
        Social.ShowLeaderboardUI();
    }
}
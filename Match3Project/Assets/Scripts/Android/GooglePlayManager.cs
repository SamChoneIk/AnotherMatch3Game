using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SocialPlatforms;
using GooglePlayGames;
using GooglePlayGames.BasicApi;

public class GooglePlayManager : MonoBehaviour
{
    public void InitializeGooglePlay()
    {
        PlayGamesPlatform.Activate();
        LogIn();
    }

    private void LogIn()
    {
        Social.localUser.Authenticate((success) =>
        {
            StaticVariables.LoginSuccess = success;
            Debug.Log($"로그인 결과 : {StaticVariables.LoginSuccess}");
        });
    }

    public void LogOut()
    {
        ((PlayGamesPlatform)Social.Active).SignOut();
    }

    public void GoogleLogInButton()
    {
        LogIn();
    }

    public void ShowLeaderBoard()
    {
        Social.ShowLeaderboardUI();
    }

    public void ShowAchievements()
    {
        Social.ShowAchievementsUI();
    }

    public void RefreshAchievements()
    {
        Debug.Log("playerScore 새로고침");
        Social.ReportScore(StaticVariables.TotalScore, GPGSIds.leaderboard_playerscore, null);
    }

    public void ClearAchievements()
    {
        Social.ReportProgress(GPGSIds.achievement_first_game_clear, 100f, null);

        switch (StaticVariables.LoadLevel)
        {
            case 1:
                Social.ReportProgress(GPGSIds.achievement_stage_1_clear, 100f, null);
                Debug.Log("스테이지1 클리어");
                break;

            case 2:
                Social.ReportProgress(GPGSIds.achievement_stage_2_clear, 100f, null);
                Debug.Log("스테이지2 클리어");
                break;

            case 3:
                Social.ReportProgress(GPGSIds.achievement_stage_3_clear, 100f, null);
                Debug.Log("스테이지3 클리어");
                break;

            case 4:
                Social.ReportProgress(GPGSIds.achievement_stage_4_clear, 100f, null);
                Debug.Log("스테이지4 클리어");
                break;

            case 5:
                Social.ReportProgress(GPGSIds.achievement_stage_5_clear, 100f, null);
                Debug.Log("스테이지5 클리어");
                break;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SocialPlatforms;
using GooglePlayGames;
using GooglePlayGames.BasicApi;

public class GooglePlayManager : MonoBehaviour
{
	public void InitializeGooglePlayManager()
	{
		PlayGamesPlatform.Activate();
		LogIn();
	}

    public void LogIn()
    {
        Social.localUser.Authenticate((success) =>
        {
            //IAPManager.Instance.GoogleLogin(success);
        });
    }

    public void LogOut()
    {
        ((PlayGamesPlatform)Social.Active).SignOut();
    }

	public void GoogleLogInButton()
	{
		if (Social.localUser.authenticated)
			LogOut();

		LogIn();
	}

	public void RefreshAchievements()
	{
		Social.ReportScore(StaticVariables.TotalScore, GPGSIds.leaderboard_playerscore, null);
	}

	public void ClearAchievements()
	{
		Social.ReportProgress(GPGSIds.achievement_first_game_clear, 100f, null);

		switch (StaticVariables.StageLevel)
		{
			case 1:
				{
					Social.ReportProgress(GPGSIds.achievement_stage_1_clear, 100f, null);
					break;
				}

			case 2:
				{
					Social.ReportProgress(GPGSIds.achievement_stage_2_clear, 100f, null);
					break;
				}

			case 3:
				{
					Social.ReportProgress(GPGSIds.achievement_stage_3_clear, 100f, null);
					break;
				}

			case 4:
				{
					Social.ReportProgress(GPGSIds.achievement_stage_4_clear, 100f, null);
					break;
				}

			case 5:
				{
					Social.ReportProgress(GPGSIds.achievement_stage_5_clear, 100f, null);
					break;
				}
		}
	}
}
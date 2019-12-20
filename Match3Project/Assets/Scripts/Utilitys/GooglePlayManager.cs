using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SocialPlatforms;
using GooglePlayGames;
using GooglePlayGames.BasicApi;

public class GooglePlayManager : MonoBehaviour
{
	private static GooglePlayManager instance;
	public static GooglePlayManager Instance
	{
		get
		{
			if (instance != null)
				return instance;

			instance = FindObjectOfType<GooglePlayManager>();

			if (instance == null)
				instance = new GameObject(name: "GooglePlayManager").AddComponent<GooglePlayManager>();

			return instance;
		}
	}

	public void GooglePlayManagerInit()
	{
		if (Social.localUser.authenticated)
		{
			GameManager.Instance.WriteLog($"이미 로그인이 되어있습니다. {Social.localUser.id}\n");
			return;
		}

		PlayGamesPlatform.Activate();
		LogIn();
	}

    public void LogIn()
    {
        Social.localUser.Authenticate((success) =>
        {
            if (success)
            {
				GameManager.Instance.WriteLog($"환영합니다! {Social.localUser.id}\n");
			}

            else
            {
				GameManager.Instance.WriteLog("구글 로그인 실패\n");
            }

            IAPManager.Instance.GoogleLogin(success);
        });
    }

    public void LogOut()
    {
        ((PlayGamesPlatform)Social.Active).SignOut();
		GameManager.Instance.WriteLog("구글 로그아웃\n");
    }

	public void GoogleLogInButton()
	{
		if (Social.localUser.authenticated)
			LogOut();

		LogIn();
	}

	public void RefreshAchievements()
	{
		Social.ReportScore(StaticVariables.TotalScore, GPGSIds.leaderboard_playerscore, (_success) =>
		{
			GameManager.Instance.WriteLog($"ReportScore : {StaticVariables.TotalScore}, {_success} \n");

			if (_success)
				GameManager.Instance.WriteLog("Reported score successfully\n");
			else
				GameManager.Instance.WriteLog("Failed to report score\n");
		});
	}

	public void ClearAchievements()
	{
		Social.ReportProgress(GPGSIds.achievement_first_game_clear, 100f, (bool success) =>
		{
			if (success)
			{
				GameManager.Instance.WriteLog("처음으로 스테이지를 완료했습니다.\n");
			}

			else
			{
				GameManager.Instance.WriteLog("로그인이 되어있지 않거나 업적을 이미 클리어했습니다.\n");
			}

		});

		switch (StaticVariables.StageLevel)
		{
			case 1:
				{
					Social.ReportProgress(GPGSIds.achievement_stage_1_clear, 100f, (bool success) =>
					{
						if (success)
						{
							GameManager.Instance.WriteLog("1 스테이지를 완료했습니다.\n");
						}

						else
						{
							GameManager.Instance.WriteLog("로그인이 되어있지 않거나 업적을 이미 클리어했습니다.\n");
						}

					});

					break;
				}
			case 2:
				{
					Social.ReportProgress(GPGSIds.achievement_stage_2_clear, 100f, (bool success) =>
					{
						if (success)
						{
							GameManager.Instance.WriteLog("2 스테이지를 완료했습니다.\n");
						}

						else
						{
							GameManager.Instance.WriteLog("로그인이 되어있지 않거나 업적을 이미 클리어했습니다.\n");
						}

					});

					break;
				}
			case 3:
				{
					Social.ReportProgress(GPGSIds.achievement_stage_3_clear, 100f, (bool success) =>
					{
						if (success)
						{
							GameManager.Instance.WriteLog("3 스테이지를 완료했습니다.\n");
						}

						else
						{
							GameManager.Instance.WriteLog("로그인이 되어있지 않거나 업적을 이미 클리어했습니다.\n");
						}

					});

					break;
				}
			case 4:
				{
					Social.ReportProgress(GPGSIds.achievement_stage_4_clear, 100f, (bool success) =>
					{
						if (success)
						{
							GameManager.Instance.WriteLog("4 스테이지를 완료했습니다.\n");
						}

						else
						{
							GameManager.Instance.WriteLog("로그인이 되어있지 않거나 업적을 이미 클리어했습니다.\n");
						}

					});

					break;
				}
			case 5:
				{
					Social.ReportProgress(GPGSIds.achievement_stage_5_clear, 100f, (bool success) =>
					{
						if (success)
						{
							GameManager.Instance.WriteLog("5 스테이지를 완료했습니다.\n");
						}

						else
						{
							GameManager.Instance.WriteLog("로그인이 되어있지 않거나 업적을 이미 클리어했습니다.\n");
						}

					});

					break;
				}
		}
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	public Text logText;

	private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if (instance != null)
                return instance;

            instance = FindObjectOfType<GameManager>();

            if (instance == null)
                instance = new GameObject(name: "GameManager").AddComponent<GameManager>();

            return instance;
        }
    }

	private void Awake()
	{
		if (Social.localUser.authenticated)
		{
			WriteLog("구글 로그인\n");
			if (IAPManager.Instance.isInitialized)
				WriteLog("IAP 초기화 완료\n");
			if (StaticVariables.DestroyAd)
				GoogleAdmobManager.Instance.DestroyAd();
		}
	
		var findGameManager = FindObjectOfType<GameManager>();
		if (findGameManager != this)
		{
			Destroy(gameObject);
			return;
		}

		DontDestroyOnLoad(gameObject);

		PlayerSystemToJsonData.Instance.LoadPlayerSystemData();
		GooglePlayManager.Instance.GooglePlayManagerInit();
	}

    public void WriteLog(string text)
    {
		logText.text += text;
    }
}
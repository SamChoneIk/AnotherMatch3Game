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
        var findGameManager = FindObjectOfType<GameManager>();
        if (findGameManager != this)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        PlayerSystemToJsonData.Instance.LoadPlayerSystemData();
    }

    public void WriteLog(string text)
    {
		logText.text += text;
    }
}
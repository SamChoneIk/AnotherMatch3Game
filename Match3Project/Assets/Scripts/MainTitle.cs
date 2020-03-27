using UnityEngine;

public class MainTitle : MonoBehaviour
{
    private void Start()
    {
        GameManager.Instance.BackgroundMusicPlay(BGMClip.Title);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            GameManager.Instance.SceneLoad("StageSelect");
    }
}
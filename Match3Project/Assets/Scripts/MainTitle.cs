using UnityEngine;

public class MainTitle : MonoBehaviour
{
    private void Start()
    {
        GameManager.Instance.BackgroundMusicPlay(BGMClip.Title);
    }

    private void Update()
    {
        if (!StaticVariables.GameStarts && Input.GetMouseButtonDown(0))
        {
            StaticVariables.GameStarts = !StaticVariables.GameStarts;
            GameManager.Instance.SceneLoad("StageSelect");
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class StageData
{
    public int stage;
    public int goalScore;
    public int move;
    public int bgIdx;
    public int bgmIdx;
}

[ExecuteInEditMode]
public class CreateStageToJson : MonoBehaviour
{
    public string fileName;

    public bool create = false;

    public StageData stageData;

    private void Update()
    {
        if(!create)
            return;

        create = false;

        string jsonData = JsonUtility.ToJson(stageData);
        string path = SaveLocation();

        if(string.IsNullOrEmpty(fileName))
            fileName = "stage" + stageData.stage + ".json";

        else
            fileName += ".json";

        path += fileName;

        File.WriteAllText(path, jsonData);
        Debug.Log("Create path");
    }

    private string SaveLocation()
    {
        string path = Application.streamingAssetsPath + "/stage/";

        if(!Directory.Exists(path))
            Directory.CreateDirectory(path);

        return path;
    }
}
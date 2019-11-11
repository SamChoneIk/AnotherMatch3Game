using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

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
            fileName = "Stage " + stageData.stage + " Data.json";

        else
            fileName += ".json";

        path += fileName;

        File.WriteAllText(path, jsonData);        
    }

    private string SaveLocation()
    {
        string path = Application.streamingAssetsPath + "/Stages/";

        if(!Directory.Exists(path))
            Directory.CreateDirectory(path);

        return path;
    }
}
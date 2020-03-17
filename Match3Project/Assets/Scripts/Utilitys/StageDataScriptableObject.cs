using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New StageData", menuName = "Stage/Create a StageData")]
public class StageDataScriptableObject : ScriptableObject
{
    public List<StageData> stageDatas = new List<StageData>();
}
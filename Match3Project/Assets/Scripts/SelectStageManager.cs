using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectStageManager : MonoBehaviour
{
    public Transform stageNodeParent;
    public Transform playerNode;

    public float duration = 0.02f;
    public float distance = 15.0f;

    public bool isMove;

    private StageNode[] stageNodes;
    private StageNode nextNode;
    private float delta;

    public void Start()
    {
        stageNodes = stageNodeParent.GetComponentsInChildren<StageNode>();
        SetStageNode();

        playerNode.transform.position = stageNodes[0].transform.position;
        nextNode = stageNodes[1];
    }

    private void SetStageNode()
    {
        for(int i = 0; i < stageNodes.Length; ++i)
        {
            stageNodes[i].InitStageNode();
        }
    }

    public void Update()
    {
        delta = Time.deltaTime;

        if(isMove)
        {
            Debug.Log(Vector2.Distance(playerNode.transform.position, nextNode.transform.position));

            if(Vector2.Distance(playerNode.transform.position, nextNode.transform.position) > distance)
            {
               playerNode.position = Vector2.Lerp(playerNode.position, nextNode.transform.position, delta + duration);
               // playerNode.position = Parabola(playerNode.transform.position, nextNode.transform.position, h, delta + duration);
            }

            else
            {
                playerNode.position = nextNode.transform.position;
                int idx = Array.FindIndex(stageNodes, n => n == nextNode) + 1;

                if (idx >= stageNodes.Length)
                    idx = 0;

                nextNode = stageNodes[idx];

                isMove = false;
            }
        }
    }

    public void PlayerMove()
    {
        if (isMove)
            return;

        isMove = true;
    }
}
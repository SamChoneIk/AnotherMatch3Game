using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamPos : MonoBehaviour
{
    private Board board;
    public float offset;
    public float aspectratio = 0.625f;
    public float padding = 2f;

    void Start()
    {
        board = FindObjectOfType<Board>();

        if (board != null)
            CameraPositioning(board.width, board.height);
    }

    private void CameraPositioning(float x, float y)
    {
        Vector3 camPos = new Vector3(x / 2, y / 2, offset);
        transform.position = camPos;
        if(board.width >= board.height)
        {
            Camera.main.orthographicSize = (board.width / 2 + padding) / aspectratio;
        }
        else
        {
            Camera.main.orthographicSize = (board.height / 2 + padding);
        }
    }
}

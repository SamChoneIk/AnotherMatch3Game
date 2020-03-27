using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamPivot : MonoBehaviour
{
    private Camera cam;

    public void SetCameraPivot(float row, float Column)
    {
        transform.position = new Vector3((row / 2) - 0.5f, 
                                     (Column / 2), -10);

        cam = GetComponent<Camera>();
        cam.orthographicSize = (transform.position.x + transform.position.y);
    }
}

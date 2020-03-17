using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamPivot : MonoBehaviour
{
    private Transform camTr;
    private Camera cam;

    public float xCapacity;
    public float yCapacity;
    public float orthographicSizeCapacity;

    public void SetCameraPivot(float row, float Column)
    {
        camTr = GetComponent<Transform>();
        camTr.position = new Vector3(((row / 2) - 0.5f) + xCapacity, 
                                     ((Column / 2) - 0.5f + yCapacity), -10);

        cam = GetComponent<Camera>();
        cam.orthographicSize = (((camTr.position.x + camTr.position.y) - 1.5f) + orthographicSizeCapacity);
    }
}

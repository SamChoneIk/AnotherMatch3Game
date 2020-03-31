using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamPivot : MonoBehaviour
{
    private Camera cam;

    public void SetCameraPivot(float row, float Column, float fixed_x, float fixed_y, float fixed_size)
    {
        transform.position = new Vector3(((row * 0.5f ) - 0.5f) + fixed_x, 
                                                (Column * 0.5f) + fixed_y, -10);
        

        cam = GetComponent<Camera>();
        cam.orthographicSize = (((row * 0.5f) - 0.5f) + (Column * 0.5f)) + fixed_size;


#if UNITY_ANDROID
        cam.orthographicSize += 1.5f;
#endif
    }
}

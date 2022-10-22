using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraDepth : MonoBehaviour
{
    public DepthTextureMode Mode;
    private Camera cam;
    void OnEnable()
    {
        cam = GetComponent<Camera>();
        cam.depthTextureMode = Mode;
    }
}

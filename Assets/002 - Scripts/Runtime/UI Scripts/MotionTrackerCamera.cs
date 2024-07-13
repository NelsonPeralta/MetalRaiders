using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionTrackerCamera : MonoBehaviour
{
    public void ChooseRenderTexture(int rewiredId)
    {
        GetComponent<Camera>().targetTexture = GameManager.instance.minimapRenderTextures[rewiredId];
    }
}

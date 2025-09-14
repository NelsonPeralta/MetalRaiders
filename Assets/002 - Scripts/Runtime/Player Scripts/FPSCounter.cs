using TMPro;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    public TextMeshProUGUI txtFps; // or Text if not using TMP

    public float updateRateSeconds = 4.0f;

    int frameCount = 0;
    float dt = 0.0f;
    float fps = 0.0f;

    void Update()
    {
        frameCount++;
        dt += Time.unscaledDeltaTime;
        if (dt > 1.0f / updateRateSeconds)
        {
            fps = frameCount / dt;
            frameCount = 0;
            dt -= 1.0f / updateRateSeconds;

            // Avoid string allocations completely:
            if (txtFps != null)
            {
                int fpsInt = Mathf.RoundToInt(fps);
                txtFps.SetText("{0} FPS", fpsInt); // TMP-friendly, no GC
            }
        }
    }
}

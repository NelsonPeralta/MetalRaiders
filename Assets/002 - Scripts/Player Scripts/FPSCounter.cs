using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour
{
    public int avgFrameRate;
    public Text display_Text;

    private void Start()
    {
        StartCoroutine(UpdateFPSText());
    }
    IEnumerator UpdateFPSText()
    {
        float current = 0;
        current = (int)(1f / Time.unscaledDeltaTime);
        avgFrameRate = (int)current;
        display_Text.text = "FPS: " + avgFrameRate.ToString();
        yield return new WaitForSeconds(0.5f);

        StartCoroutine(UpdateFPSText());
    }
    //public void Update()
    //{
    //    float current = 0;
    //    current = (int)(1f / Time.unscaledDeltaTime);
    //    //current = Time.frameCount / Time.time;
    //    avgFrameRate = (int)current;
    //    display_Text.text = "FPS: " + avgFrameRate.ToString();
    //}
}

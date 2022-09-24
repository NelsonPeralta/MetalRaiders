using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBlackScreen : MonoBehaviour
{
    public Image Blackscreen;

    [Header("Blackscreen Fade-out")]
    public bool useFade;
    public float fadeTime;
    public float elapsedTime;
    private YieldInstruction fadeInstruction = new YieldInstruction();

    private void Start()
    {
        if (useFade)
            StartCoroutine(FadeOut(Blackscreen));
        else
        {
            Color c = Blackscreen.color;
            c.a = 0;
            Blackscreen.color = c;
        }
    }

    IEnumerator FadeOut(Image image)
    {
        image.gameObject.SetActive(true);
        Debug.Log("Fading Blackscreen");
        float elapsedTime = 0.0f;
        Color c = image.color;
        while (elapsedTime < fadeTime)
        {
            yield return fadeInstruction;
            elapsedTime += Time.deltaTime;
            c.a = 1.0f - Mathf.Clamp01(elapsedTime / fadeTime);
            image.color = c;
        }
    }
}

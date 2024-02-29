using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IncreaseScaleThenScaleBackInTime : MonoBehaviour
{
    //[SerializeField] float _startScale, _endScale, _time;

    // Start is called before the first frame update
    Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;
    }
    private void OnEnable()
    {
        StopAllCoroutines();
        StartCoroutine(ScaleOverTime(0.25f, 1));
    }

    private void Start()
    {
        StopAllCoroutines();
        transform.localScale = originalScale;
    }

    IEnumerator ScaleOverTime(float time, float destScale)
    {
        Vector3 _orSc = new Vector3(originalScale.x * 4, originalScale.y * 4, originalScale.z * 4);
        Vector3 destinationScale = new Vector3(destScale, destScale, destScale);
        destinationScale = originalScale;

        float currentTime = 0.0f;

        do
        {
            transform.localScale = Vector3.Lerp(_orSc, destinationScale, currentTime / time);
            currentTime += Time.deltaTime;
            yield return null;
        } while (currentTime <= time);
    }
}

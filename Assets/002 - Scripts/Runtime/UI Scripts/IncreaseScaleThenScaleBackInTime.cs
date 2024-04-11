using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IncreaseScaleThenScaleBackInTime : MonoBehaviour
{
    //[SerializeField] float _startScale, _endScale, _time;

    // Start is called before the first frame update
    Vector3 originalScale = Vector3.zero;
    Coroutine coroutine;

    private void Awake()
    {
        originalScale = transform.localScale;
        print($"IncreaseScaleThenScaleBackInTime Awake {originalScale} {name}");
    }
    //private void OnEnable()
    //{
    //    print("IncreaseScaleThenScaleBackInTime OnEnable");
    //    if (originalScale == Vector3.zero) originalScale = transform.localScale;


    //    if (coroutine != null) StopCoroutine(coroutine);
    //    coroutine = StartCoroutine(ScaleOverTime(0.25f, 1));
    //}

    //void OnDisable()
    //{
    //    print("IncreaseScaleThenScaleBackInTime OnDisable");
    //    StopAllCoroutines();
    //}

    public void Trigger()
    {
        if (coroutine != null) StopCoroutine(coroutine);
        coroutine = StartCoroutine(ScaleOverTime(0.25f, 1));
    }

    IEnumerator ScaleOverTime(float time, float destScale)
    {
        print("IncreaseScaleThenScaleBackInTime ScaleOverTime");
        Vector3 _orSc = new Vector3(originalScale.x * 4, originalScale.y * 4, originalScale.z * 4);
        Vector3 destinationScale = new Vector3(destScale, destScale, destScale);
        destinationScale = originalScale;

        float currentTime = 0.0f;

        do
        {
            print(Vector3.Lerp(_orSc, destinationScale, currentTime / time));
            print(name);
            transform.localScale = Vector3.Lerp(_orSc, destinationScale, currentTime / time);
            currentTime += Time.deltaTime;
            yield return null;
        } while (currentTime <= time);
    }
}

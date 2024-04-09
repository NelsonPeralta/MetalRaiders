using System.Collections;
using System.Collections.Generic;
using PlasticGui.WorkspaceWindow.PendingChanges;
using UnityEngine;

public class OnEnableIncreaseScaleThenScaleBack : MonoBehaviour
{

    float timeToDescale = 0.2f;
    float diff;
    int neededTicks;


    Vector3 sub = Vector3.zero;

    private void OnEnable()
    {
        transform.localScale = Vector3.one * 2;
        neededTicks = Mathf.RoundToInt(timeToDescale / (1 / 50f)); print($"needed ticks: {neededTicks}");
        sub = new Vector3(1f / neededTicks, 1f / neededTicks, 1f / neededTicks); print($"sub: {sub}");
    }

    private void FixedUpdate()
    {
        if (neededTicks > 0 && sub != Vector3.zero)
        {
            transform.localScale -= sub;
            neededTicks--;
        }
    }

    void OnDisable()
    {
        sub = Vector3.zero;
    }




    //private void OnEnable()
    //{
    //    if (originalScale == Vector3.zero) originalScale = transform.localScale;

    //    StopAllCoroutines();
    //    StartCoroutine(ScaleOverTime(0.15f, 1));
    //}

    //private void Awake()
    //{
    //    originalScale = transform.localScale;
    //}

    //void OnDisable()
    //{
    //    StopAllCoroutines();
    //}

    //IEnumerator ScaleOverTime(float time, float destScale)
    //{
    //    //Vector3 _orSc = new Vector3(originalScale.x * 4, originalScale.y * 4, originalScale.z * 4);
    //    Vector3 _orSc = Vector3.one * 2;
    //    Vector3 destinationScale = new Vector3(destScale, destScale, destScale);
    //    destinationScale = originalScale;

    //    float currentTime = 0.0f;

    //    do
    //    {
    //        transform.localScale = Vector3.Lerp(_orSc, destinationScale, currentTime / time);
    //        currentTime += Time.deltaTime;
    //        yield return null;
    //    } while (currentTime <= time);
    //}
}

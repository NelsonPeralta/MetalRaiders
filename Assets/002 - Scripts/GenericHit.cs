using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericHit : MonoBehaviour
{
    public float timeToDisable;

    private void OnEnable()
    {
        StartCoroutine(Disable());
    }

    IEnumerator Disable()
    {
        yield return new WaitForSeconds(timeToDisable);
        gameObject.SetActive(false);
    }
}

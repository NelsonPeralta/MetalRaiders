using System.Collections;
using UnityEngine;

public class HitMarker : MonoBehaviour
{
    float _t;
    private void OnEnable()
    {
        _t = 0;
        transform.localScale = Vector3.one;
        StartCoroutine(Disable_Coroutine());
    }

    private void Update()
    {
        _t += Time.deltaTime;
        transform.localScale = new Vector3(1 + (_t * 5), 1 + (_t * 5), 1 + (_t * 5));
        //transform.localScale = Vector3.Lerp(transform.localScale, transform.localScale * 4, Time.deltaTime * 1f);
    }

    IEnumerator Disable_Coroutine()
    {
        yield return new WaitForSeconds(0.2f);
        gameObject.SetActive(false);
    }
}

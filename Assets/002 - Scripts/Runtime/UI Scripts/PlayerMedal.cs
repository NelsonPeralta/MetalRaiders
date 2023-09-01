using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMedal : MonoBehaviour
{
    public AudioClip clip { get { return _clip; } }

    float oScaleCounter
    {
        get { return _oScaleCounter; }
        set
        {
            _oScaleCounter = Mathf.Clamp(value, 0, oScale.x);
            transform.localScale = new Vector3(_oScaleCounter, _oScaleCounter, _oScaleCounter);
        }
    }

    [SerializeField] AudioClip _clip;

    Vector3 oScale;
    float _oScaleCounter;

    private void Awake()
    {
        oScale = transform.localScale;
        transform.localScale = Vector3.zero;
    }

    private void Start()
    {
        Destroy(gameObject, 6);
    }

    private void Update()
    {
        //https://forum.unity.com/threads/slowly-scale-object.91659/
        //var newScale = Mathf.Lerp(1, 5, Time.time);
        //transform.localScale = new Vector3(newScale, newScale, 1);

        oScaleCounter += Time.deltaTime * 5 * oScale.x;
    }
}

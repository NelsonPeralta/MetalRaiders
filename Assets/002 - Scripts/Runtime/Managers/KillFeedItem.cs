using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class KillFeedItem : MonoBehaviour
{
    public TMP_Text tmpt;

    float oScaleCounter
    {
        get { return _scaleCounter; }
        set
        {
            _scaleCounter = Mathf.Clamp(value, 0, _oLocalScale.x);
            transform.localScale = new Vector3(_scaleCounter, _oLocalScale.y, _oLocalScale.z);
        }
    }


    float _scaleCounter;
    Vector3 _oLocalScale;
    RectTransform _rTrans;

    private void Start()
    {
        _rTrans = GetComponent<RectTransform>();
        _oLocalScale = _rTrans.localScale;


        _rTrans.pivot = new Vector2(0, GetComponent<RectTransform>().pivot.y);
        _rTrans.localScale = new Vector3(0, 1, 1);
    }
    private void Update()
    {
        oScaleCounter += Time.deltaTime * 4 * _oLocalScale.x;
    }
}

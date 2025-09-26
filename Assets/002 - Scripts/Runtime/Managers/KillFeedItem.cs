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


    float _disableCountdown;
    float _scaleCounter;
    Vector3 _oLocalScale;
    RectTransform _rTrans;

    private void Start()
    {
        _disableCountdown = 4;
        _rTrans = GetComponent<RectTransform>();
        _oLocalScale = _rTrans.localScale;


        _rTrans.pivot = new Vector2(0, GetComponent<RectTransform>().pivot.y);
        _rTrans.localScale = new Vector3(0, 1, 1);



        _scaleCounter = -1;
        gameObject.SetActive(false);
    }
    private void Update()
    {
        if (_disableCountdown > 0)
        {
            _disableCountdown -= Time.deltaTime;

            if (_disableCountdown <= 0)
            {
                _scaleCounter = -1;
                _disableCountdown = 4;
                gameObject.SetActive(false);
            }
        }

        if (_scaleCounter > -1)
            oScaleCounter += Time.deltaTime * 3 * _oLocalScale.x;
    }

    public void TriggerBehaviour()
    {
        Log.Print(() => "Killfeed TriggerBehaviour");
        transform.localScale = _oLocalScale;
        _disableCountdown = PlayerMedals.MEDAL_TTL;
        _scaleCounter = 0;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAudioListener : MonoBehaviour
{
    [SerializeField] GameObject multiAudioListenerGameObject;
    float _enableDelay;

    // Start is called before the first frame update
    void Start()
    {
        _enableDelay = GameManager.GameStartDelay * 0.99f;
    }

    // Update is called once per frame
    void Update()
    {
        if(_enableDelay > 0)
        {
            _enableDelay -= Time.deltaTime;

            if (_enableDelay <= 0)
                multiAudioListenerGameObject.SetActive(true);
        }
    }
}

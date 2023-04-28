using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class PlayerPing : MonoBehaviour
{
    [SerializeField] Text pingText;

    float _delay;

    private void Start()
    {
        _delay = 1;
    }

    void Update()
    {
        if (_delay > 0)
            _delay -= Time.deltaTime;

        if (_delay <= 0)
        {
            pingText.text = $"Ping: {PhotonNetwork.GetPing()}";
            _delay = 1;
        }
    }
}

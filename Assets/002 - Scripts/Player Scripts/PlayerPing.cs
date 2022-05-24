using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class PlayerPing : MonoBehaviour
{
    [SerializeField] Text pingText;

    // Update is called once per frame
    void Update()
    {
        pingText.text = $"Ping: {PhotonNetwork.GetPing()}";
    }
}

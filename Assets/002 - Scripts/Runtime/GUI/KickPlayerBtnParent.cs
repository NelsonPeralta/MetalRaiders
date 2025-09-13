using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KickPlayerBtnParent : MonoBehaviour
{
    private void Awake()
    {
        Log.Print("KickPlayerBtn PARENT Awake");
        if (GameManager.instance.connection == GameManager.NetworkType.Local ||
           (GameManager.instance.connection == GameManager.NetworkType.Internet && !PhotonNetwork.IsMasterClient))
        {
            transform.GetChild(0).gameObject.SetActive(false);
        }
    }
}

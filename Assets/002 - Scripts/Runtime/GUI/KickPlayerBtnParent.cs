using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class KickPlayerBtnParent : MonoBehaviour
{
    private void Awake()
    {
        if(GameManager.instance.connection == GameManager.Connection.Local || 
           (GameManager.instance.connection == GameManager.Connection.Online && !PhotonNetwork.IsMasterClient))
        {
            transform.GetChild(0).gameObject.SetActive(false);  
        }
    }
}

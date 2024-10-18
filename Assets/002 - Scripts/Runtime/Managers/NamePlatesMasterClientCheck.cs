using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class NamePlatesMasterClientCheck : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        foreach (Transform child in transform)
        {

        }

        if (PhotonNetwork.InRoom)
            foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
            {
                if (player.IsMasterClient)
                {// there ya go}}
                }
            }
    }
}

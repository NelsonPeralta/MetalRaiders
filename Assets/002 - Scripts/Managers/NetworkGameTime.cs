using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class NetworkGameTime : MonoBehaviour
{
    OnlineGameTime gameTime;
    [PunRPC]
    public void AddSecond_RPC()
    {
        FindObjectOfType<OnlineGameTime>().totalTime++;
    }
}

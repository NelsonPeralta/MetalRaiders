using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class NetworkGameTime : MonoBehaviour
{
    GameTime gameTime;
    [PunRPC]
    public void AddSecond_RPC(int newTotalTime)
    {
        FindObjectOfType<GameTime>().totalTime = newTotalTime;
    }

    [PunRPC]
    public void UpdateTime_RPC(int newTotalTime)
    {
        FindObjectOfType<GameTime>().totalTime = newTotalTime;
    }
}

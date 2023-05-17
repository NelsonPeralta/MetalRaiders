using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class NetworkGameTime : MonoBehaviour
{
    public static NetworkGameTime instance { get { return _instance; } }

    GameTime gameTime;
    static NetworkGameTime _instance;


    void Awake()
    {
        _instance = this;
    }

    private void OnDestroy()
    {
        _instance = null;
    }

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

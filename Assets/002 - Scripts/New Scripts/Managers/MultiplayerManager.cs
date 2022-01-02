using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MultiplayerManager : MonoBehaviourPunCallbacks
{
    // public variables
    public static MultiplayerManager instance;

    [Header("Score")]
    public int scoreToWin;

    // private variables
    PhotonView PV;

    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        PV = GetComponent<PhotonView>();

        GameManager.instance.OnSceneLoadedEvent += OnSceneLoaded;
    }

    void OnSceneLoaded()
    {
        if (GameManager.instance.gameMode != GameManager.GameMode.Multiplayer)
        {
            enabled = false;
            return;
        }

        if (GameManager.instance.multiplayerMode == GameManager.MultiplayerMode.Deathmatch)
            scoreToWin = 15;
    }
}

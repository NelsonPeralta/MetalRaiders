using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using ExitGames.Client.Photon;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using System;

public class GameTime : MonoBehaviourPunCallbacks
{
    public static GameTime instance { get { return _instance; } }
    public delegate void GameTimeEvent(GameTime gameTime);
    public GameTimeEvent OnGameTimeChanged;

    public int totalTime
    {
        get { return _totalTime; }
        set
        {
            if (_totalTime != value)
            {
                _totalTime = value;
                OnGameTimeChanged?.Invoke(this);
            }
        }
    }

    [SerializeField] int _totalTime = 0;
    [SerializeField] int __totalTime = 0;
    [SerializeField] int minPlayers = 2;
    [SerializeField] int timeOutMultiples = 15;

    float secondCountdown = 1f;
    bool waitingTimedOut;

    static GameTime _instance;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void OnEnable()
    {
        base.OnEnable();// need this for OnRoomPropertiesUpdate to work if MonoBehaviourPunCallbacks

        secondCountdown = 1;
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        OnGameTimeChanged = null;
        totalTime = 0;
        __totalTime = 0;
        secondCountdown = 1;
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Update()
    {
        if (GameManager.sceneIndex <= 0)
            return;
        secondCountdown -= Time.deltaTime;

        if (secondCountdown < 0)
        {
            __totalTime++;

            if (PhotonNetwork.IsMasterClient)
            {
                try { NetworkGameTime.instance.GetComponent<PhotonView>().RPC("UpdateTime_RPC", RpcTarget.All, __totalTime); } catch { }
            }

            secondCountdown = 1;
        }
    }
}
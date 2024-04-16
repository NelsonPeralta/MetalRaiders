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
    public GameTimeEvent OnGameTimeRemainingChanged, OnGameTimeElapsedChanged;

    public int timeRemaining
    {
        get { return _timeRemaining; }
        set
        {
            if (_timeRemaining != value && value >= 0)
            {
                _timeRemaining = value;
                OnGameTimeRemainingChanged?.Invoke(this);

                if (value == 0 && PhotonNetwork.IsMasterClient) NetworkGameManager.instance.EndGame();
            }
        }
    }

    public int timeElapsed
    {
        get { return _timeElapsed; }
        set
        {
            if (_timeElapsed != value)
            {
                _timeElapsed = value;
                OnGameTimeElapsedChanged?.Invoke(this);
            }
        }
    }

    [SerializeField] int _timeRemaining = 0, _timeElapsed, _masterTimeRemaining = 0, _masterTimeElapsed;
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
        OnGameTimeRemainingChanged = null;
        timeRemaining = 300;
        _masterTimeRemaining = 300;

        //timeRemaining = 10; _masterTimeRemaining = 10;

        _timeElapsed = 0;
        _masterTimeElapsed = 0;
        secondCountdown = 1;
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Update()
    {
        if (GameManager.sceneIndex <= 0 || !CurrentRoomManager.instance.gameStarted) return;
        if (_masterTimeRemaining <= 0) return;

        secondCountdown -= Time.deltaTime;

        if (secondCountdown < 0)
        {
            _masterTimeRemaining--; _masterTimeElapsed++;

            if (PhotonNetwork.IsMasterClient)
            {
                try { NetworkGameTime.instance.GetComponent<PhotonView>().RPC("UpdateTime_RPC", RpcTarget.AllViaServer, _masterTimeRemaining, _masterTimeElapsed); } catch { }
            }

            secondCountdown = 1;
        }
    }
}
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


                if (GameManager.instance.gameMode == GameManager.GameMode.Coop && GameManager.instance.gameType != GameManager.GameType.Endless)
                {
                    SwarmManager.instance.gameWon = true;
                }

                if (value == 0 && PhotonNetwork.IsMasterClient && GameManager.instance.gameType != GameManager.GameType.Endless)
                {
                    NetworkGameManager.instance.EndGame();
                }
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

    public int roundTimeRemaining
    {
        get
        {
            return _roundTimeRemaining;
        }
        set
        {
            _roundTimeRemaining = value;
            OnGameTimeElapsedChanged?.Invoke(this);

            if (value == 0) GameManager.instance.OneObjModeRoundOver = true;
        }
    }

    [SerializeField] int _timeRemaining = 0, _timeElapsed, _masterTimeRemaining = 0, _masterTimeElapsed, _masterRoundTimeRemaining, _roundTimeRemaining;
    [SerializeField] int minPlayers = 2;
    [SerializeField] int timeOutMultiples = 15;
    [SerializeField] bool _unlimitedTime;

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
        timeRemaining = 600;
        _masterTimeRemaining = 600;
        _masterRoundTimeRemaining = 0; _roundTimeRemaining = 0;

        if (GameManager.instance.gameMode == GameManager.GameMode.Coop)
        {
            timeRemaining = 1800;
            _masterTimeRemaining = 1800;
        }

        if (GameManager.instance.gameType == GameManager.GameType.CTF && GameManager.instance.oneObjMode == GameManager.OneObjMode.On)
        {
            _unlimitedTime = true;
            _masterRoundTimeRemaining = GameManager.ROUND_DEFAULT_TIME;
        }
        else _unlimitedTime = false;


        //// tests
        //timeRemaining = 10;
        //_masterTimeRemaining = 10;
        //// test




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
            if (!_unlimitedTime) _masterTimeRemaining--;
            _masterTimeElapsed++;

            if (PhotonNetwork.IsMasterClient)
            {
                if (GameManager.instance.oneObjMode == GameManager.OneObjMode.On && _masterRoundTimeRemaining > 0 && !GameManager.instance.OneObjModeRoundOver)
                {
                    if (_masterRoundTimeRemaining > 0) _masterRoundTimeRemaining--;
                    try { NetworkGameTime.instance.GetComponent<PhotonView>().RPC("UpdateTime_RPC", RpcTarget.AllViaServer, _masterRoundTimeRemaining, _masterTimeElapsed); } catch { }
                }
                else if (GameManager.instance.oneObjMode == GameManager.OneObjMode.Off)
                    try { NetworkGameTime.instance.GetComponent<PhotonView>().RPC("UpdateTime_RPC", RpcTarget.AllViaServer, _masterTimeRemaining, _masterTimeElapsed); } catch { }
            }

            secondCountdown = 1;
        }
    }

    public void ResetOneObjRoundTime()
    {
        _masterRoundTimeRemaining = _roundTimeRemaining = GameManager.ROUND_DEFAULT_TIME;
    }
}
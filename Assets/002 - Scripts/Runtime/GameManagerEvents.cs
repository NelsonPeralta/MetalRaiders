using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManagerEvents : MonoBehaviour
{
    // Events
    public delegate void GameManagerEvent(GameManagerEvents gme);
    public GameManagerEvent OnAllPlayersJoinedRoom, OnGameStarted;

    public bool allPlayersJoined
    {
        get { return _allPlayersJoined; }
        set
        {
            _allPlayersJoined = value;
            if (value)
            {
                Debug.Log("allPlayersJoined");
                OnAllPlayersJoinedRoom?.Invoke(this);
            }
        }
    }

    public bool gameStarted
    {
        get { return _gameStarted; }
        private set
        {
            _gameStarted = value;
            if (value)
                OnGameStarted?.Invoke(this);
            else
            {
                _gameStartedTime = GameManager.GameStartDelay;
                allPlayersJoined = false;
            }
        }
    }





    [SerializeField] bool _gameStarted;
    [SerializeField] bool _allPlayersJoined;
    [SerializeField] float _gameStartedTime;






    private void Start()
    {
        OnAllPlayersJoinedRoom += OnAllPlayersJoinedGame;
    }

    private void Update()
    {
        if (allPlayersJoined && !gameStarted)
        {
            _gameStartedTime -= Time.deltaTime;

            if (_gameStartedTime <= 0)
                gameStarted = true;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        Debug.Log("GAME MANAGER EVENTS: OnSceneLoaded");

        if (scene.buildIndex == 0)
        {
            OnAllPlayersJoinedRoom = null;


            OnAllPlayersJoinedRoom -= FindObjectOfType<SwarmManager>().OnAllPlayersJoinedRoom_Delegate;
            OnAllPlayersJoinedRoom += FindObjectOfType<SwarmManager>().OnAllPlayersJoinedRoom_Delegate;
        }

        gameStarted = false;
    }

    void OnAllPlayersJoinedGame(GameManagerEvents gme)
    {
        _gameStartedTime = GameManager.GameStartDelay;
    }

}

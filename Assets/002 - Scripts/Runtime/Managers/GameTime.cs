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
    public static GameTime instance;


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

    private void Awake()
    {
        Debug.Log("OnlineGameTime Awake");
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        instance = this;
    }

    private void OnEnable()
    {
        base.OnEnable();// need this for OnRoomPropertiesUpdate to work if MonoBehaviourPunCallbacks
    }

    private void OnDestroy()
    {
        instance = null;
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        totalTime = 0;
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
                FindObjectOfType<NetworkGameTime>().GetComponent<PhotonView>().RPC("UpdateTime_RPC", RpcTarget.All, __totalTime);

            //FindObjectOfType<NetworkGameTime>().GetComponent<PhotonView>().RPC("AddSecond_RPC", RpcTarget.All, totalTime);
            secondCountdown = 1;

            return;
            // TODO
            // Waiting room Timeout
            #region
            if (totalTime % timeOutMultiples == 0 && GameManager.sceneIndex == Launcher.instance.waitingRoomLevelIndex && PhotonNetwork.CurrentRoom.PlayerCount >= minPlayers)
            {
                // Choosing random GameType
                #region
                Array values = Enum.GetValues(typeof(GameManager.ArenaGameType));
                System.Random random = new System.Random();
                GameManager.ArenaGameType arenaGameType = (GameManager.ArenaGameType)values.GetValue(random.Next(values.Length));

                for (int i = 0; i < 3; i++)
                    if (arenaGameType != GameManager.ArenaGameType.Slayer)
                        arenaGameType = (GameManager.ArenaGameType)values.GetValue(random.Next(values.Length));

                ExitGames.Client.Photon.Hashtable ht = new ExitGames.Client.Photon.Hashtable();
                ht.Add("gamemode", GameManager.GameMode.Multiplayer.ToString());
                ht.Add("gametype", arenaGameType.ToString());

                PhotonNetwork.CurrentRoom.SetCustomProperties(ht);
                #endregion

                waitingTimedOut = true;
            }
            #endregion
        }
    }








    // OLD
    #region

    //public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    //{
    //    if (waitingTimedOut)
    //    {
    //        waitingTimedOut = false;
    //        Debug.Log("OnRoomPropertiesUpdate");

    //        System.Random random = new System.Random();
    //        int index = random.Next(GameManager.instance.arenaLevelIndexes.Count);
    //        index = GameManager.instance.arenaLevelIndexes[index];
    //        PhotonNetwork.LoadLevel(index);
    //    }
    //}

    #endregion
}
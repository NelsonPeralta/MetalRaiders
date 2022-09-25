using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using ExitGames.Client.Photon;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using System;

public class OnlineGameTime : MonoBehaviourPunCallbacks
{
    public static OnlineGameTime onlineGameTimeInstance;

    public List<Text> playerTimerTexts;
    public int totalTime
    {
        get { return _totalTime; }
        set
        {
            if (_totalTime != value)
            {
                _totalTime = value;
                UpdateTimerTexts();
            }
        }
    }

    [SerializeField] int _totalTime = 0;
    float secondCountdown = 1f;

    private void Awake()
    {
        Debug.Log("OnlineGameTime Awake");
        if (onlineGameTimeInstance)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        onlineGameTimeInstance = this;
    }

    private void OnEnable()
    {
        Debug.Log("OnEnable Awake");

    }
    private void Start()
    {

    }

    private void Update()
    {
        if (GameManager.sceneIndex <= 0)
            return;
        secondCountdown -= Time.deltaTime;

        if (secondCountdown < 0)
        {
            totalTime++;
            if (PhotonNetwork.IsMasterClient)
                FindObjectOfType<NetworkGameTime>().GetComponent<PhotonView>().RPC("AddSecond_RPC", RpcTarget.All, totalTime);
            secondCountdown = 1;

            // Waiting room Timeout
            #region
            if (totalTime >= 4 && GameManager.sceneIndex == Launcher.instance.waitingRoomLevelIndex /*&& PhotonNetwork.CurrentRoom.PlayerCount > 1*/)
            {
                // Choosing random GameType
                #region
                Array values = Enum.GetValues(typeof(GameManager.ArenaGameType));
                System.Random random = new System.Random();
                GameManager.ArenaGameType arenaGameType = (GameManager.ArenaGameType)values.GetValue(random.Next(values.Length));

                for (int i = 0; i < 1; i++)
                    if (arenaGameType != GameManager.ArenaGameType.Slayer)
                        arenaGameType = (GameManager.ArenaGameType)values.GetValue(random.Next(values.Length));

                PhotonNetwork.CurrentRoom.CustomProperties["gametype"] = arenaGameType.ToString();
                #endregion

                // Choosing random Level
                #region
                int index = random.Next(GameManager.instance.arenaLevelIndexes.Count);
                index = GameManager.instance.arenaLevelIndexes[index];
                #endregion

                Debug.Log($"Waiting room timeout.\nNew Scene: {GameManager.SceneNameFromIndex(index)} ({index}). New GameType: {arenaGameType}");
                PhotonNetwork.LoadLevel(index);
            }
            #endregion
        }
    }

    void UpdateTimerTexts()
    {
        var playerUIs = FindObjectsOfType<PlayerUI>(true);

        foreach (PlayerUI ui in playerUIs)
            ui.Timer.text = $"{(totalTime / 60).ToString("00")}:{(totalTime % 60).ToString("00")}";
    }

    private void OnDestroy()
    {
        onlineGameTimeInstance = null;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        totalTime = 0;
    }
}
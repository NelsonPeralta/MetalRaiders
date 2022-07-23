using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using ExitGames.Client.Photon;
using Photon.Realtime;
using UnityEngine.SceneManagement;

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

    int _totalTime = 0;
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
        if (GameManager.instance.sceneIndex <= 0)
            return;
        secondCountdown -= Time.deltaTime;

        if (secondCountdown < 0)
        {
            if (PhotonNetwork.IsMasterClient)
                FindObjectOfType<NetworkGameTime>().GetComponent<PhotonView>().RPC("AddSecond_RPC", RpcTarget.All);
            secondCountdown = 1;
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
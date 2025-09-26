using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;

public class MapCamera : MonoBehaviourPunCallbacks
{
    public static MapCamera instance { get { return _instance; } private set { _instance = value; } }
    static MapCamera _instance;

    [SerializeField] GameObject _canvas;
    [SerializeField] Image _mapPreview;
    [SerializeField] AudioClip _slayerClip;
    [SerializeField] AudioClip _KingOfTheHillClip;
    [SerializeField] AudioClip _FirefightClip, _oddballIntro, _ctfIntro;
    [SerializeField] TMP_Text _loadingText, _gametypePreviewInfo, _mapNameInfo;
    [SerializeField] AudioSource _beepConsecutiveAudioSource;
    [SerializeField] GameObject _blackscreenCamera;

    public Transform disabledJunk;


    float _announcerDelay;
    bool _allPlayersJoined;
    bool _announcementPLayed;


    float _loadingTimeOut;


    private void OnDestroy()
    {
        _instance = null;
    }
    private void Awake()
    {
        AudioListener.volume = PlayerPrefs.GetFloat("volume");
        _loadingTimeOut = 20;
        _instance = this;
    }
    private void Start()
    {
        Log.Print(() =>"MapCamera");
        AudioListener.volume = 0f;


        try
        {
            _mapPreview.sprite = GameManager.instance.mapDataCells.Where(obj => obj.sceneBuildIndex == SceneManager.GetActiveScene().buildIndex).SingleOrDefault().image;
            _mapPreview.color = Color.white;
        }
        catch { }


        _gametypePreviewInfo.text = GameManager.instance.gameType.ToString();

        if (GameManager.instance.teamMode == GameManager.TeamMode.Classic && GameManager.instance.gameMode == GameManager.GameMode.Versus)
        {
            _gametypePreviewInfo.text = $"Team {_gametypePreviewInfo.text}";
        }
        _gametypePreviewInfo.text += " on";


        _mapNameInfo.text = GameManager.instance.mapDataCells.Where(obj => obj.sceneBuildIndex == SceneManager.GetActiveScene().buildIndex).SingleOrDefault().name;

    }

    private void Update()
    {
        //_text.text = $"expectedMapAddOns {CurrentRoomManager.instance.expectedMapAddOns} - spawnedMapAddOns {CurrentRoomManager.instance.spawnedMapAddOns}";
        //_text.text += $"\nmapIsReady {CurrentRoomManager.instance.mapIsReady} - playersLoadedScene {CurrentRoomManager.instance.playersLoadedScene}";
        //_text.text += $"\nnbPlayersJoined {CurrentRoomManager.instance.nbPlayersJoined} - expectedNbPlayers {CurrentRoomManager.instance.expectedNbPlayers}";
        //_text.text += $"\nallPlayersJoined {CurrentRoomManager.instance.allPlayersJoined}";
        //_text.text += $"\ngameIsReady {CurrentRoomManager.instance.gameIsReady}";

        if (!GameManager.instance) return;


        _loadingText.text = $"Mapp Add-Ons: {CurrentRoomManager.instance.spawnedMapAddOns}/{CurrentRoomManager.instance.expectedMapAddOns}";
        _loadingText.text += $"\nMap Loaded: {CurrentRoomManager.instance.mapIsReady}";
        _loadingText.text += $"\nPlayers Loaded Map: {CurrentRoomManager.instance.playersLoadedScene}/{CurrentRoomManager.instance.expectedNbPlayers}";
        _loadingText.text += $"\nPlayers Spawned: {CurrentRoomManager.instance.nbPlayersSpawned}/{CurrentRoomManager.instance.expectedNbPlayers}";
        _loadingText.text += $"\nPlayers Set: {CurrentRoomManager.instance.nbPlayersSet}/{CurrentRoomManager.instance.expectedNbPlayers}";
        _loadingText.text += $"\nAll Players Joined: {CurrentRoomManager.instance.allPlayersJoined}";
        _loadingText.text += $"\nReady: {CurrentRoomManager.instance.gameIsReady} ({CurrentRoomManager.instance.GameReadyStep})";




        if (_announcerDelay > 0)
        {
            _announcerDelay -= Time.deltaTime;

            if (_announcerDelay <= 0)
            {
                PlayAnnouncer();
            }
        }


        if (_loadingTimeOut > 0)
        {
            _loadingTimeOut -= Time.deltaTime;
            if (_loadingTimeOut <= 0)
            {
                try
                {
                    Log.Print(() =>"MapCamera TimeOut");

                    //GameManager.SendErrorEmailReport(_text.text);
                    GameManager.instance.previousScenePayloads.Add(GameManager.PreviousScenePayload.LoadTimeOutOpenErrorMenu);

                    PhotonNetwork.LeaveRoom(); // Will trigger on OnLeftRoom in the GameManager script!!!! Not Here!!!
                    //PhotonNetwork.LoadLevel(0);
                }
                catch { }
            }
        }
    }
    public void TriggerGameStartBehaviour()
    {
        Log.Print(() => $"oneobjmode - TriggerGameStartBehaviour");

        _canvas.gameObject.SetActive(false);
        _announcementPLayed = true;
        GameManager.UpdateVolume();
        _announcerDelay = 1;

        try { if (GameManager.instance.gameMode == GameManager.GameMode.Coop) SwarmManager.instance.PlayOpeningMusic(); } catch { }

    }

    void PlayAnnouncer()
    {
        try
        {
            GetComponent<AudioSource>().clip = _slayerClip;
            if (GameManager.instance.gameType == GameManager.GameType.Hill)
                GetComponent<AudioSource>().clip = _KingOfTheHillClip;
            if (GameManager.instance.gameMode == GameManager.GameMode.Coop)
                GetComponent<AudioSource>().clip = _FirefightClip;

            if (GameManager.instance.gameType == GameManager.GameType.Oddball)
                GetComponent<AudioSource>().clip = _oddballIntro;

            if (GameManager.instance.gameType == GameManager.GameType.CTF)
                GetComponent<AudioSource>().clip = _ctfIntro;

            GetComponent<AudioSource>().Play();
        }
        catch { }
    }




    public void EnableBlackscreen()
    {
        _blackscreenCamera.transform.parent = null;
        _blackscreenCamera.gameObject.SetActive(true);
    }

    public override void OnLeftRoom() // Is also called when quitting a game while connected to the internet. Does not trigger when offline
    {
        //Log.Print(() =>"MAP CAMERA: OnLeftRoom");
        //PhotonNetwork.LoadLevel(0); // This is handled in the GameManager Script. If you enable this, you will be calling it TWICE!
    }

    public void ResetLoadingTimeOut()
    {
        if (_loadingTimeOut < 10)
            _loadingTimeOut = 10;
    }
}

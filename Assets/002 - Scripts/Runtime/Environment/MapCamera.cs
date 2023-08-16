using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCamera : MonoBehaviour
{
    public static MapCamera instance { get { return _instance; } private set { _instance = value; } }
    static MapCamera _instance;

    [SerializeField] GameObject _canvas;
    [SerializeField] AudioClip _slayerClip;
    [SerializeField] AudioClip _KingOfTheHillClip;
    [SerializeField] AudioClip _FirefightClip;

    float _announcerDelay;
    bool _allPlayersJoined;
    bool _announcementPLayed;

    private void OnDestroy()
    {
        _instance = null;
    }
    private void Awake()
    {
        _instance = this;
    }
    private void Start()
    {
        Debug.Log("MapCamera");
        Debug.Log(AudioListener.volume);
        AudioListener.volume = 0f;
        Debug.Log(AudioListener.volume);
    }

    private void Update()
    {
        if (_announcerDelay > 0)
        {
            _announcerDelay -= Time.deltaTime;

            if (_announcerDelay <= 0)
            {
                PlayAnnouncer();
            }
        }
    }
    public void TriggerGameStartBehaviour()
    {
        _canvas.gameObject.SetActive(false);
        _announcementPLayed = true;
        AudioListener.volume = 1f;
        _announcerDelay = 2;
    }

    void PlayAnnouncer()
    {
        try
        {
            GetComponent<AudioSource>().clip = _slayerClip;
            if (GameManager.instance.gameType == GameManager.GameType.Hill)
                GetComponent<AudioSource>().clip = _KingOfTheHillClip;
            if (GameManager.instance.gameMode == GameManager.GameMode.Swarm)
                GetComponent<AudioSource>().clip = _FirefightClip;

            GetComponent<AudioSource>().Play();
        }
        catch { }
    }
}

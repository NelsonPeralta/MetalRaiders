using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCamera : MonoBehaviour
{
    [SerializeField] AudioClip _slayerClip;
    [SerializeField] AudioClip _KingOfTheHillClip;
    [SerializeField] AudioClip _FirefightClip;

    float _gameStartDelay;
    bool _allPlayersJoined;
    bool _announcementPLayed;

    private void Start()
    {
        Debug.Log("MapCamera");
        Debug.Log(AudioListener.volume);
        AudioListener.volume = 0f;
        Debug.Log(AudioListener.volume);

        FindObjectOfType<GameManagerEvents>().OnAllPlayersJoinedRoom -= OnAllPlayersJoinedRoom_Delegate;
        FindObjectOfType<GameManagerEvents>().OnAllPlayersJoinedRoom += OnAllPlayersJoinedRoom_Delegate;
    }

    private void Update()
    {
        DisableObject();
    }

    void OnAllPlayersJoinedRoom_Delegate(GameManagerEvents gme)
    {
        try
        {
            _gameStartDelay = GameManager.GameStartDelay;
            _allPlayersJoined = true;
        }
        catch (System.Exception e) { Debug.Log(e); }
    }

    void DisableObject()
    {
        if (_allPlayersJoined)
        {
            if (_gameStartDelay > 0)
                _gameStartDelay -= Time.deltaTime;

            if ((_gameStartDelay <= GameManager.GameStartDelay * 0.5f) && !_announcementPLayed)
            {
                _announcementPLayed = true;
                AudioListener.volume = 1f;

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

            if (_gameStartDelay <= 0)
            {
                gameObject.SetActive(false);
            }
        }
    }
}

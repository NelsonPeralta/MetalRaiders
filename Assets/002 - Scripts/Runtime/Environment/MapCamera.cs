using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCamera : MonoBehaviour
{
    [SerializeField] AudioClip _slayerClip;

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
        Debug.Log("OnAllPlayersJoinedRoom_Delegate");
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
                Debug.Log("OnAllPlayersJoinedRoom_Coroutine");
                Debug.Log(AudioListener.volume);
                AudioListener.volume = 1f;
                Debug.Log(AudioListener.volume);
                GetComponent<AudioSource>().clip = _slayerClip;
                GetComponent<AudioSource>().Play();
            }

            if (_gameStartDelay <= 0)
            {
                gameObject.SetActive(false);
            }
        }
    }
}

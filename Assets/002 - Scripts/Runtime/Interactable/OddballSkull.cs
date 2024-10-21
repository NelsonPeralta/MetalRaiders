using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OddballSkull : MonoBehaviour
{
    public Transform thisRoot;
    public OddballSpawnPoint spawnPoint;
    public Rigidbody rb;
    public AudioClip _taken, _dropped, _ballReset;


    Player _player;
    float _triggerReset;


    private void Awake()
    {
        GameManager.instance.oddballSkull = this;
    }


    private void OnTriggerStay(Collider other)
    {
        if (CurrentRoomManager.instance.gameStarted && !CurrentRoomManager.instance.gameOver)
        {
            print($"OODBALL {other.name}");
            if (_triggerReset <= 0 && thisRoot.gameObject.activeSelf &&
                other.transform.root.GetComponent<Player>())
            {
                _player = other.transform.root.GetComponent<Player>();
                if (!_player.isDead && !_player.isRespawning)
                {
                    print("OODBALL Player");
                    NetworkGameManager.instance.EquipOddballToPlayer_RPC(_player.photonId);
                }
            }
        }
    }

    private void Update()
    {
        if (_triggerReset > 0)
            _triggerReset -= Time.deltaTime;
    }

    private void OnEnable()
    {

    }


    public void DisableOddball()
    {
        _triggerReset = 1;
        thisRoot.gameObject.SetActive(false);
    }

    public void PlayBallTakenClip()
    {
        print("PlayBallTakenClip");
        GameManager.GetRootPlayer().announcer.AddClip(_taken);
    }

    public void PlayBallDroppedClip()
    {
        print("PlayBallDroppedClip");
        GameManager.GetRootPlayer().announcer.AddClip(_dropped);
    }

    public void PlayBallResetClip()
    {
        print("PlayBallResetClip");
        GameManager.GetRootPlayer().announcer.AddClip(_ballReset);
    }
}

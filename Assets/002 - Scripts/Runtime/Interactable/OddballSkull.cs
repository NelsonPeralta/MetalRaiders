using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class OddballSkull : MonoBehaviour
{
    public Transform thisRoot;
    public OddballSpawnPoint spawnPoint;
    public Rigidbody rb;
    public AudioClip _taken, _dropped, _ballReset;


    Player _player;
    float _triggerReset, _onTriggerStayCheck;


    RaycastHit _playerHit, _obsHit;



    private void Awake()
    {
        GameManager.instance.oddballSkull = this;

        _onTriggerStayCheck = 0.1f;
    }


    private void OnTriggerStay(Collider other)
    {
        if (CurrentRoomManager.instance.gameStarted && !CurrentRoomManager.instance.gameOver && PhotonNetwork.IsMasterClient)
        {
            if (_onTriggerStayCheck < 0)
            {
                Log.Print($"OODBALL OnTriggerStay {other.name}");
                if (_triggerReset <= 0 && thisRoot.gameObject.activeSelf &&
                    other.transform.root.GetComponent<Player>())
                {
                    _player = other.transform.root.GetComponent<Player>();
                    if (!_player.isDead && !_player.isRespawning)
                    {
                        Log.Print("OODBALL Player");

                        if (Physics.Raycast(transform.position, (_player.playerCapsule.transform.position - transform.position)
                                , out _playerHit, 5, GameManager.instance.playerCapsuleLayerMask))
                        {
                            //PrintOnlyInEditor.Log($"OODBALL Player Hit {Vector3.Distance(_playerHit.point, transform.position)}");



                            if (Physics.Raycast(transform.position, (_player.playerCapsule.transform.position - transform.position)
                                , out _obsHit, 5, GameManager.instance.obstructionMask))
                            {
                                Log.Print($"OODBALL Obstruction Hit {Vector3.Distance(_obsHit.point, transform.position)}");

                                if (Vector3.Distance(_playerHit.point, transform.position) < Vector3.Distance(_obsHit.point, transform.position))
                                {
                                    Log.Print($"EquipOddballToPlayer obstruction clear");
                                    NetworkGameManager.instance.EquipOddballToPlayer_RPC(_player.photonId);
                                }
                                else
                                {
                                    Log.Print($"Oddball obstruction {_obsHit.collider.name}");
                                }
                            }
                            else
                            {
                                Log.Print($"EquipOddballToPlayer no obstruction");
                                NetworkGameManager.instance.EquipOddballToPlayer_RPC(_player.photonId);
                            }
                        }
                    }
                }
                //_onTriggerStayCheck = 0.1f; DO NOT DO THIS HERE. IT WILL STOP US FROM CHECKING ALL COLLIDERS IN TRIGGER
            }
        }
    }

    private void Update()
    {
        if (_triggerReset > 0)
            _triggerReset -= Time.deltaTime;

        if (_onTriggerStayCheck > 0)
            _onTriggerStayCheck -= Time.deltaTime;
        else if (_onTriggerStayCheck <= 0)
            _onTriggerStayCheck = 0.2f; // we do it here because reseting in the triggerstay will prematurely break checking the entire list of colliders
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
        Log.Print("PlayBallTakenClip");
        GameManager.GetRootPlayer().announcer.AddClip(_taken);
    }

    public void PlayBallDroppedClip()
    {
        Log.Print("PlayBallDroppedClip");
        GameManager.GetRootPlayer().announcer.AddClip(_dropped);
    }

    public void PlayBallResetClip()
    {
        Log.Print("PlayBallResetClip");
        GameManager.GetRootPlayer().announcer.AddClip(_ballReset);
    }
}

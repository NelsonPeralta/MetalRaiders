using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OddballSkull : MonoBehaviour
{
    public Transform thisRoot;
    public OddballSpawnPoint spawnPoint;
    public Rigidbody rb;


    Player _player;
    [SerializeField] float _del, _reset;

    private void Awake()
    {
        GameManager.instance.oddballSkull = this;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (_del > 0) return;


        print($"OODBALL {other.name}");
        if (other.transform.root.GetComponent<Player>())
        {


            _player = other.transform.root.GetComponent<Player>();
            if (!_player.isDead && !_player.isRespawning)
            {
                print("OODBALL Player");
                NetworkGameManager.instance.EquipOddballToPlayer_RPC(_player.playerId);
            }
        }
    }

    private void Update()
    {
        if (_del > 0)
            _del -= Time.deltaTime;



        if (Vector3.Distance(spawnPoint.transform.position, rb.transform.position) > 3)
        {
            _reset -= Time.deltaTime;

            if (_reset <= 0)
            {
                spawnPoint.SpawnOddball();
                _reset = 10;
            }
        }
        else
        {
            _reset = 10;
        }
    }

    private void OnEnable()
    {
        _reset = 10;
    }


    public void DisableOddball()
    {
        _del = 0.5f;
        thisRoot.gameObject.SetActive(false);
    }
}

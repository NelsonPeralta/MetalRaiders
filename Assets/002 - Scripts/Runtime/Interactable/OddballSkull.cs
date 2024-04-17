using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OddballSkull : MonoBehaviour
{
    public Rigidbody rb;


    Player _player;
    float _del;

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
    }


    public void DisableOddball()
    {
        _del = 0.5f;
        transform.root.gameObject.SetActive(false);
    }
}

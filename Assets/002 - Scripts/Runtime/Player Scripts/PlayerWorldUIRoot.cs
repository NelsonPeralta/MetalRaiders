using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWorldUIRoot : MonoBehaviour
{
    [SerializeField] Player _player;


    private void Start()
    {
        _player.OnPlayerDeath += OnPlayerDied;
        _player.OnPlayerRespawnEarly += OnPlayerRespawned;
    }


    void OnPlayerDied(Player p)
    {
        transform.parent = null;
    }


    void OnPlayerRespawned(Player p)
    {
        transform.parent = _player.transform;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }
}

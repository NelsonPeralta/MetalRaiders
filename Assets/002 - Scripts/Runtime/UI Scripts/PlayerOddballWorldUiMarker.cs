using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerOddballWorldUiMarker : MonoBehaviour
{
    [SerializeField] PlayerWorldUIMarker _playerWorldUIMarker;
    [SerializeField] Player player;
    [SerializeField] GameObject _skullTag, _shieldTag;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (CurrentRoomManager.instance.gameOver) return;

        if (GameManager.instance.teamMode == GameManager.TeamMode.None)
            _skullTag.SetActive(player.playerInventory.playerOddballActive);
        else
        {
            if (_playerWorldUIMarker && _playerWorldUIMarker.lookAtThisPlayer)
            {
                _skullTag.SetActive(player.playerInventory.playerOddballActive && player.team != _playerWorldUIMarker.lookAtThisPlayer.team);
                _shieldTag.SetActive(player.playerInventory.playerOddballActive && player.team == _playerWorldUIMarker.lookAtThisPlayer.team);


                _shieldTag.SetActive(player.playerInventory.hasEnnemyFlag && player.team == _playerWorldUIMarker.lookAtThisPlayer.team);
            }
        }
    }
}

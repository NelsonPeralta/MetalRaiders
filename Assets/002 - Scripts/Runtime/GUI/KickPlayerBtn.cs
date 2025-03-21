using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class KickPlayerBtn : MonoBehaviour
{
    [SerializeField] ScriptObjPlayerData _playerData;


    private void Awake()
    {
        gameObject.SetActive(false);
    }

    public void SetPlayerDataCell(ScriptObjPlayerData sopd)
    {
        if (GameManager.instance.connection == GameManager.Connection.Online && PhotonNetwork.IsMasterClient)
        {
            _playerData = sopd;

            foreach (ScriptObjPlayerData s in CurrentRoomManager.instance.playerDataCells)
            {
                if (s.occupied && s.local && sopd == s)
                {
                    gameObject.SetActive(false);
                }
            }
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void KickPlayer()
    {
        if (_playerData)
        {
            print($"Kick Player: {_playerData.playerExtendedPublicData.player_id}");

            NetworkGameManager.instance.KickPlayerWithDatabaseId(_playerData.playerExtendedPublicData.player_id);
        }
    }
}

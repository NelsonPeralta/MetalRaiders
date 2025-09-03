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
        print("SetPlayerDataCell");
        if (GameManager.instance.connection == GameManager.NetworkType.Internet && PhotonNetwork.IsMasterClient)
        {
            _playerData = sopd;

            foreach (ScriptObjPlayerData s in CurrentRoomManager.instance.playerDataCells)
            {
                if (s.occupied && !s.local && sopd == s && s.rewiredId == 0)
                {
                    gameObject.SetActive(true);
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

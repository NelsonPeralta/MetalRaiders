using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class KickPlayerBtn : MonoBehaviour
{
    /// <summary>
    /// /////////// WARNING. This script is used as a child for Name Plate and Scoreboard Row
    /// </summary>
    public ScriptObjPlayerData playerData { get { return _playerData; } }

    [SerializeField] ScriptObjPlayerData _playerData;


    public void SetPlayerDataCell(ScriptObjPlayerData sopd)
    {
        Log.Print("SetPlayerDataCell");
        if (GameManager.instance.connection == GameManager.NetworkType.Internet && PhotonNetwork.IsMasterClient)
        {
            if (!sopd.local && sopd.rewiredId == 0)
            {
                _playerData = sopd;
                Log.Print($"KickPlayerBtn {name} show");
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
        else
        {
            Log.Print($"KickPlayerBtn {name} hide");
            gameObject.SetActive(false);
        }
    }

    public void KickPlayer()
    {
        if (_playerData)
        {
            Log.Print($"Kick Player: {_playerData.playerExtendedPublicData.player_id}");

            NetworkGameManager.instance.KickPlayerWithDatabaseId(_playerData.playerExtendedPublicData.player_id);
        }
    }

    private void Update()
    {
        if (_playerData)
        {
            if (!GameManager.GetPlayerWithSteamIdAndRewId(_playerData.steamId, _playerData.rewiredId)) // we check if the player left of was kicked
            {
                gameObject.SetActive(false);
            }
        }
    }
}

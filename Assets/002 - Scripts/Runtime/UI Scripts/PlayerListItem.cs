using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerListItem : MonoBehaviourPunCallbacks
{
    public TMP_Text text;
    public TMP_Text levelText;
    Photon.Realtime.Player player;

    public PlayerDatabaseAdaptor pda
    {
        get { return _pda; }
        set
        {
            _pda = value;
        }
    }

    PlayerDatabaseAdaptor _pda;

    public void SetUp(Photon.Realtime.Player _player) // MAIN
    {
        player = _player;
        WebManager.webManagerInstance.SetPlayerListItemInRoom(_player.NickName, this);
    }

    public void SetUp(string s)
    {
        Debug.Log($"Setup {s}");
        //text.text = s;
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        if (player == otherPlayer)
        {
            Destroy(gameObject);
        }
    }

    public override void OnLeftRoom()
    {
        Destroy(gameObject);
    }
}
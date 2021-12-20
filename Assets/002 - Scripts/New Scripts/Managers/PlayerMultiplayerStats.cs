using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerMultiplayerStats : MonoBehaviourPunCallbacks
{
    public delegate void PlayerMultiplayerStatsEvent(PlayerMultiplayerStats playerMultiplayerStats);
    // Events
    public PlayerMultiplayerStatsEvent OnKillsChanged;
    public PlayerMultiplayerStatsEvent OnDeathsChanged;
    public PlayerMultiplayerStatsEvent OnHeadshotsChanged;
    public PlayerMultiplayerStatsEvent OnKDRatioChanged;

    public PlayerProperties player;
    public int PVID;
    public string playerName;
    public int kills;
    public int deaths;
    int _headshots;
    float _kd;

    public int headshots
    {
        get { return _headshots; }
        set
        {
            var previous = _headshots;

            _headshots = Mathf.Clamp(value, 0, 999);

            if (_headshots != previous)
            {
                OnHeadshotsChanged?.Invoke(this);
            }
        }
    }

    public float kd
    {
        get { return _kd; }
        set
        {
            var previous = _kd;

            _kd = Mathf.Clamp(value, 0, 999);

            if (_kd != previous)
            {
                OnKDRatioChanged?.Invoke(this);
            }
        }
    }

    ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable();
    private void Start()
    {
        if (player)
        {
            PVID = player.PV.ViewID;
            playerName = player.PV.Owner.NickName;
        }

        this.OnKillsChanged += this.OnKillsChange;
    }

    void OnKillsChange(PlayerMultiplayerStats playerMultiplayerStats)
    {
        if(deaths > 0)
            _kd = (kills / deaths);
    }

    public PlayerMultiplayerStats(PlayerProperties pp)
    {
        player = pp;
        PVID = pp.GetComponent<PhotonView>().ViewID;
        playerName = pp.GetComponent<PhotonView>().Owner.NickName;
    }

    public PlayerMultiplayerStats(int pvid, string pName, int k, int d)
    {
        PVID = pvid;
        playerName = pName;
        kills = k;
        deaths = d;
    }

    public void AddKill(int pointsToWin)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;
        if (customProperties == null)
            customProperties = new ExitGames.Client.Photon.Hashtable();
        Debug.Log("here");

        if (!customProperties.ContainsKey($"{playerName}_kills"))
            customProperties.Add($"{playerName}_kills", 0);
        if (!customProperties.ContainsKey($"points_to_win"))
            customProperties.Add($"points_to_win", 1000);
        customProperties[$"{playerName}_kills"] = (int)customProperties[$"{playerName}_kills"] + 1;
        PhotonNetwork.SetPlayerCustomProperties(customProperties);
    }

    public void AddDeath()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;
        if (customProperties == null)
            customProperties = new ExitGames.Client.Photon.Hashtable();
        Debug.Log("here");

        if (!customProperties.ContainsKey($"{playerName}_deaths"))
            customProperties.Add($"{playerName}_deaths", 0);
        customProperties[$"{playerName}_deaths"] = (int)customProperties[$"{playerName}_deaths"] + 1;
        PhotonNetwork.SetPlayerCustomProperties(customProperties);
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);
        if (changedProps.ContainsKey($"{playerName}_kills"))
        {
            Debug.Log($"On Properties Updtate: {changedProps}. Player: {targetPlayer}");
            kills = (int)changedProps[$"{playerName}_kills"];
            player.allPlayerScripts.playerUIComponents.multiplayerPointsRed.text = $"{kills}";

            OnKillsChanged?.Invoke(this);
        }

        if (changedProps.ContainsKey($"{playerName}_deaths"))
        {
            Debug.Log($"On Properties Updtate: {changedProps}. Player: {targetPlayer}");
            deaths = (int)changedProps[$"{playerName}_deaths"];

            OnDeathsChanged?.Invoke(this);
        }
    }
}

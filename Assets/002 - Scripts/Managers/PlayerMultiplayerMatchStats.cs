using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerMultiplayerMatchStats : MonoBehaviourPunCallbacks
{
    public delegate void PlayerMultiplayerStatsEvent(PlayerMultiplayerMatchStats playerMultiplayerStats);
    // Events
    public PlayerMultiplayerStatsEvent OnKillsChanged, OnDeathsChanged, OnHeadshotsChanged, OnKDRatioChanged;

    // private variables
    [SerializeField] int _kills;
    [SerializeField] int _deaths;
    [SerializeField] int _headshots;
    [SerializeField] float _kd;

    public int kills
    {
        get { return _kills; }
        set
        {
            var previous = _kills;

            _kills = Mathf.Clamp(value, 0, 999);

            if (_kills != previous)
            {
                OnKillsChanged?.Invoke(this);
            }
            _kills = kills;
        }
    }
    public int deaths
    {
        get { return _deaths; }
        set
        {
            var previous = _deaths;

            _deaths = Mathf.Clamp(value, 0, 999);

            if (_deaths != previous)
            {
                OnDeathsChanged?.Invoke(this);
            }
            _deaths = deaths;
        }
    }

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
            _headshots = headshots;
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

    private void Start()
    {
        this.OnKillsChanged += this.OnKillsChange;
        kills = 0;
        deaths = 0;
        headshots = 0;
    }

    void OnKillsChange(PlayerMultiplayerMatchStats playerMultiplayerStats)
    {
        if(deaths > 0)
            _kd = (kills / deaths);
    }
}

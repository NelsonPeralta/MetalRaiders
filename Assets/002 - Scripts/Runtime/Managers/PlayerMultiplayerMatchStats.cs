using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

public class PlayerMultiplayerMatchStats : MonoBehaviourPunCallbacks
{
    public delegate void PlayerMultiplayerStatsEvent(PlayerMultiplayerMatchStats playerMultiplayerStats);
    // Events
    public PlayerMultiplayerStatsEvent OnPlayerScoreChanged, OnDeathsChanged, OnHeadshotsChanged, OnKDRatioChanged;

    public GameManager.Team team
    {
        get
        {
            return _player.team;
        }
    }

    public string username
    {
        get { return GetComponent<Player>().username; }
    }

    public int score
    {
        get
        {
            if (GameManager.instance.gameType == GameManager.GameType.Hill || GameManager.instance.gameType == GameManager.GameType.Oddball)
                return _score;

            return kills;
        }
        set
        {
            var previous = _score;

            _score = Mathf.Clamp(value, 0, 999);

            if (_score != previous)
            {
                OnPlayerScoreChanged?.Invoke(this);
            }
            //_score = _score;
        }
    }
    public int kills
    {
        get { return _kills; }
        set
        {
            var previous = _kills;

            _kills = Mathf.Clamp(value, 0, 999);

            if (_kills != previous)
            {
                OnPlayerScoreChanged?.Invoke(this);
            }
            //_kills = kills;
        }
    }

    public int damage
    {
        get { return _damage; }
        set { _damage = value; }
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
            //_deaths = deaths;
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
            //_headshots = headshots;
        }
    }

    public int meleeKills
    {
        get { return _meleeKills; }
        set
        {
            var previous = _meleeKills;

            _meleeKills = Mathf.Clamp(value, 0, 999);
        }
    }

    public int grenadeKills
    {
        get { return _grenadeKills; }
        set
        {
            var previous = _grenadeKills;

            _grenadeKills = Mathf.Clamp(value, 0, 999);
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


    public Player player { get { return _player; } }

    // private variables
    [SerializeField] int _score;
    [SerializeField] int _kills;
    [SerializeField] int _damage;
    [SerializeField] int _deaths;
    [SerializeField] int _headshots;
    [SerializeField] int _meleeKills;
    [SerializeField] int _grenadeKills;
    [SerializeField] float _kd;


    Player _player;

    private void Start()
    {
        _player = GetComponent<Player>();
        this.OnPlayerScoreChanged += this.OnKillsChange;
        kills = 0;
        deaths = 0;
        headshots = 0;

        //if (GetComponent<Player>().isMine)
        //    team = GameManager.instance.onlineTeam;
    }

    void OnKillsChange(PlayerMultiplayerMatchStats playerMultiplayerStats)
    {
        if (deaths > 0)
            _kd = (kills / deaths);
    }
}

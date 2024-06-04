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
            //if (GameManager.instance.gameType == GameManager.GameType.Hill || GameManager.instance.gameType == GameManager.GameType.Oddball)
            //    return player.playerDataCell.playerCurrentGameScore.score;

            return player.playerDataCell.playerCurrentGameScore.score;
        }
        set
        {
            var previous = player.playerDataCell.playerCurrentGameScore.score;

            player.playerDataCell.playerCurrentGameScore.ChangeScore(Mathf.Clamp(value, 0, 999));

            if (player.playerDataCell.playerCurrentGameScore.score != previous)
            {
                OnPlayerScoreChanged?.Invoke(this);
            }
            //_score = _score;
        }
    }
    public int kills
    {
        get { return player.playerDataCell.playerCurrentGameScore.kills; }
        set
        {
            var previous = player.playerDataCell.playerCurrentGameScore.kills;

            player.playerDataCell.playerCurrentGameScore.kills = Mathf.Clamp(value, 0, 999);

            if (player.playerDataCell.playerCurrentGameScore.kills != previous)
            {
                OnPlayerScoreChanged?.Invoke(this);
            }
            //_kills = kills;
        }
    }

    public int damage
    {
        get { return player.playerDataCell.playerCurrentGameScore.damage; }
        set { player.playerDataCell.playerCurrentGameScore.damage = value; }
    }
    public int deaths
    {
        get { return player.playerDataCell.playerCurrentGameScore.deaths; }
        set
        {
            var previous = player.playerDataCell.playerCurrentGameScore.deaths;

            player.playerDataCell.playerCurrentGameScore.deaths = Mathf.Clamp(value, 0, 999);

            if (player.playerDataCell.playerCurrentGameScore.deaths != previous)
            {
                OnDeathsChanged?.Invoke(this);
            }
            //_deaths = deaths;
        }
    }

    public int headshots
    {
        get { return player.playerDataCell.playerCurrentGameScore.headshots; }
        set
        {
            var previous = player.playerDataCell.playerCurrentGameScore.headshots;

            player.playerDataCell.playerCurrentGameScore.headshots = Mathf.Clamp(value, 0, 999);

            if (player.playerDataCell.playerCurrentGameScore.headshots != previous)
            {
                OnHeadshotsChanged?.Invoke(this);
            }
            //_headshots = headshots;
        }
    }

    public int meleeKills
    {
        get { return player.playerDataCell.playerCurrentGameScore.meleeKills; }
        set
        {
            var previous = player.playerDataCell.playerCurrentGameScore.meleeKills;

            player.playerDataCell.playerCurrentGameScore.meleeKills = Mathf.Clamp(value, 0, 999);
        }
    }

    public int grenadeKills
    {
        get { return player.playerDataCell.playerCurrentGameScore.grenadeKills; }
        set
        {
            var previous = player.playerDataCell.playerCurrentGameScore.grenadeKills;

            player.playerDataCell.playerCurrentGameScore.grenadeKills = Mathf.Clamp(value, 0, 999);
        }
    }

    public float kd
    {
        get { return player.playerDataCell.playerCurrentGameScore.kd; }
        set
        {
            var previous = player.playerDataCell.playerCurrentGameScore.kd;

            player.playerDataCell.playerCurrentGameScore.kd = Mathf.Clamp(value, 0, 999);

            if (player.playerDataCell.playerCurrentGameScore.kd != previous)
            {
                OnKDRatioChanged?.Invoke(this);
            }
        }
    }


    public Player player { get { return _player; } }



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
            player.playerDataCell.playerCurrentGameScore.kd = (kills / deaths);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerSwarmMatchStats : MonoBehaviourPunCallbacks
{
    public delegate void PlayerSwarmEvent(PlayerSwarmMatchStats onlinePlayerSwarmScript);
    public PlayerSwarmEvent OnPointsChanged, OnKillsChanged, OnDeathsChanged, OnHeadshotsChanged;

    [SerializeField] Player _player;


    public int points
    {
        get { return _player.playerDataCell.playerCurrentGameScore.points; }
        private set
        {
            int previousValue = _player.playerDataCell.playerCurrentGameScore.points;

            _player.playerDataCell.playerCurrentGameScore.points = value;
            _player.playerDataCell.playerCurrentGameScore.ChangeScore(value);


            if (previousValue != value)
            {
                OnPointsChanged?.Invoke(this);
            }
        }
    }

    public int totalPoints
    {
        get { return _player.playerDataCell.playerCurrentGameScore.totalPoints; }
        private set
        {
            _player.playerDataCell.playerCurrentGameScore.totalPoints = value;

            if (_player.playerDataCell.local
                && _player.playerDataCell.rewiredId == 0
                && _player.playerDataCell.playerCurrentGameScore.totalPoints >= 1000000
                && !player.hasArmor
                && !_achvCheck)
            {
                bool _achUn = false;

                Steamworks.SteamUserStats.GetAchievement("OMA", out _achUn);
                if (!_achUn && _player.isMine)
                {
                    _achvCheck = true;
                    AchievementManager.UnlockAchievement("OMA");

                    if (!CurrentRoomManager.instance.playerDataCells[0].playerExtendedPublicData.unlocked_armor_data_string.Contains("gps-lfa"))
                        WebManager.webManagerInstance.StartCoroutine(WebManager.UnlockArmorPiece_Coroutine("-burning-helmet-"));
                }
            }
        }
    }


    public int kills
    {
        get { return _player.playerDataCell.playerCurrentGameScore.kills; }
        set
        {
            var previous = _player.playerDataCell.playerCurrentGameScore.kills;

            _player.playerDataCell.playerCurrentGameScore.kills = Mathf.Clamp(value, 0, 999);

            if (_player.playerDataCell.playerCurrentGameScore.kills != previous)
            {
                OnKillsChanged?.Invoke(this);
            }
        }
    }

    public int deaths
    {
        get { return _player.playerDataCell.playerCurrentGameScore.deaths; }
        set
        {
            var previous = _player.playerDataCell.playerCurrentGameScore.deaths;

            _player.playerDataCell.playerCurrentGameScore.deaths = Mathf.Clamp(value, 0, 999);

            if (_player.playerDataCell.playerCurrentGameScore.deaths != previous)
            {
                OnDeathsChanged?.Invoke(this);
            }
        }
    }

    public int headshots
    {
        get { return _player.playerDataCell.playerCurrentGameScore.headshots; }
        set
        {
            var previous = _player.playerDataCell.playerCurrentGameScore.headshots;

            _player.playerDataCell.playerCurrentGameScore.headshots = Mathf.Clamp(value, 0, 999);

            if (_player.playerDataCell.playerCurrentGameScore.headshots != previous)
            {
                OnHeadshotsChanged?.Invoke(this);
            }
        }
    }

    public Player player { get { return _player; } }















    bool _achvCheck;




    private void Awake()
    {
        _player = gameObject.GetComponent<Player>();
    }

    private void Start()
    {
        OnPointsChanged += OnPointsChanged_Delegate;
    }
    public void AddPoints(int _points, bool isBonusPoints = false)
    {
        if (isBonusPoints)
        {
            Debug.Log("Bonus points");
            GetComponent<PlayerUI>().AddInformerText($"Wave End! Bonus Points: {_points}");
        }
        GetComponent<PhotonView>().RPC("AddPoints_RPC", RpcTarget.All, _points);
    }

    [PunRPC]
    void AddPoints_RPC(int _points)
    {
        points += _points;
        totalPoints += _points;
    }

    public void RemovePoints(int _points)
    {
        GetComponent<PhotonView>().RPC("RemovePoints_RPC", RpcTarget.All, _points);
    }

    [PunRPC]
    void RemovePoints_RPC(int _points)
    {
        points -= _points;
    }

    public int GetPoints()
    {
        return points;
    }

    public int GetTotalPoints()
    {
        return _player.playerDataCell.playerCurrentGameScore.totalPoints;
    }

    void OnPointsChanged_Delegate(PlayerSwarmMatchStats onlinePlayerSwarmScript)
    {
        GetComponent<PlayerUI>().swarmPointsText.text = points.ToString();
        //GetComponent<PlayerUI>().UpdateBottomRighScore();
    }

    public void ResetPoints()
    {
        GetComponent<PhotonView>().RPC("ResetPoints_RPC", RpcTarget.All);

    }

    [PunRPC]
    void ResetPoints_RPC()
    {
        points = 0;
        totalPoints = 0;
    }
}

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
    public PlayerMultiplayerStatsEvent OnKillsChanged, OnDeathsChanged, OnHeadshotsChanged, OnKDRatioChanged;

    public enum Team { None, Red, Blue }

    // private variables
    [SerializeField] int _kills;
    [SerializeField] int _deaths;
    [SerializeField] int _headshots;
    [SerializeField] float _kd;
    [SerializeField] Team _team;

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

    public Team team
    {
        get { return _team; }
        set { _team = value; Debug.Log(value); }
    }

    private void Start()
    {
        this.OnKillsChanged += this.OnKillsChange;
        kills = 0;
        deaths = 0;
        headshots = 0;

        StartCoroutine(ChangeTeam_Coroutine());
    }

    IEnumerator ChangeTeam_Coroutine()
    {
        yield return new WaitForSeconds(1);

        int c = 1;

        if (GameManager.instance.gameType == GameManager.GameType.TeamSlayer)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                Player[] pa = FindObjectsOfType<Player>();

                List<Player> pl = pa.ToList();

                foreach (Player p in pl)
                {
                    Team t = Team.Red;

                    if (c % 2 == 0)
                        t = Team.Blue;

                    if (p.controllerId == 0)
                    {
                        p.GetComponent<PlayerMultiplayerMatchStats>().ChangeTeam(t);
                        c++;
                    }
                }
            }
        }
    }

    void OnKillsChange(PlayerMultiplayerMatchStats playerMultiplayerStats)
    {
        if (deaths > 0)
            _kd = (kills / deaths);
    }

    public void ChangeTeam(Team t)
    {
        GetComponent<PhotonView>().RPC("ChangeTeam_RPC", RpcTarget.All, t);
    }

    [PunRPC]
    void ChangeTeam_RPC(Team t)
    {
        foreach (Player p in FindObjectsOfType<Player>().ToList())
            if (p.isMine)
                p.GetComponent<PlayerMultiplayerMatchStats>().team = t;
    }
}

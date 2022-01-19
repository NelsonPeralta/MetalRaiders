using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class OnlinePlayerSwarmScript : MonoBehaviourPunCallbacks
{
    public PhotonView PV;
    public AllPlayerScripts allPlayerScripts;
    [SerializeField] int _points;
    [SerializeField] int totalPoints;

    public delegate void PlayerSwarmEvent(OnlinePlayerSwarmScript onlinePlayerSwarmScript);
    public PlayerSwarmEvent OnPointsChanged, OnKillsChanged, OnDeathsChanged, OnHeadshotsChanged;
    [SerializeField] int _kills;
    [SerializeField] int _deaths;
    [SerializeField] int _headshots;

    public int points
    {
        get { return _points; }
        set
        {
            if (_points != value)
                OnPointsChanged?.Invoke(this);
            _points = value;
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
                OnKillsChanged?.Invoke(this);
            }
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
        }
    }

    private void Start()
    {
        OnPointsChanged += OnPointsChanged_Delegate;
    }
    public void AddPoints(int _points)
    {
        PV.RPC("AddPoints_RPC", RpcTarget.All, _points);
    }

    [PunRPC]
    void AddPoints_RPC(int _points)
    {
        points += _points;
        totalPoints += _points;
    }

    public void RemovePoints(int _points)
    {
        PV.RPC("RemovePoints_RPC", RpcTarget.All, _points);
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
        return totalPoints;
    }

    void OnPointsChanged_Delegate(OnlinePlayerSwarmScript onlinePlayerSwarmScript)
    {
        GetComponent<PlayerUI>().swarmPointsText.text = points.ToString();
    }

    public void ResetPoints()
    {
        Debug.Log(PV);
        PV.RPC("ResetPoints_RPC", RpcTarget.All);

    }

    [PunRPC]
    void ResetPoints_RPC()
    {
        points = 0;
        totalPoints = 0;
    }
}

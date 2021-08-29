using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class OnlinePlayerSwarmScript : MonoBehaviourPunCallbacks
{
    public PhotonView PV;
    public AllPlayerScripts allPlayerScripts;
    [SerializeField] int points;
    [SerializeField] int totalPoints;
        public void AddPoints(int _points)
    {
        PV.RPC("AddPoints_RPC", RpcTarget.All, _points);
    }

    [PunRPC]
    void AddPoints_RPC(int _points)
    {
        points += _points;
        totalPoints += _points;
        UpdatePointText();
    }

    public void RemovePoints(int _points)
    {
        PV.RPC("RemovePoints_RPC", RpcTarget.All, _points);
    }

    [PunRPC]
    void RemovePoints_RPC(int _points)
    {
        points -= _points;
        UpdatePointText();
    }

    public int GetPoints()
    {
        return points;
    }

    public int GetTotalPoints()
    {
        return totalPoints;
    }

    void UpdatePointText()
    {
        allPlayerScripts.playerUIComponents.swarmPointsText.text = points.ToString();
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

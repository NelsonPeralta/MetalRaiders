using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NetworkMainMenu : MonoBehaviourPunCallbacks
{
    public delegate void MainMenuCallerEvent(NetworkMainMenu mainMenuCaller);
    public MainMenuCallerEvent mainMenuCallerCreated;
    private void Start()
    {
        UpdatePlayerList();
    }

    public override void OnLeftRoom()
    {
        Debug.Log($"NetworkMainMenu OnLeftRoom");
        //base.OnLeftRoom();

        //Destroy(gameObject);
    }


    [PunRPC]
    public void UpdatePlayerList()
    {
        PlayerDatabaseAdaptor pda = WebManager.webManagerInstance.pda;
        Photon.Realtime.Player[] players = PhotonNetwork.PlayerList;

        foreach (Transform child in Launcher.instance.playerListContent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < players.Count(); i++)
        {
            Debug.Log(players[i].NickName);
            GameObject plt = Instantiate(Launcher.instance.playerListItemPrefab, Launcher.instance.playerListContent);
            plt.GetComponent<PlayerListItem>().SetUp(players[i]);
            plt.GetComponent<PlayerListItem>().levelText.text = pda.playerBasicOnlineStats.level.ToString();
        }
    }

    [PunRPC]
    public void UpdateSelectedMap(int index)
    {
        Launcher.instance.levelToLoadIndex = index;
        string mode = PhotonNetwork.CurrentRoom.CustomProperties["gamemode"].ToString();

        if (mode == "multiplayer")
            Launcher.instance.mapSelectedText.text = $"Map: {Launcher.NameFromIndex(index).Replace("PVP - ", "")}";
        if (mode == "swarm")
            Launcher.instance.mapSelectedText.text = $"Map: {Launcher.NameFromIndex(index).Replace("Coop - ", "")}";

    }

    public void UpdateRoomSettings(Dictionary<string, string> roomParams)
    {
        GetComponent<PhotonView>().RPC("UpdateRoomSettings_RPC", RpcTarget.All, roomParams);
    }

    [PunRPC]
    void UpdateRoomSettings_RPC(Dictionary<string, string> roomParams)
    {
        try
        {
            GameManager.instance.gameMode = (GameManager.GameMode)System.Enum.Parse(typeof(GameManager.GameMode), roomParams["gamemode"]);
            GameManager.instance.gameType = (GameManager.GameType)System.Enum.Parse(typeof(GameManager.GameType), roomParams["multiplayermode"]);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"No such gamemode. {e}");
        }
    }

    [PunRPC]
    void ChangeSubGameType_RPC(string sgt)
    {
        Launcher.instance.gametypeSelectedText.text = $"Gametype: {sgt}";
        sgt = sgt.Replace(" ", "");
        GameManager.instance.gameType = (GameManager.GameType)System.Enum.Parse(typeof(GameManager.GameType), sgt);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuCaller : MonoBehaviour
{
    public delegate void MainMenuCallerEvent(MainMenuCaller mainMenuCaller);
    public MainMenuCallerEvent mainMenuCallerCreated;
    private void Start()
    {
        UpdatePlayerList();
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
        string mode = PhotonNetwork.CurrentRoom.CustomProperties["mode"].ToString();

        if (mode == "multiplayer")
            Launcher.instance.mapSelectedText.text = $"Map: {Launcher.NameFromIndex(index).Replace("PVP - ", "")}";
        if (mode == "swarm")
            Launcher.instance.mapSelectedText.text = $"Map: {Launcher.NameFromIndex(index).Replace("Coop - ", "")}";

    }

    public void UpdateRoomSettings(Dictionary<string, string> roomParams)
    {
        GetComponent<PhotonView>().RPC("UpdateRoomSettings_RPC", RpcTarget.All,  roomParams);
    }

    [PunRPC]
    void UpdateRoomSettings_RPC(Dictionary<string, string> roomParams)
    {
        try
        {
            GameManager.instance.gameMode = (GameManager.GameMode)System.Enum.Parse(typeof(GameManager.GameMode), roomParams["gamemode"]);
            GameManager.instance.multiplayerMode = (GameManager.MultiplayerMode)System.Enum.Parse(typeof(GameManager.MultiplayerMode), roomParams["multiplayermode"]);
            GameManager.instance.swarmMode = (GameManager.SwarmMode)System.Enum.Parse(typeof(GameManager.SwarmMode), roomParams["swarmmode"]);
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
        GameManager.instance.multiplayerMode = (GameManager.MultiplayerMode)System.Enum.Parse(typeof(GameManager.MultiplayerMode), sgt);
    }
}

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
    [PunRPC]
    public void UpdatePlayerList()
    {
        PlayerDatabaseAdaptor pda = WebManager.webManagerInstance.playerDatabaseAdaptor;
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
}

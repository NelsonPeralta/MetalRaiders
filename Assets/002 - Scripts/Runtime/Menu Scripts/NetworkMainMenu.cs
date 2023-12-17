using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

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

    public void UpdateRoomSettings(Dictionary<string, string> roomParams)
    {
        GetComponent<PhotonView>().RPC("UpdateRoomSettings_RPC", RpcTarget.All, roomParams);
    }

    IEnumerator LateStart()
    {
        yield return new WaitForEndOfFrame();
        //UpdateRoomSettings();
    }


    [PunRPC]
    public void UpdatePlayerList()
    {
        PlayerDatabaseAdaptor pda = WebManager.webManagerInstance.pda;
        Photon.Realtime.Player[] players = PhotonNetwork.PlayerList;

        foreach (Transform child in Launcher.instance.namePlatesParent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < players.Count(); i++)
        {
            Debug.Log(players[i].NickName);



            GameObject plt = Instantiate(Launcher.instance.namePlatePrefab, Launcher.instance.namePlatesParent);
            plt.GetComponent<PlayerNamePlate>().SetUp(players[i]);
            //plt.GetComponent<PlayerNamePlate>().playerLevel = pda.playerBasicOnlineStats.level;

            if (CurrentRoomManager.instance.playerNicknameNbLocalPlayersDict.ContainsKey(players[i].NickName)
                && CurrentRoomManager.instance.playerNicknameNbLocalPlayersDict[players[i].NickName] > 1)
                for (int j = 1; j < CurrentRoomManager.instance.playerNicknameNbLocalPlayersDict[players[i].NickName]; j++)
                {
                    GameObject _plt = Instantiate(Launcher.instance.namePlatePrefab, Launcher.instance.namePlatesParent);
                    _plt.GetComponent<PlayerNamePlate>().SetUp(players[i].NickName + $" ({j})"); // DEPRECATED
                }
        }
    }

    [PunRPC]
    public void UpdateSelectedMap(int index)
    {
        Launcher.instance.levelToLoadIndex = index;
        string mode = PhotonNetwork.CurrentRoom.CustomProperties["gamemode"].ToString();

        try
        {
            if (PhotonNetwork.IsMasterClient)
            {
                ExitGames.Client.Photon.Hashtable props = PhotonNetwork.CurrentRoom.CustomProperties;
                //ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
                props.Add("leveltoloadindex", index);
                PhotonNetwork.CurrentRoom.SetCustomProperties(props);
                Debug.Log($"UpdateSelectedMap: {PhotonNetwork.CurrentRoom.CustomProperties["leveltoloadindex"]}");
            }
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
        }

        Debug.Log($"UpdateSelectedMap: {PhotonNetwork.CurrentRoom.CustomProperties["gamemode"].ToString()}");

        if (mode == "multiplayer")
            Launcher.instance.mapSelectedText.text = $"Map: {Launcher.NameFromIndex(index).Replace("PVP - ", "")}";
        if (mode == "swarm")
            Launcher.instance.mapSelectedText.text = $"Map: {Launcher.NameFromIndex(index).Replace("Coop - ", "")}";

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

        try
        {
            if (PhotonNetwork.IsMasterClient)
            {
                ExitGames.Client.Photon.Hashtable props = PhotonNetwork.CurrentRoom.CustomProperties;
                //ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
                props.Add("gametype", (int)GameManager.instance.gameType);
                PhotonNetwork.CurrentRoom.SetCustomProperties(props);
                Debug.Log($"UpdateSelectedMap: {PhotonNetwork.CurrentRoom.CustomProperties["gametype"]}");
            }
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
        }
    }
}

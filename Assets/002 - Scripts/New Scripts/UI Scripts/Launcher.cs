using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using System.Linq;
using UnityEngine.SceneManagement;

public class Launcher : MonoBehaviourPunCallbacks
{
    public static Launcher launcherInstance; // Singleton of the Photon Launcher
    public PhotonView PV;
    public int levelToLoadIndex;

    // SerializeField makes private variables visible in the inspector
    [SerializeField] TMP_InputField roomNameInputField;
    [SerializeField] TMP_InputField nicknameInputField;
    [SerializeField] TMP_Text errorText;
    [SerializeField] GameObject commonRoomTexts;
    [SerializeField] TMP_Text roomNameText;
    [SerializeField] Transform roomListContent;
    [SerializeField] GameObject roomListItemPrefab;
    [SerializeField] Transform playerListContent;
    [SerializeField] GameObject PlayerListItemPrefab;
    [SerializeField] TMP_Text mapSelectedText;

    [Header("Master Client Only")]
    [SerializeField] GameObject startGameButton;
    [SerializeField] GameObject mapSelector;
    [SerializeField] GameObject multiplayerMapSelector;
    [SerializeField] GameObject swarmMapSelector;

    void Awake()
    {
        launcherInstance = this;
    }

    void Start()
    {
        if (levelToLoadIndex == 0)
            levelToLoadIndex = 1;
        Debug.Log("Connecting to Master");
        PhotonNetwork.ConnectUsingSettings();
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        ChangeLevelToLoadWithIndex(levelToLoadIndex);
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master");
        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public override void OnJoinedLobby()
    {
        MenuManager.Instance.OpenMenu("title");
        Debug.Log("Joined Lobby");
        if (PhotonNetwork.NickName == "")
            PhotonNetwork.NickName = "Player " + Random.Range(0, 1000).ToString("0000");
    }

    public void CreateMultiplayerRoom()
    {
        RoomOptions options = new RoomOptions();
        options.BroadcastPropsChangeToAll = true;
        options.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable();
        options.CustomRoomProperties.Add("mode", "multiplayer");
        if (string.IsNullOrEmpty(roomNameInputField.text)) // If there is no text in the input field of the room name we want to create
        {
            return; // Do nothing
        }

        // else
        PhotonNetwork.CreateRoom(roomNameInputField.text, options); // Create a room with the text in parameter
        MenuManager.Instance.OpenMenu("loading"); // Show the loading menu/message

        // When creating a room is done, OnJoinedRoom() will automatically trigger
    }

    public void CreateSwarmRoom()
    {
        RoomOptions options = new RoomOptions();
        options.BroadcastPropsChangeToAll = true;
        options.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable();
        options.CustomRoomProperties.Add("mode", "swarm");
        if (string.IsNullOrEmpty(roomNameInputField.text)) // If there is no text in the input field of the room name we want to create
        {
            return; // Do nothing
        }

        // else
        PhotonNetwork.CreateRoom(roomNameInputField.text, options); // Create a room with the text in parameter
        MenuManager.Instance.OpenMenu("loading"); // Show the loading menu/message

        // When creating a room is done, OnJoinedRoom() will automatically trigger
    }

    public override void OnJoinedRoom()
    {
        Debug.Log(PhotonNetwork.CurrentRoom.CustomProperties["mode"].ToString());
        Debug.Log(PhotonNetwork.CurrentRoom.Name);
        string roomType = PhotonNetwork.CurrentRoom.CustomProperties["mode"].ToString() + "_room";
        string mode = PhotonNetwork.CurrentRoom.CustomProperties["mode"].ToString();

        commonRoomTexts.SetActive(true);
        MenuManager.Instance.OpenMenu(roomType); // Show the "room" menu
        roomNameText.text = PhotonNetwork.CurrentRoom.Name; // Change the name of the room to the one given 

        UpdatePlayerList();

        Debug.Log($"Is Master Client: {PV.ViewID} and Master Client: {PhotonNetwork.IsMasterClient}");
        startGameButton.SetActive(PhotonNetwork.IsMasterClient);
        if(mode == "multiplayer")
        {
            mapSelector = multiplayerMapSelector;
            mapSelector.SetActive(PhotonNetwork.IsMasterClient);
        }
        if(mode == "swarm")
        {
            mapSelector = swarmMapSelector;
            mapSelector.SetActive(PhotonNetwork.IsMasterClient);
        }
    }

    [PunRPC]
    public void UpdatePlayerList()
    {
        Player[] players = PhotonNetwork.PlayerList;

        foreach (Transform child in playerListContent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < players.Count(); i++)
        {
            Instantiate(PlayerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(players[i]);
        }
    }

    public void UpdateNickname()
    {
        PhotonNetwork.NickName = nicknameInputField.text;
        PV.RPC("UpdatePlayerList", RpcTarget.All);
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        startGameButton.SetActive(PhotonNetwork.IsMasterClient);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        errorText.text = "Room Creation Failed: " + message;
        Debug.LogError("Room Creation Failed: " + message);
        MenuManager.Instance.OpenMenu("error");
    }

    public void StartGame()
    {
        PhotonNetwork.LoadLevel(levelToLoadIndex);
    }

    public void LeaveRoom()
    {
        commonRoomTexts.SetActive(false);
        PhotonNetwork.LeaveRoom();
        MenuManager.Instance.OpenMenu("loading");
    }

    public void JoinRoom(RoomInfo info)
    {
        PhotonNetwork.JoinRoom(info.Name);
        MenuManager.Instance.OpenMenu("loading");
    }

    public override void OnLeftRoom()
    {
        MenuManager.Instance.OpenMenu("title");
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (Transform trans in roomListContent)
        {
            Destroy(trans.gameObject);
        }

        for (int i = 0; i < roomList.Count; i++)
        {
            if (roomList[i].RemovedFromList)
                continue;
            Instantiate(roomListItemPrefab, roomListContent).GetComponent<RoomListItem>().SetUp(roomList[i]);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Instantiate(PlayerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(newPlayer);
    }

    public void ChangeLevelToLoadWithIndex(int index)
    {
        PV.RPC("UpdateSelectedMap", RpcTarget.All, index);
    }

    [PunRPC]
    public void UpdateSelectedMap(int index)
    {
        levelToLoadIndex = index;
        mapSelectedText.text = $"Map: {NameFromIndex(index).Replace("PVP - ", "")}";
        mapSelectedText.text = $"Map: {NameFromIndex(index).Replace("Coop - ", "")}";
    }


    // By JimmyCushnie
    // Reference: https://answers.unity.com/questions/1262342/how-to-get-scene-name-at-certain-buildindex.html
    private static string NameFromIndex(int BuildIndex)
    {
        string path = SceneUtility.GetScenePathByBuildIndex(BuildIndex);
        int slash = path.LastIndexOf('/');
        string name = path.Substring(slash + 1);
        int dot = name.LastIndexOf('.');
        return name.Substring(0, dot);
    }
}
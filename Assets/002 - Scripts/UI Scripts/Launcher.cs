using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

public class Launcher : MonoBehaviourPunCallbacks
{
    private TypedLobby typedLobby = new TypedLobby("NelsonLobby", LobbyType.Default);

    // Events
    public delegate void LauncherEvent(Launcher launcher);
    public LauncherEvent OnCreateSwarmRoomButton;
    public LauncherEvent OnCreateMultiplayerRoomButton;

    public static Launcher instance; // Singleton of the Photon Launcher
    public PhotonView PV;
    public GameObject loginButton;

    #region
    public int levelToLoadIndex;
    [SerializeField] int testingRoomLevelIndex;
    public int waitingRoomLevelIndex;
    #endregion

    // SerializeField makes private variables visible in the inspector
    [SerializeField] TMP_InputField roomNameInputField;
    [SerializeField] TMP_InputField nicknameInputField;
    [SerializeField] TMP_Text errorText;
    [SerializeField] TMP_Text messageText;
    [SerializeField] GameObject commonRoomTexts;
    [SerializeField] TMP_Text roomNameText;
    [SerializeField] Transform roomListContent;
    [SerializeField] GameObject roomListItemPrefab;
    [SerializeField] Transform _playerListContent;
    [SerializeField] GameObject _playerListItemPrefab;
    [SerializeField] TMP_Text _mapSelectedText;
    [SerializeField] TMP_Text _gametypeSelectedText;

    [SerializeField] TMP_InputField _loginUsernameText;
    [SerializeField] TMP_InputField registerUsernameText;
    [SerializeField] TMP_InputField _loginPasswordText;
    [SerializeField] TMP_InputField registerPasswordText;

    [Header("Master Client Only")]
    [SerializeField] GameObject startGameButton;
    [SerializeField] GameObject mapSelector;
    [SerializeField] GameObject multiplayerMapSelector;
    [SerializeField] GameObject swarmMapSelector;

    public string quickMatchRoomName = "quick_match_room";

    public TMP_InputField loginUsernameText
    {
        get { return _loginUsernameText; }
    }

    public TMP_InputField loginPasswordText
    {
        get { return _loginPasswordText; }
    }

    public Transform playerListContent
    {
        get { return _playerListContent; }
    }

    public GameObject playerListItemPrefab
    {
        get { return _playerListItemPrefab; }
    }
    void Awake()
    {
        if (instance)
        {
            Debug.Log("There is a MenuManager Instance");
            Destroy(gameObject);
            return;
        }
        //DontDestroyOnLoad(gameObject);
        instance = this;

        FindObjectOfType<GameManager>().OnSceneLoadedEvent -= OnSceneLoaded;
        FindObjectOfType<GameManager>().OnSceneLoadedEvent += OnSceneLoaded;
    }

    public TMP_Text mapSelectedText
    {
        get { return _mapSelectedText; }
    }

    public TMP_Text gametypeSelectedText
    {
        get { return _gametypeSelectedText; }
    }

    void Start()
    {
        //if (Application.platform == RuntimePlatform.WindowsPlayer)
            Debug.Log($"You are playing on a {Application.platform}");

        if (levelToLoadIndex == 0)
            levelToLoadIndex = 1;

        ConnectToPhotonMasterServer();
        //GetComponent<MenuManager>().OpenMainMenu();
    }

    public void ConnectToPhotonMasterServer(bool showMessage = true)
    {
        Debug.Log($"ConnectToPhotonMasterServer");
        GetComponent<MenuManager>().OpenLoadingMenu("Connecting To Server...");

        PhotonNetwork.ConnectUsingSettings();
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        ChangeLevelToLoadWithIndex(levelToLoadIndex);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        Debug.Log($"Disconnected: {cause}");
        //ShowPlayerMessage($"Disconnected from Server: {cause}.\nReconnecting...");
        ConnectToPhotonMasterServer(false);
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master");
        //ShowPlayerMessage("Conneected To Master Server!");
        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true;
        GetComponent<MenuManager>().OpenMainMenu();

    }

    public override void OnJoinedLobby()
    {
        Scene currentScene = SceneManager.GetActiveScene();

        if (currentScene.buildIndex > 0) // We are not in the menu
        {
            PhotonNetwork.LoadLevel(0);
        }

        if (WebManager.webManagerInstance)
        {
            if (WebManager.webManagerInstance.pda.PlayerDataIsSet())
                MenuManager.Instance.OpenMenu("online title");
            else
                MenuManager.Instance.OpenMenu("offline title");
        }
        else
        {
            MenuManager.Instance.OpenMenu("offline title");
        }
        Debug.Log("Joined Lobby");
        if (PhotonNetwork.NickName == "")
            PhotonNetwork.NickName = "Player " + Random.Range(0, 1000).ToString("0000");
    }

    public void QuickMatch()
    {
        string roomName = quickMatchRoomName;
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = maxRandomRoomPlayers;
        roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable();
        roomOptions.CustomRoomProperties.Add("gamemode", GameManager.GameMode.Multiplayer.ToString());
        roomOptions.CustomRoomProperties.Add("gametype", GameManager.GameType.Fiesta.ToString()); 
        PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, typedLobby);
        //PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log($"OnJoinRandomFailed: {message}");
        //CreateAndJoinRandomRoom();
    }

    [SerializeField]
    private byte maxRandomRoomPlayers = 6;

    public void CreateMultiplayerRoom()
    {
        Debug.Log($"CreateMultiplayerRoom. Client State: {PhotonNetwork.NetworkClientState}");
        RoomOptions options = new RoomOptions();
        options.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable();
        options.CustomRoomProperties.Add("gamemode", "multiplayer");
        if (string.IsNullOrEmpty(roomNameInputField.text)) // If there is no text in the input field of the room name we want to create
        {
            return; // Do nothing
        }

        if (PhotonNetwork.NetworkClientState == ClientState.ConnectedToMasterServer)
        {
            //PhotonNetwork.JoinRandomRoom();

            // Can Join Room
        }
        else
        {
            //Debug.LogError("Can't join random room now, client is not ready");
        }

        // else
        PhotonNetwork.CreateRoom(roomNameInputField.text, options); // Create a room with the text in parameter
        MenuManager.Instance.OpenLoadingMenu("Creating Multiplayer Room..."); // Show the loading menu/message

        // When creating a room is done, OnJoinedRoom() will automatically trigger
        OnCreateMultiplayerRoomButton?.Invoke(this);
    }

    public void CreateSwarmRoom()
    {
        RoomOptions options = new RoomOptions();
        options.BroadcastPropsChangeToAll = true;
        options.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable();
        options.CustomRoomProperties.Add("gamemode", "swarm");
        if (string.IsNullOrEmpty(roomNameInputField.text)) // If there is no text in the input field of the room name we want to create
        {
            return; // Do nothing
        }

        // else
        PhotonNetwork.CreateRoom(roomNameInputField.text, options); // Create a room with the text in parameter
        MenuManager.Instance.OpenLoadingMenu("Creating Coop Room..."); // Show the loading menu/message

        // When creating a room is done, OnJoinedRoom() will automatically trigger

        OnCreateSwarmRoomButton?.Invoke(this);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined room");
        Debug.Log(PhotonNetwork.CurrentRoom.CustomProperties["gamemode"].ToString());
        Debug.Log(PhotonNetwork.CurrentRoom.Name);

        if (PhotonNetwork.CurrentRoom.Name == quickMatchRoomName)
        { // Room is Random
            GameManager.instance.gameMode = GameManager.GameMode.Multiplayer;
            GameManager.instance.gameType = GameManager.GameType.Fiesta;
            PhotonNetwork.LoadLevel(waitingRoomLevelIndex);
        }
        else
        { // Room is private

            if (PhotonNetwork.IsMasterClient)
                PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs/UI", "MainMenuCommunicator"), Vector3.zero, Quaternion.identity);
            string roomType = PhotonNetwork.CurrentRoom.CustomProperties["gamemode"].ToString().ToLower() + "_room";
            string mode = PhotonNetwork.CurrentRoom.CustomProperties["gamemode"].ToString().ToLower();

            commonRoomTexts.SetActive(true);
            MenuManager.Instance.OpenMenu(roomType); // Show the "room" menu
            roomNameText.text = PhotonNetwork.CurrentRoom.Name; // Change the name of the room to the one given 

            //Debug.Log($"Is Master Client: {FindObjectOfType<MainMenuCaller>().GetComponent<PhotonView>().ViewID} and Master Client: {PhotonNetwork.IsMasterClient}");
            startGameButton.SetActive(PhotonNetwork.IsMasterClient);
            if (mode == "multiplayer")
            {
                mapSelector = multiplayerMapSelector;
                mapSelector.SetActive(PhotonNetwork.IsMasterClient);
            }
            if (mode == "swarm")
            {
                mapSelector = swarmMapSelector;
                mapSelector.SetActive(PhotonNetwork.IsMasterClient);
            }
        }


    }
    //[PunRPC]
    //public void UpdatePlayerList()
    //{
    //    PlayerDatabaseAdaptor pda = WebManager.webManagerInstance.playerDatabaseAdaptor;
    //    Photon.Realtime.Player[] players = PhotonNetwork.PlayerList;
    //public void UpdatePlayerList()
    //{
    //    PlayerDatabaseAdaptor pda = WebManager.webManagerInstance.playerDatabaseAdaptor;
    //    Photon.Realtime.Player[] players = PhotonNetwork.PlayerList;

    ////    foreach (Transform child in _playerListContent)
    ////    for (int i = 0; i < players.Count(); i++)
    ////    {
    ////        Debug.Log(players[i].NickName);
    ////        GameObject plt = Instantiate(_playerListItemPrefab, _playerListContent);
    ////        plt.GetComponent<PlayerListItem>().SetUp(players[i]);
    ////        plt.GetComponent<PlayerListItem>().levelText.text = pda.playerBasicOnlineStats.level.ToString();
    ////    }
    ////}
    //        plt.GetComponent<PlayerListItem>().SetUp(players[i]);
    //        plt.GetComponent<PlayerListItem>().levelText.text = pda.playerBasicOnlineStats.level.ToString();
    //    }
    //}

    public void UpdateNickname() // Deprecated. Used to be used when changing username with a text field and button
    {
        //PhotonNetwork.NickName = nicknameInputField.text;
        FindObjectOfType<MainMenuCaller>().GetComponent<PhotonView>().RPC("UpdatePlayerList", RpcTarget.All);
    }

    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        startGameButton.SetActive(PhotonNetwork.IsMasterClient);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        errorText.text = "Room Creation Failed: " + message;
        Debug.LogError("Room Creation Failed: " + message);
        MenuManager.Instance.OpenMenu("error");
    }

    public void ShowPlayerMessage(string message)
    {
        messageText.text = message;
        MenuManager.Instance.OpenMenu("info");
    }

    public void StartGame()
    {
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;

        startGameButton.SetActive(false);
        PhotonNetwork.LoadLevel(levelToLoadIndex);
    }

    public void LeaveRoom()
    {
        commonRoomTexts.SetActive(false);
        PhotonNetwork.LeaveRoom();
        //MenuManager.Instance.OpenMenu("loading");
    }

    public void JoinRoom(RoomInfo info)
    {
        PhotonNetwork.JoinRoom(info.Name);
        MenuManager.Instance.OpenLoadingMenu("Joining Room...");
    }

    public override void OnLeftRoom()
    {
        MenuManager.Instance.OpenMenu("offline title");
    }

    private Dictionary<string, RoomInfo> cachedRoomList = new Dictionary<string, RoomInfo>();

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        UpdateCachedRoomList(roomList);

        foreach (Transform trans in roomListContent)
        {
            Destroy(trans.gameObject);
        }

        for (int i = 0; i < roomList.Count; i++)
        {
            if (roomList[i].RemovedFromList)
                continue;
            if (roomList[i].Name != quickMatchRoomName)
                Instantiate(roomListItemPrefab, roomListContent).GetComponent<RoomListItem>().SetUp(roomList[i]);
        }
    }

    private void UpdateCachedRoomList(List<RoomInfo> roomList)
    {
        for (int i = 0; i < roomList.Count; i++)
        {
            RoomInfo info = roomList[i];
            if (info.RemovedFromList)
            {
                cachedRoomList.Remove(info.Name);
            }
            else
            {
                cachedRoomList[info.Name] = info;
            }
            Debug.Log($"UpdateCachedRoomList: {info.Name}");
        }
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        Debug.Log("OnPlayerEnteredRoom");
        Instantiate(_playerListItemPrefab, _playerListContent).GetComponent<PlayerListItem>().SetUp(newPlayer);

        if (PhotonNetwork.IsMasterClient)
        {
            Dictionary<string, string> roomParams = new Dictionary<string, string>();
            roomParams["gamemode"] = GameManager.instance.gameMode.ToString();
            roomParams["gametype"] = GameManager.instance.gameType.ToString();

            FindObjectOfType<MainMenuCaller>().UpdateRoomSettings(roomParams);
        }
    }

    public void ChangeLevelToLoadWithIndex(int index)
    {
        try
        {
            FindObjectOfType<MainMenuCaller>().GetComponent<PhotonView>().RPC("UpdateSelectedMap", RpcTarget.All, index);
        }
        catch { }
    }

    public void ChangeGameType(string gt)
    {
        FindObjectOfType<MainMenuCaller>().GetComponent<PhotonView>().RPC("ChangeSubGameType_RPC", RpcTarget.All, gt);
    }

    //[PunRPC]
    //public void UpdateSelectedMap(int index)
    //{
    //    levelToLoadIndex = index;
    //    string mode = PhotonNetwork.CurrentRoom.CustomProperties["gamemode"].ToString();

    //    if (mode == "multiplayer")
    //        _mapSelectedText.text = $"Map: {NameFromIndex(index).Replace("PVP - ", "")}";
    //    if (mode == "swarm")
    //        _mapSelectedText.text = $"Map: {NameFromIndex(index).Replace("Coop - ", "")}";

    //}


    // By JimmyCushnie
    // Reference: https://answers.unity.com/questions/1262342/how-to-get-scene-name-at-certain-buildindex.html
    public static string NameFromIndex(int BuildIndex)
    {
        string path = SceneUtility.GetScenePathByBuildIndex(BuildIndex);
        int slash = path.LastIndexOf('/');
        string name = path.Substring(slash + 1);
        int dot = name.LastIndexOf('.');
        return name.Substring(0, dot);
    }

    public void Register()
    {
        WebManager.webManagerInstance.Register(registerUsernameText.text, registerPasswordText.text);
    }

    public void Login()
    {
        WebManager.webManagerInstance.Login(loginUsernameText.text, _loginPasswordText.text);

        Debug.Log(PhotonNetwork.NetworkClientState);
        //if (PhotonNetwork.NetworkClientState == ClientState.Disconnected)
        //    ConnectToPhotonMasterServer();

        MenuManager.Instance.OpenLoadingMenu();
        //loginButton.SetActive(false);
    }

    void OnSceneLoaded()
    {
        Debug.Log($"PhotonNetwork.NetworkClientState: {PhotonNetwork.NetworkClientState}");
        Scene currentScene = SceneManager.GetActiveScene();

        if (currentScene.buildIndex == 0)
        {
            if (PhotonNetwork.NetworkClientState == ClientState.ConnectedToMasterServer)
            {
                // Can Join Room
                //PhotonNetwork.JoinRandomRoom();
            }
            else
            {
                //Debug.LogWarning("Can't join random room now, client is not ready");
            }
        }

        if (currentScene.buildIndex > 0) // We are not in the menu
        {
            //commonRoomTexts.gameObject.SetActive(false);
        }
    }
}
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
    public int levelToLoadIndex
    {
        get { return _levelToLoadIndex; }
        set
        {
            _levelToLoadIndex = value;
            mapSelectedText.text = $"Map: {Launcher.NameFromIndex(_levelToLoadIndex).Replace("PVP - ", "")}";
        }
    }
    int _levelToLoadIndex;
    [SerializeField] int testingRoomLevelIndex;
    public int waitingRoomLevelIndex;
    #endregion

    // SerializeField makes private variables visible in the inspector
    [SerializeField] TMP_InputField roomNameInputField;
    [SerializeField] TMP_InputField nicknameInputField;
    [SerializeField] TMP_Text errorText;
    [SerializeField] TMP_Text messageText;
    [SerializeField] GameObject commonRoomTexts;
    [SerializeField] GameObject _teamRoomUI;
    [SerializeField] TMP_Text roomNameText;
    [SerializeField] Transform roomListContent;
    [SerializeField] GameObject roomListItemPrefab;
    [SerializeField] Transform _playerListContent;
    [SerializeField] GameObject _playerListItemPrefab;
    [SerializeField] TMP_Text _mapSelectedText;
    [SerializeField] TMP_Text _gametypeSelectedText;
    [SerializeField] TMP_Text _gameModeSelectedText;
    [SerializeField] TMP_Text _teamModeText;
    [SerializeField] TMP_Text _teamText;
    [SerializeField] GameObject _teamModeBtns;
    [SerializeField] GameObject _swarmDifficultyBtns;

    [SerializeField] TMP_InputField _loginUsernameText;
    [SerializeField] TMP_InputField registerUsernameText;
    [SerializeField] TMP_InputField _loginPasswordText;
    [SerializeField] TMP_InputField registerPasswordText;

    [Header("Master Client Only")]
    [SerializeField] GameObject startGameButton;
    [SerializeField] GameObject _multiplayerMcComponentsHolder;
    [SerializeField] GameObject _swarmMcComponentsHolder;

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

    public TMP_Text gameModeText { get { return _gameModeSelectedText; } }
    public TMP_Text teamModeText { get { return _teamModeText; } }
    public TMP_Text teamText { get { return _teamText; } }
    public GameObject teamRoomUI { get { return _teamRoomUI; } }

    public GameObject teamModeBtns { get { return _teamModeBtns; } }
    public GameObject swarmModeBtns { get { return _swarmDifficultyBtns; } }
    public GameObject multiplayerMcComponentsHolder { get { return _multiplayerMcComponentsHolder; } }
    public GameObject swarmMcComponentsHolder { get { return _swarmMcComponentsHolder; } }



    List<Photon.Realtime.Player> _previousListOfPlayersInRoom = new List<Photon.Realtime.Player>();


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
        foreach (var kvp in PhotonNetwork.CurrentRoom.Players) { Debug.Log($"Key: {kvp.Key}, Value: {kvp.Value}"); }

        List<Photon.Realtime.Player> newListPlayers = PhotonNetwork.CurrentRoom.Players.Values.ToList();

        var listPlayersDiff = newListPlayers.Except(_previousListOfPlayersInRoom).ToList();
        Debug.Log($"{listPlayersDiff[0].NickName} Joined room");

        {
            //if (PhotonNetwork.CurrentRoom.Name == quickMatchRoomName)
            //{ // Room is Random
            //    GameManager.instance.gameMode = GameManager.GameMode.Multiplayer;
            //    GameManager.instance.gameType = GameManager.GameType.Fiesta;
            //    PhotonNetwork.LoadLevel(waitingRoomLevelIndex);
            //}
            //else
            //{ // Room is private

            //    if (PhotonNetwork.IsMasterClient)
            //    {
            //        //PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs/UI", "NetworkMainMenu"), Vector3.zero, Quaternion.identity);
            //        PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs/Managers", "NetworkGameManager"), Vector3.zero, Quaternion.identity);
            //    }
            //    string roomType = PhotonNetwork.CurrentRoom.CustomProperties["gamemode"].ToString().ToLower() + "_room";
            //    string mode = PhotonNetwork.CurrentRoom.CustomProperties["gamemode"].ToString().ToLower();

            //    commonRoomTexts.SetActive(true);
            //    MenuManager.Instance.OpenMenu(roomType); // Show the "room" menu
            //    roomNameText.text = PhotonNetwork.CurrentRoom.Name; // Change the name of the room to the one given 

            //    //Debug.Log($"Is Master Client: {FindObjectOfType<MainMenuCaller>().GetComponent<PhotonView>().ViewID} and Master Client: {PhotonNetwork.IsMasterClient}");
            //    startGameButton.SetActive(PhotonNetwork.IsMasterClient);
            //}

            //if (!PhotonNetwork.IsMasterClient)
            //{

            //    try
            //    {
            //        Debug.Log($"OnPlayerEnteredRoom: {PhotonNetwork.CurrentRoom.CustomProperties["leveltoloadindex"]}");
            //        int ltl = (int)PhotonNetwork.CurrentRoom.CustomProperties["leveltoloadindex"];
            //        Launcher.instance.levelToLoadIndex = ltl;
            //        Launcher.instance.mapSelectedText.text = $"Map: {Launcher.NameFromIndex(ltl).Replace("PVP - ", "")}";
            //    }
            //    catch (System.Exception e) { Debug.Log(e); }

            //    try
            //    {
            //        Debug.Log($"OnPlayerEnteredRoom: {PhotonNetwork.CurrentRoom.CustomProperties["gametype"]}");
            //        int ei = (int)PhotonNetwork.CurrentRoom.CustomProperties["gametype"];
            //        GameManager.instance.gameType = (GameManager.GameType)ei;

            //        Launcher.instance.gametypeSelectedText.text = $"Gametype: {GameManager.instance.gameType}";
            //    }
            //    catch (System.Exception e) { Debug.Log(e); }
            //}
        }






        if (PhotonNetwork.CurrentRoom.Name == quickMatchRoomName)
        { // Room is Random
            GameManager.instance.gameMode = GameManager.GameMode.Multiplayer;
            GameManager.instance.gameType = GameManager.GameType.Fiesta;
            PhotonNetwork.LoadLevel(waitingRoomLevelIndex);
        }
        else
        { // Room is private

            commonRoomTexts.SetActive(true);
            MenuManager.Instance.OpenMenu("multiplayer_room"); // Show the "room" menu
            roomNameText.text = PhotonNetwork.CurrentRoom.Name; // Change the name of the room to the one given 

            try { Destroy(FindObjectOfType<NetworkGameManager>().gameObject); } catch { }

            if (PhotonNetwork.IsMasterClient)
            {
                if (listPlayersDiff[0].NickName.Contains(GameManager.instance.rootPlayerNickname))
                    GameManager.instance.gameMode = GameManager.GameMode.Multiplayer;

                StartCoroutine(InstantiateNetworkGameManager_Coroutine());
                startGameButton.SetActive(PhotonNetwork.IsMasterClient);

                //NetworkGameManager.instance.SendGameParams();
            }
        }
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        try
        {
            //int ei = (int)PhotonNetwork.CurrentRoom.CustomProperties["gamemode"];
            //GameManager.instance.gameMode = (GameManager.GameMode)ei;

            //Launcher.instance._gameModeSelectedText.text = $"Game Mode: {GameManager.instance.gameMode}";

            //if (GameManager.instance.gameMode == GameManager.GameMode.Multiplayer)
            //    multiplayerMapSelector.SetActive(PhotonNetwork.IsMasterClient);
            //else if (GameManager.instance.gameMode == GameManager.GameMode.Swarm)
            //{
            //    swarmMapSelector.SetActive(PhotonNetwork.IsMasterClient);
            //    GameManager.instance.teamMode = GameManager.TeamMode.Classic;
            //}
        }
        catch (System.Exception e) { Debug.Log(e); }

        try
        {
            //Debug.Log($"OnPlayerEnteredRoom: {PhotonNetwork.CurrentRoom.CustomProperties["leveltoloadindex"]}");
            //int ltl = (int)PhotonNetwork.CurrentRoom.CustomProperties["leveltoloadindex"];
            //Launcher.instance.levelToLoadIndex = ltl;
            //Launcher.instance.mapSelectedText.text = $"Map: {Launcher.NameFromIndex(ltl).Replace("PVP - ", "")}";
        }
        catch (System.Exception e) { Debug.Log(e); }

        try
        {
            //Debug.Log($"OnPlayerEnteredRoom: {PhotonNetwork.CurrentRoom.CustomProperties["gametype"]}");
            //int ei = (int)PhotonNetwork.CurrentRoom.CustomProperties["gametype"];
            //GameManager.instance.gameType = (GameManager.GameType)ei;

            //Launcher.instance.gametypeSelectedText.text = $"Gametype: {GameManager.instance.gameType}";
        }
        catch (System.Exception e) { Debug.Log(e); }

        try
        {
            //Debug.Log($"OnPlayerEnteredRoom: {PhotonNetwork.CurrentRoom.CustomProperties["teammode"]}");
            //int ei = (int)PhotonNetwork.CurrentRoom.CustomProperties["teammode"];
            //GameManager.instance.teamMode = (GameManager.TeamMode)ei;

            UpdateTeams();
        }
        catch (System.Exception e) { Debug.Log(e); }
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
        FindObjectOfType<NetworkMainMenu>().GetComponent<PhotonView>().RPC("UpdatePlayerList", RpcTarget.All);
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

        //PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs/Managers", "NetworkGameManager"), Vector3.zero, Quaternion.identity);
        StartCoroutine(LoadLevel_Coroutine());
    }

    public void LeaveRoom()
    {
        commonRoomTexts.SetActive(false);
        PhotonNetwork.LeaveRoom();
        GameManager.instance.gameMode = GameManager.GameMode.Multiplayer;
        //MenuManager.Instance.OpenMenu("loading");
    }

    public void JoinRoom(RoomInfo info)
    {
        PhotonNetwork.JoinRoom(info.Name);
        MenuManager.Instance.OpenLoadingMenu("Joining Room...");
    }

    public override void OnLeftRoom()
    {
        try
        {
            Destroy(FindObjectOfType<NetworkGameManager>().gameObject);
        }
        catch { }
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

            //FindObjectOfType<NetworkMainMenu>().UpdateRoomSettings(roomParams);
        }
        else
        {

        }
    }

    public void ChangeLevelToLoadWithIndex(int index)
    {
        Launcher.instance.levelToLoadIndex = index;
        NetworkGameManager.instance.SendGameParams();
        return;


        Launcher.instance.levelToLoadIndex = index;
        string mode = PhotonNetwork.CurrentRoom.CustomProperties["gamemode"].ToString();

        try
        {
            if (PhotonNetwork.IsMasterClient)
            {
                ExitGames.Client.Photon.Hashtable props = PhotonNetwork.CurrentRoom.CustomProperties;
                //ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
                try { props.Add("leveltoloadindex", index); } catch { props["leveltoloadindex"] = index; }
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

        //try
        //{
        //    FindObjectOfType<NetworkMainMenu>().GetComponent<PhotonView>().RPC("UpdateSelectedMap", RpcTarget.All, index);
        //}
        //catch { }
    }

    public void ChangeTeamMode(string tm)
    {
        GameManager.instance.teamMode = (GameManager.TeamMode)System.Enum.Parse(typeof(GameManager.TeamMode), tm);

        try
        {
            if (PhotonNetwork.IsMasterClient)
            {
                ExitGames.Client.Photon.Hashtable props = PhotonNetwork.CurrentRoom.CustomProperties;
                //ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
                try { props.Add("teammode", (int)GameManager.instance.teamMode); } catch { props["teammode"] = (int)GameManager.instance.teamMode; }
                PhotonNetwork.CurrentRoom.SetCustomProperties(props);
                Debug.Log($"ChangeTeamMode: {PhotonNetwork.CurrentRoom.CustomProperties["teammode"]}");
            }
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
        }


        //PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs/Managers", "NetworkGameManager"), Vector3.zero, Quaternion.identity);
        //FindObjectOfType<NetworkGameManager>().GetComponent<PhotonView>().RPC("UpdateTeamMode_RPC", RpcTarget.All, tm);
        UpdateTeams();
    }

    public void ChangeTeam()
    {
        try
        {
            if (GameManager.instance.onlineTeam == PlayerMultiplayerMatchStats.Team.Blue)
                GameManager.instance.onlineTeam = PlayerMultiplayerMatchStats.Team.Red;
            else if (GameManager.instance.onlineTeam == PlayerMultiplayerMatchStats.Team.Red)
                GameManager.instance.onlineTeam = PlayerMultiplayerMatchStats.Team.Blue;
        }
        catch { }
    }

    public void ChangeGameType(string gt)
    {

        GameManager.instance.gameType = (GameManager.GameType)System.Enum.Parse(typeof(GameManager.GameType), gt);
        FindObjectOfType<NetworkGameManager>().SendGameParams();
        return;

        instance.gametypeSelectedText.text = $"Gametype: {gt}";
        gt = gt.Replace(" ", "");
        GameManager.instance.gameType = (GameManager.GameType)System.Enum.Parse(typeof(GameManager.GameType), gt);

        Launcher.instance.gametypeSelectedText.text = $"Gametype: {gt}";
        gt = gt.Replace(" ", "");
        GameManager.instance.gameType = (GameManager.GameType)System.Enum.Parse(typeof(GameManager.GameType), gt);

        try
        {
            if (PhotonNetwork.IsMasterClient)
            {
                ExitGames.Client.Photon.Hashtable props = PhotonNetwork.CurrentRoom.CustomProperties;
                //ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
                try { props.Add("gametype", (int)GameManager.instance.gameType); } catch { props["gametype"] = (int)GameManager.instance.gameType; }
                PhotonNetwork.CurrentRoom.SetCustomProperties(props);
                Debug.Log($"UpdateSelectedMap: {PhotonNetwork.CurrentRoom.CustomProperties["gametype"]}");
            }
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
        }

        //FindObjectOfType<NetworkMainMenu>().GetComponent<PhotonView>().RPC("ChangeSubGameType_RPC", RpcTarget.All, gt);
    }

    public void ChangeGameMode(string gt)
    {
        GameManager.instance.gameMode = (GameManager.GameMode)System.Enum.Parse(typeof(GameManager.GameMode), gt);
        FindObjectOfType<NetworkGameManager>().SendGameParams();
        //GameManager.instance.gameMode = (GameManager.GameMode)System.Enum.Parse(typeof(GameManager.GameMode), gm);

        //try
        //{
        //    if (PhotonNetwork.IsMasterClient)
        //    {
        //        ExitGames.Client.Photon.Hashtable props = PhotonNetwork.CurrentRoom.CustomProperties;
        //        //ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
        //        try { props.Add("gamemode", (int)GameManager.instance.gameMode); } catch { props["gamemode"] = (int)GameManager.instance.gameMode; }
        //        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        //    }
        //}
        //catch (System.Exception e)
        //{
        //    Debug.Log(e);
        //}
    }

    void UpdateTeams()
    {
        foreach (KeyValuePair<int, Photon.Realtime.Player> kvp in PhotonNetwork.CurrentRoom.Players)
        {
            if (GameManager.instance.teamMode == GameManager.TeamMode.Classic)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    PlayerMultiplayerMatchStats.Team t = PlayerMultiplayerMatchStats.Team.Red;

                    if (kvp.Key % 2 == 0)
                        t = PlayerMultiplayerMatchStats.Team.Blue;

                    NetworkGameManager.instance.UpdateTeam(t.ToString(), kvp.Value.NickName);
                }
            }
        }
    }

    public void ChangeSwarmDifficulty(int enumI)
    {
        NetworkGameManager.instance.UpdateSwarmDifficulty(enumI);

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





    IEnumerator InstantiateNetworkGameManager_Coroutine()
    {
        yield return new WaitForEndOfFrame();
        PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs/Managers", "NetworkGameManager"), Vector3.zero, Quaternion.identity);
        NetworkGameManager.instance.SendGameParams();

    }

    IEnumerator LoadLevel_Coroutine()
    {
        yield return new WaitForSeconds(0.5f);
        PhotonNetwork.LoadLevel(levelToLoadIndex);
    }
}
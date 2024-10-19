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
using Steamworks;
using Rewired;
using static GameManager;

public class Launcher : MonoBehaviourPunCallbacks
{
    private TypedLobby typedLobby = new TypedLobby("NelsonLobby", LobbyType.Default);

    // Events
    public delegate void LauncherEvent(Launcher launcher);
    public LauncherEvent OnCreateSwarmRoomButton;
    public LauncherEvent OnCreateMultiplayerRoomButton;

    public static Launcher instance; // Singleton of the Photon Launcher
    public static int DEFAULT_ROOM_COUNTDOWN = 7;
    public PhotonView PV;
    public GameObject loginButton;
    public GameObject playerModel { get { return _playerModel; } }

    #region
    public int levelToLoadIndex
    {
        get { return _levelToLoadIndex; }
        set
        {
            _levelToLoadIndex = value;
            _mapSelectedPreview.gameObject.SetActive(!PhotonNetwork.IsMasterClient);
            _mapSelectedPreview.gameObject.SetActive((CurrentRoomManager.instance.roomType == CurrentRoomManager.RoomType.QuickMatch) ||
                (!PhotonNetwork.IsMasterClient && CurrentRoomManager.instance.roomType == CurrentRoomManager.RoomType.Private));
            mapSelectedText.text = $"Map: {GameManager.instance.mapDataCells.Where(obj => obj.sceneBuildIndex.Equals(value)).SingleOrDefault().mapName}";
            _mapSelectedPreview.sprite = GameManager.instance.mapDataCells.Where(obj => obj.sceneBuildIndex.Equals(value)).SingleOrDefault().image;
            //mapSelectedText.text = $"Map: {Launcher.NameFromIndex(_levelToLoadIndex).Replace("PVP - ", "")}";

            Debug.Log($"levelToLoadIndex {GameManager.instance.mapDataCells.Where(obj => obj.sceneBuildIndex.Equals(value)).SingleOrDefault().mapName}");
        }
    }



    [SerializeField] GameObject _playerModel;


    [SerializeField] int _levelToLoadIndex;
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
    [SerializeField] TMP_Text _sprintModeText, _hitMarkersMode;
    [SerializeField] TMP_Text roomNameText;
    [SerializeField] Transform roomListContent;
    [SerializeField] GameObject roomListItemPrefab;
    [SerializeField] Transform _namePlatesParent;
    [SerializeField] GameObject _namePlatePrefab;
    [SerializeField] TMP_Text _mapSelectedText;
    [SerializeField] Image _mapSelectedPreview;
    [SerializeField] TMP_Text _gametypeSelectedText;
    [SerializeField] TMP_Text _gameModeSelectedText;
    [SerializeField] TMP_Text _teamModeText;
    [SerializeField] TMP_Text _teamText, _difficultyText;
    [SerializeField] GameObject _teamModeBtns;
    [SerializeField] GameObject _swarmDifficultyBtns;
    [SerializeField] GameObject _vetoBtn;
    [SerializeField] TMP_Text _matchStartCountdownText;

    [SerializeField] TMP_InputField _loginUsernameText;
    [SerializeField] TMP_InputField registerUsernameText;
    [SerializeField] TMP_InputField _loginPasswordText;
    [SerializeField] TMP_InputField registerPasswordText;

    [Header("Master Client Only")]
    [SerializeField] GameObject _startGameButton;
    [SerializeField] GameObject _gameModeBtns;
    [SerializeField] GameObject _multiplayerMcComponentsHolder;
    [SerializeField] GameObject _swarmMcComponentsHolder;

    public static string quickMatchRoomName = "quick_match_room";


    [SerializeField] GameObject _nbLocalPlayersHolder;
    [SerializeField] TMP_InputField _nbLocalPlayersInputed;





    public MenuGamePadCursor menuGamePadCursorScript;
    public TMP_Text errorMenuText { get { return errorText; } }














    public TMP_InputField loginUsernameText
    {
        get { return _loginUsernameText; }
    }

    public TMP_InputField loginPasswordText
    {
        get { return _loginPasswordText; }
    }

    public Transform namePlatesParent
    {
        get { return _namePlatesParent; }
    }

    public GameObject namePlatePrefab
    {
        get { return _namePlatePrefab; }
    }

    public TMP_Text gameModeText { get { return _gameModeSelectedText; } }
    public TMP_Text teamModeText { get { return _teamModeText; } }
    public TMP_Text teamText { get { return _teamText; } }
    public TMP_Text difficultyText { get { return _difficultyText; } }
    public GameObject teamRoomUI { get { return _teamRoomUI; } }
    public TMP_Text sprintModeText { get { return _sprintModeText; } }
    public TMP_Text hitMarkersModeText { get { return _hitMarkersMode; } }

    public GameObject gameModeBtns { get { return _gameModeBtns; } }
    public GameObject teamModeBtns { get { return _teamModeBtns; } }
    public GameObject swarmModeBtns { get { return _swarmDifficultyBtns; } }
    public GameObject multiplayerMcComponentsHolder { get { return _multiplayerMcComponentsHolder; } }
    public GameObject swarmMcComponentsHolder { get { return _swarmMcComponentsHolder; } }
    public TMP_Text gameCountdownText { get { return _matchStartCountdownText; } }
    public GameObject vetoBtn { get { return _vetoBtn; } }
    public GameObject nbLocalPlayersHolder { get { return _nbLocalPlayersHolder; } }

    List<Photon.Realtime.Player> _previousListOfPlayersInRoom = new List<Photon.Realtime.Player>();



    bool _tryingToLeaveRoomFromMenu;

    List<RoomInfo> _roomsCached = new List<RoomInfo>();











    public TMP_Text mapSelectedText
    {
        get { return _mapSelectedText; }
    }

    public TMP_Text gametypeSelectedText
    {
        get { return _gametypeSelectedText; }
    }

    public TMP_InputField nbLocalPlayersText { get { return _nbLocalPlayersInputed; } }


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

        FindObjectOfType<GameManager>().OnGameManagerFinishedLoadingScene_Late -= OnSceneLoaded;
        FindObjectOfType<GameManager>().OnGameManagerFinishedLoadingScene_Late += OnSceneLoaded;
    }

    void Start()
    {
        //if (Application.platform == RuntimePlatform.WindowsPlayer)
        Debug.Log($"You are playing on a {Application.platform}");

        if (levelToLoadIndex == 0)
            levelToLoadIndex = 1;

        //TODO: PhotonNetwork.OfflineMode = true;
        //ConnectToPhotonMasterServer();
        //GetComponent<MenuManager>().OpenMainMenu();
    }



    [SerializeField] float _masterClientIconCheck;
    private void Update()
    {
        if (_masterClientIconCheck > 0)
        {
            _masterClientIconCheck -= Time.deltaTime;

            if (_masterClientIconCheck < 0)
            {

                if (SceneManager.GetActiveScene().buildIndex == 0 && PhotonNetwork.InRoom)
                {
                    FindMasterClientAndToggleIcon();
                }
                _masterClientIconCheck = 0.6f;
            }
        }
    }

    public void ConnectToPhotonMasterServer(bool showMessage = true)
    {
        Debug.Log($"ConnectToPhotonMasterServer");
        //GetComponent<MenuManager>().OpenLoadingMenu("Connecting To Server...");

        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        Debug.Log($"LAUNCHER Disconnected: {cause}");
        //ShowPlayerMessage($"Disconnected from Server: {cause}.\nReconnecting...");
        _tries++;

        //if (_tries < 4) StartCoroutine(TryToConnectAgain());
        //else
        //{
        //    print("YOU ARE PROLLY NOT CONNECTED TO THE INTERNET");
        //    //CreateLocalModePlayerDataCells();
        //    //MenuManager.Instance.OpenMenu("online title");
        //    //_nbLocalPlayersHolder.SetActive(true);
        //}
    }
    int _tries = 0;

    IEnumerator TryToConnectAgain()
    {
        yield return new WaitForSeconds(1);
        ConnectToPhotonMasterServer(false);

    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master");
        try { ChangeLevelToLoadWithIndex(levelToLoadIndex); } catch { }
        //ShowPlayerMessage("Conneected To Master Server!");
        if (!PhotonNetwork.OfflineMode)
            PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true;
        //GetComponent<MenuManager>().OpenMainMenu();

    }

    public override void OnJoinedLobby()
    {
        print("LAUNCHER OnJoinedLobby");

        Scene currentScene = SceneManager.GetActiveScene();

        if (currentScene.buildIndex > 0) // We are not in the menu
        {
            PhotonNetwork.LoadLevel(0);
        }

        //if (WebManager.webManagerInstance)
        //{
        //    if (WebManager.webManagerInstance.pda.PlayerDataIsSet())
        //    {
        //        Debug.Log(GameManager.instance.carnageReport.xpGained);
        //        if (GameManager.instance.carnageReport == null)
        //            Debug.Log("Carnage report is NULL");

        //        if (GameManager.instance.carnageReport.xpGained <= 0)
        //        {
        //            //MenuManager.Instance.OpenMenu("online title"); // Runs this line if quit game an returning to menu
        //        }
        //        //else
        //        MenuManager.Instance.OpenMenu("carnage report");
        //    }
        //    else
        //    {

        //        //MenuManager.Instance.OpenMenu("offline title");
        //    }
        //}
        //else
        //{
        //    MenuManager.Instance.OpenMenu("offline title");
        //}
        Debug.Log("Joined Lobby");
        if (PhotonNetwork.NickName == "")
            PhotonNetwork.NickName = "Player " + Random.Range(0, 1000).ToString("0000");
    }

    public void QuickMatch()
    {
        Debug.Log("Click QuickMatch");
        CurrentRoomManager.instance.roomType = CurrentRoomManager.RoomType.QuickMatch;
        CurrentRoomManager.instance.vetos = 0;

        string roomName = quickMatchRoomName;
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = maxRandomRoomPlayers;
        roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable();
        //roomOptions.CustomRoomProperties.Add("gamemode", GameManager.GameMode.Multiplayer.ToString());
        //roomOptions.CustomRoomProperties.Add("gametype", GameManager.GameType.Fiesta.ToString());
        //PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, typedLobby);
        CreateRoom(roomName, roomOptions, typedLobby);
        //PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log($"OnJoinRandomFailed: {message}");
        //CreateAndJoinRandomRoom();
    }

    [SerializeField]
    private byte maxRandomRoomPlayers = 6;

    public void CreatePrivateRoom()
    {
        if (!RoomBrowserMenu.GAMEPAD_ROOM_NAMES.Contains(roomNameInputField.text) && !RoomBrowserMenu.FORBIDDEN_ROOM_NAMES.Contains(roomNameInputField.text))
        {
            Debug.Log($"CreateMultiplayerRoom. Client State: {PhotonNetwork.NetworkClientState}");

            CurrentRoomManager.instance.roomType = CurrentRoomManager.RoomType.Private;
            GameManager.instance.teamMode = GameManager.TeamMode.None;
            GameManager.instance.gameMode = GameManager.GameMode.Versus;
            GameManager.instance.sprintMode = GameManager.SprintMode.On;
            GameManager.instance.hitMarkersMode = HitMarkersMode.On;
            GameManager.instance.difficulty = SwarmManager.Difficulty.Normal;

            RoomOptions options = new RoomOptions();
            options.CustomRoomPropertiesForLobby = new string[1] { "gamemode" };
            options.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable();
            options.CustomRoomProperties.Add("gamemode", "multiplayer");


            if (GameManager.instance.connection == GameManager.Connection.Online && PhotonNetwork.NetworkClientState != ClientState.Disconnected)
            {


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
                //PhotonNetwork.CreateRoom(roomNameInputField.text, options); // Create a room with the text in parameter
                CreateRoom(roomNameInputField.text, options);
                MenuManager.Instance.OpenLoadingMenu("Creating Multiplayer Room..."); // Show the loading menu/message

                // When creating a room is done, OnJoinedRoom() will automatically trigger
                OnCreateMultiplayerRoomButton?.Invoke(this);
            }
            else if (GameManager.instance.connection == GameManager.Connection.Local)
            {
                PhotonNetwork.OfflineMode = true; PhotonNetwork.NickName = "0";
                CreateRoom(roomNameInputField.text, options);

                //MenuManager.Instance.OpenMenu("multiplayer_room");
                //commonRoomTexts.SetActive(true);

                ////if (PhotonNetwork.CurrentRoom.Name != quickMatchRoomName)
                //{
                //    roomNameText.text = "LOCAL"; // Change the name of the room to the one given 
                //    _vetoBtn.SetActive(false); _matchStartCountdownText.gameObject.SetActive(false);
                //}

                //CurrentRoomManager.instance.roomType = CurrentRoomManager.RoomType.Private;
                //_startGameButton.SetActive(true);

                //{
                //    PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs/Managers", "NetworkGameManager"), Vector3.zero, Quaternion.identity);

                //    //if (CurrentRoomManager.instance.playerNicknameNbLocalPlayersDict.ContainsKey(PhotonNetwork.NickName))
                //    //    CurrentRoomManager.instance.playerNicknameNbLocalPlayersDict[PhotonNetwork.NickName] = GameManager.instance.nbLocalPlayersPreset;
                //    //else
                //    //    CurrentRoomManager.instance.playerNicknameNbLocalPlayersDict.Add(PhotonNetwork.NickName, GameManager.instance.nbLocalPlayersPreset);
                //    //CurrentRoomManager.instance.playerNicknameNbLocalPlayersDict = CurrentRoomManager.instance.playerNicknameNbLocalPlayersDict;

                //    //NetworkGameManager.instance.SendGameParams();
                //}
            }
            Debug.Log($"CreateMultiplayerRoom. Client State: {PhotonNetwork.NetworkClientState}");
        }
        else
        {
            errorText.text = "You cannot use that name";
            MenuManager.Instance.OpenPopUpMenu("error");
        }
    }




    public void CreatePrivateRoomFromGamepad()
    {
        if (GameManager.instance.connection == GameManager.Connection.Local)
        {
            CurrentRoomManager.instance.ResetAllPlayerDataExceptMine();
            Launcher.CreateLocalModePlayerDataCells();
        }


        string roomNameToUse = RoomBrowserMenu.GAMEPAD_ROOM_NAMES[0];

        for (int i = 0; i < RoomBrowserMenu.GAMEPAD_ROOM_NAMES.Length; i++)
        {
            if (_roomsCached.Find(j => j.Name.Equals(roomNameToUse)) != null)
            {
                roomNameToUse = "impossible";

                try { roomNameToUse = RoomBrowserMenu.GAMEPAD_ROOM_NAMES[i]; } catch { }
            }
        }

        if (!roomNameToUse.Equals("impossible"))
        {
            CurrentRoomManager.instance.roomType = CurrentRoomManager.RoomType.Private;
            GameManager.instance.teamMode = GameManager.TeamMode.None;
            GameManager.instance.gameMode = GameManager.GameMode.Versus;
            GameManager.instance.sprintMode = SprintMode.On;
            GameManager.instance.hitMarkersMode = HitMarkersMode.On;

            RoomOptions options = new RoomOptions();
            options.CustomRoomPropertiesForLobby = new string[1] { "gamemode" };
            options.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable();
            options.CustomRoomProperties.Add("gamemode", "multiplayer");


            if (GameManager.instance.connection == GameManager.Connection.Online && PhotonNetwork.NetworkClientState != ClientState.Disconnected)
            {
                //PhotonNetwork.CreateRoom(roomNameInputField.text, options); // Create a room with the text in parameter
                CreateRoom(roomNameToUse, options);
                MenuManager.Instance.OpenLoadingMenu("Creating Multiplayer Room..."); // Show the loading menu/message

                // When creating a room is done, OnJoinedRoom() will automatically trigger
                OnCreateMultiplayerRoomButton?.Invoke(this);
            }
            else if (GameManager.instance.connection == GameManager.Connection.Local)
            {
                PhotonNetwork.OfflineMode = true; PhotonNetwork.NickName = "0";
                CreateRoom(roomNameToUse, options);
            }
            Debug.Log($"CreateMultiplayerRoom. Client State: {PhotonNetwork.NetworkClientState}");
        }
        else
        {
            errorText.text = "All Gamepad Room Names have been used";
            MenuManager.Instance.OpenPopUpMenu("error");
        }
    }






    void CreateRoom(string roomNam, RoomOptions ro, TypedLobby tl = null)
    {

        ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable { { "username", GameManager.ROOT_PLAYER_NAME }, { "localPlayerCount", int.Parse(nbLocalPlayersText.text) } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(h);


        CurrentRoomManager.instance.playerNicknameNbLocalPlayersDict.Clear();
        CurrentRoomManager.instance.expectedNbPlayers = 0;
        CurrentRoomManager.instance.vetoCountdown = CurrentRoomManager.instance.roomGameStartCountdown = DEFAULT_ROOM_COUNTDOWN;

        if (tl == null)
            PhotonNetwork.CreateRoom(roomNam, ro);
        else
            PhotonNetwork.JoinOrCreateRoom(roomNam, ro, typedLobby);
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

    public override void OnJoinedRoom() // Runs only when My player joined the room
    {
        Debug.Log("Joined room");
        TriggerOnJoinedRoomBehaviour();

        //FindMasterClientAndToggleIcon();
    }

    public void TriggerOnJoinedRoomBehaviour()
    {
        if (PhotonNetwork.InRoom)
        {
            DestroyNameplates();
            CreateNameplates();

            { // Room is private

                commonRoomTexts.SetActive(true);
                MenuManager.Instance.OpenMenu("multiplayer_room"); // Show the "room" menu

                if (PhotonNetwork.CurrentRoom.Name != quickMatchRoomName)
                {
                    roomNameText.text = PhotonNetwork.CurrentRoom.Name; // Change the name of the room to the one given 
                    _vetoBtn.SetActive(false);
                    _matchStartCountdownText.text = "";
                }
                else
                {
                    CurrentRoomManager.instance.ResetRoomCountdowns();
                    _vetoBtn.SetActive(true);
                    _matchStartCountdownText.gameObject.SetActive(true);
                }


                if (PhotonNetwork.CurrentRoom.Name == quickMatchRoomName)
                    CurrentRoomManager.instance.roomType = CurrentRoomManager.RoomType.QuickMatch;
                else
                    CurrentRoomManager.instance.roomType = CurrentRoomManager.RoomType.Private;

                //try { Destroy(FindObjectOfType<NetworkGameManager>().gameObject); } catch { } finally { Debug.Log("Destroying NetworkGameManager"); }


                _startGameButton.SetActive(PhotonNetwork.IsMasterClient && CurrentRoomManager.instance.roomType == CurrentRoomManager.RoomType.Private);

                if (PhotonNetwork.IsMasterClient)
                {

                    Debug.Log("Joined room IsMasterClient");

                    //if (listPlayersDiff[0].NickName.Contains(GameManager.instance.rootPlayerNickname))
                    //    GameManager.instance.gameMode = GameManager.GameMode.Multiplayer;



                    if (CurrentRoomManager.instance.roomType == CurrentRoomManager.RoomType.QuickMatch)
                    {

                    }



                    PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs/Managers", "NetworkGameManager"), Vector3.zero, Quaternion.identity);

                    if (CurrentRoomManager.instance.playerNicknameNbLocalPlayersDict.ContainsKey(PhotonNetwork.NickName))
                        CurrentRoomManager.instance.playerNicknameNbLocalPlayersDict[PhotonNetwork.NickName] = GameManager.instance.nbLocalPlayersPreset;
                    else
                        CurrentRoomManager.instance.playerNicknameNbLocalPlayersDict.Add(PhotonNetwork.NickName, GameManager.instance.nbLocalPlayersPreset);
                    CurrentRoomManager.instance.playerNicknameNbLocalPlayersDict = CurrentRoomManager.instance.playerNicknameNbLocalPlayersDict;


                    NetworkGameManager.instance.SendGameParams();
                }
                //else
                {
                    StartCoroutine(SendLocalGameParamsToMasterClient_Coroutine());
                }
            }
        }
    }

    // Runs only when OTHER player joined room.
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        print("Other player joined room");
        Debug.Log("LAUNCHER OnPlayerEnteredRoom");
        Instantiate(_namePlatePrefab, _namePlatesParent).GetComponent<PlayerNamePlate>().SetUp(newPlayer);

        DestroyAndCreateNameplates();

        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(InstantiateNetworkGameManager_Coroutine());
            //StartCoroutine(SendGameParamsWithDelay());
        }
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {

    }

    IEnumerator SendLocalGameParamsToMasterClient_Coroutine() // Allow time for the NetworkGameManager instance to fully set
    {
        yield return new WaitForEndOfFrame();
        NetworkGameManager.instance.SendLocalPlayerDataToMasterClient();
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
        print($"OnMasterClientSwitched {newMasterClient.NickName} {PhotonNetwork.IsMasterClient}");

        GameManager.instance.gameMode = GameMode.Versus;
        GameManager.instance.teamMode = TeamMode.None;
        GameManager.instance.sprintMode = SprintMode.On;
        GameManager.instance.hitMarkersMode = HitMarkersMode.On;
        GameManager.instance.difficulty = SwarmManager.Difficulty.Normal;

        _startGameButton.SetActive(PhotonNetwork.IsMasterClient && CurrentRoomManager.instance.roomType == CurrentRoomManager.RoomType.Private);
        _mapSelectedPreview.gameObject.SetActive(!PhotonNetwork.IsMasterClient);

        //FindMasterClientAndToggleIcon();
        ChangeLevelToLoadWithIndex(1); // Will send params too
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        errorText.text = "Room Creation Failed: " + message;
        Debug.LogError("Room Creation Failed: " + message);
        MenuManager.Instance.OpenPopUpMenu("error");
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

        _startGameButton.SetActive(false);



        int c = 0;
        foreach (ScriptObjPlayerData s in CurrentRoomManager.instance.playerDataCells.Where(item => item.occupied))
        {
            print($"{s.playerExtendedPublicData.player_id} {s.rewiredId} will get spawn {c}");
            NetworkGameManager.instance.SetPlayerDataCellStartingSpawnPositionIndex(s.playerExtendedPublicData.player_id, s.rewiredId, c);
            c++;
        }



        NetworkGameManager.instance.StartGameButton();
    }

    public void LeaveRoomButton()
    {
        Debug.Log("LeaveRoomButton");
        try { commonRoomTexts.SetActive(false); } catch { }
        try { PhotonNetwork.LeaveRoom(); } catch (System.Exception e) { Debug.LogWarning(e); }
        try { GameManager.instance.gameMode = GameManager.GameMode.Versus; } catch { }
        try { GameManager.instance.teamMode = GameManager.TeamMode.None; } catch { }
        try { CurrentRoomManager.instance.roomType = CurrentRoomManager.RoomType.None; } catch { }

        CurrentRoomManager.instance.ResetAllPlayerDataExceptMine();

        //MenuManager.Instance.OpenMenu("loading");
        if (GameManager.instance.connection == GameManager.Connection.Online)
        {
            _tryingToLeaveRoomFromMenu = true;
            MenuManager.Instance.OpenLoadingMenu("Leaving Room...");
        }
        else
        {
            MenuManager.Instance.OpenMenu("online title");
        }

        CurrentRoomManager.instance.matchSettingsSet = false;
        Launcher.instance.gameCountdownText.gameObject.SetActive(false);
        CurrentRoomManager.instance.roomGameStartCountdown = DEFAULT_ROOM_COUNTDOWN;
        GameManager.StopBeeps();
    }

    public void JoinRoomPlateBtn(RoomInfo info)
    {
        ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable { { "username", GameManager.ROOT_PLAYER_NAME }, { "localPlayerCount", int.Parse(nbLocalPlayersText.text) } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(h);


        PhotonNetwork.JoinRoom(info.Name);
        MenuManager.Instance.OpenLoadingMenu("Joining Room...");
    }

    // Triggers when YOU left room

    public override void OnLeftRoom() // Is also called when quitting a game while connected to the internet. Does not trigger when offline
    {
        Debug.Log("LAUNCHER: OnLeftRoom");
        CurrentRoomManager.instance.playerNicknameNbLocalPlayersDict.Clear();
        CurrentRoomManager.instance.expectedNbPlayers = 0;
        try
        {
            Destroy(FindObjectOfType<NetworkGameManager>().gameObject);
        }
        catch (System.Exception e) { Debug.LogWarning(e); }

        if (_tryingToLeaveRoomFromMenu && GameManager.instance.connection == GameManager.Connection.Online)
        {
            _tryingToLeaveRoomFromMenu = false;
            MenuManager.Instance.OpenMenu("online title");
        }

        //MenuManager.Instance.OpenMenu("offline title");
    }

    // Triggers when other player left room
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        try
        {
            CurrentRoomManager.instance.playerNicknameNbLocalPlayersDict.Remove(otherPlayer.NickName);
            CurrentRoomManager.instance.playerNicknameNbLocalPlayersDict = CurrentRoomManager.instance.playerNicknameNbLocalPlayersDict;
        }
        catch (System.Exception e) { Debug.LogWarning(e); }

        if (CurrentRoomManager.instance.PlayerDataContains(int.Parse(otherPlayer.NickName)))
        {
            foreach (Transform child in instance.namePlatesParent)
                if (child.GetComponent<PlayerNamePlate>().playerDataCell.playerExtendedPublicData.player_id == int.Parse(otherPlayer.NickName))
                    Destroy(child.gameObject);

            CurrentRoomManager.instance.RemoveExtendedPlayerData(int.Parse(otherPlayer.NickName));
        }
    }

    private Dictionary<string, RoomInfo> cachedRoomList = new Dictionary<string, RoomInfo>();

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        _roomsCached.Clear();
        UpdateCachedRoomList(roomList);

        foreach (Transform trans in roomListContent)
        {
            Destroy(trans.gameObject);
        }

        for (int i = 0; i < roomList.Count; i++)
        {
            if (roomList[i].RemovedFromList)
                continue;

            _roomsCached.Add(roomList[i]);
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



    public void ChangeLevelToLoadWithIndex(int index)
    {
        if (CurrentRoomManager.instance.roomGameStartCountdown == Launcher.DEFAULT_ROOM_COUNTDOWN)
        {
            Launcher.instance.levelToLoadIndex = index;
            NetworkGameManager.instance.SendGameParams();
        }
    }

    public void ChangeGameType(string gt)
    {
        if (CurrentRoomManager.instance.roomGameStartCountdown == Launcher.DEFAULT_ROOM_COUNTDOWN)
        {
            if ((GameManager.GameType)System.Enum.Parse(typeof(GameManager.GameType), gt) == GameType.GunGame)
                if (GameManager.instance.teamMode != TeamMode.None)
                    GameManager.instance.teamMode = TeamMode.None;

            if ((GameManager.GameType)System.Enum.Parse(typeof(GameManager.GameType), gt) == GameType.CTF)
                if (GameManager.instance.teamMode != TeamMode.Classic)
                    GameManager.instance.teamMode = TeamMode.Classic;


            GameManager.instance.gameType = (GameManager.GameType)System.Enum.Parse(typeof(GameManager.GameType), gt);
            FindObjectOfType<NetworkGameManager>().SendGameParams();
        }
    }

    public void ChangeGameMode(string gt)
    {
        if (CurrentRoomManager.instance.roomGameStartCountdown == Launcher.DEFAULT_ROOM_COUNTDOWN)
        {
            GameManager.instance.gameMode = (GameManager.GameMode)System.Enum.Parse(typeof(GameManager.GameMode), gt);
            if (GameManager.instance.gameMode == GameManager.GameMode.Versus) GameManager.instance.teamMode = GameManager.TeamMode.None;
            if (GameManager.instance.gameMode == GameManager.GameMode.Coop) GameManager.instance.teamMode = GameManager.TeamMode.Classic;

            FindObjectOfType<NetworkGameManager>().SendGameParams();
        }
    }

    public void ChangeTeamMode(string tm)
    {
        Debug.Log("ChangeTeamMode Btn");

        if (CurrentRoomManager.instance.roomGameStartCountdown == Launcher.DEFAULT_ROOM_COUNTDOWN)
        {
            if ((GameManager.TeamMode)System.Enum.Parse(typeof(GameManager.TeamMode), tm) == TeamMode.Classic)
                if (GameManager.instance.gameType == GameType.GunGame)
                    GameManager.instance.gameType = GameType.Slayer;

            if ((GameManager.TeamMode)System.Enum.Parse(typeof(GameManager.TeamMode), tm) == TeamMode.None)
                if (GameManager.instance.gameType == GameType.CTF)
                    GameManager.instance.gameType = GameType.Slayer;



            GameManager.instance.teamMode = (GameManager.TeamMode)System.Enum.Parse(typeof(GameManager.TeamMode), tm);

            if (PhotonNetwork.IsMasterClient)
                NetworkGameManager.instance.SendGameParams();
        }
    }

    public void ChangeTeam()
    {
        if (CurrentRoomManager.instance.roomGameStartCountdown == Launcher.DEFAULT_ROOM_COUNTDOWN)
        {
            if (GameManager.instance.teamMode == GameManager.TeamMode.Classic)
            {
                GameManager.Team nt = CurrentRoomManager.instance.playerDataCells[0].team;

                if (nt == GameManager.Team.Blue)
                    nt = GameManager.Team.Red;
                else if (nt == GameManager.Team.Red)
                    nt = GameManager.Team.Blue;

                Dictionary<int, int> nd = new Dictionary<int, int>();

                nd[CurrentRoomManager.instance.playerDataCells[0].playerExtendedPublicData.player_id] = (int)nt;

                NetworkGameManager.instance.ChangePlayerTeam(nd);
            }
        }
    }

    public void ChangeSprintMode() //  called from a ui button
    {
        if (CurrentRoomManager.instance.roomGameStartCountdown == Launcher.DEFAULT_ROOM_COUNTDOWN)
        {
            if (GameManager.instance.sprintMode == SprintMode.On)
            {
                GameManager.instance.sprintMode = SprintMode.Off;
            }
            else
            {
                GameManager.instance.sprintMode = SprintMode.On;
            }

            if (PhotonNetwork.IsMasterClient)
                NetworkGameManager.instance.SendGameParams();
        }
    }

    public void ChangeHitMarkersMode() //  called from a ui button
    {
        if (CurrentRoomManager.instance.roomGameStartCountdown == Launcher.DEFAULT_ROOM_COUNTDOWN)
        {
            if (GameManager.instance.hitMarkersMode == HitMarkersMode.On)
            {
                GameManager.instance.hitMarkersMode = HitMarkersMode.Off;
            }
            else
            {
                GameManager.instance.hitMarkersMode = HitMarkersMode.On;
            }

            if (PhotonNetwork.IsMasterClient)
                NetworkGameManager.instance.SendGameParams();
        }
    }





    public void ChangeTeamOfLocalPlayer(int localPlayerInd)
    {
        if (CurrentRoomManager.instance.roomGameStartCountdown == Launcher.DEFAULT_ROOM_COUNTDOWN)
        {
            if (GameManager.instance.teamMode == GameManager.TeamMode.Classic)
            {
                GameManager.Team nt = CurrentRoomManager.instance.playerDataCells[localPlayerInd].team;

                if (nt == GameManager.Team.Blue)
                    nt = GameManager.Team.Red;
                else if (nt == GameManager.Team.Red)
                    nt = GameManager.Team.Blue;

                Dictionary<int, int> nd = new Dictionary<int, int>();

                nd[CurrentRoomManager.instance.playerDataCells[localPlayerInd].playerExtendedPublicData.player_id] = (int)nt;

                NetworkGameManager.instance.ChangePlayerTeam(nd);
            }
        }
    }









    public void SendVetoToMaster()
    {
        NetworkGameManager.instance.SendVetoToMaster(GameManager.instance.nbLocalPlayersPreset);
        _vetoBtn.SetActive(false);
    }

    public void ChangeSwarmDifficulty(int enumI)
    {
        if (CurrentRoomManager.instance.roomGameStartCountdown == Launcher.DEFAULT_ROOM_COUNTDOWN)
        {
            NetworkGameManager.instance.UpdateSwarmDifficulty(enumI);
        }
    }



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

    public void Login(string u = "")
    {
        if (u.Equals(""))
            GameManager.ROOT_PLAYER_NAME = loginUsernameText.text;
        else
            GameManager.ROOT_PLAYER_NAME = u;

        //WebManager.webManagerInstance.Login("0", loginUsernameText.text, _loginPasswordText.text);

        Debug.Log(PhotonNetwork.NetworkClientState);
        Debug.Log(PhotonNetwork.OfflineMode);
        //if (PhotonNetwork.NetworkClientState == ClientState.Disconnected)
        //    ConnectToPhotonMasterServer();

        MenuManager.Instance.OpenLoadingMenu();
        //loginButton.SetActive(false);
    }

    public void LoginWithSteamName()
    {
        WebManager.webManagerInstance.Login(SteamUser.GetSteamID().m_SteamID.ToString(), SteamFriends.GetPersonaName(), "steam");

        Debug.Log(PhotonNetwork.NetworkClientState);
        //if (PhotonNetwork.NetworkClientState == ClientState.Disconnected)
        //    ConnectToPhotonMasterServer();

        //MenuManager.Instance.OpenLoadingMenu();
        //loginButton.SetActive(false);
    }

    void OnSceneLoaded()
    {
        Debug.Log($"PhotonNetwork.NetworkClientState: {PhotonNetwork.NetworkClientState}");

        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            _masterClientIconCheck = 0.6f;
        }
        else
        {
            _masterClientIconCheck = -1;
        }
    }





    IEnumerator InstantiateNetworkGameManager_Coroutine()
    {
        Debug.Log("InstantiateNetworkGameManager_Coroutine");
        yield return new WaitForSeconds(0.1f);
        PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs/Managers", "NetworkGameManager"), Vector3.zero, Quaternion.identity);
        NetworkGameManager.instance.SendGameParams();
    }

    IEnumerator LoadLevel_Coroutine()
    {

        yield return new WaitForSeconds(0.5f);
        PhotonNetwork.LoadLevel(levelToLoadIndex);
    }

    public void DestroyAndCreateNameplates()
    {
        DestroyNameplates();
        CreateNameplates();
    }

    void DestroyNameplates()
    {
        foreach (Transform child in _namePlatesParent)
            Destroy(child.gameObject);
    }

    void CreateNameplates()
    {
        Debug.Log($"CreateNameplates. Steam State: {SteamAPI.IsSteamRunning()}. Nb local: {_nbLocalPlayersInputed.text}");
        if (GameManager.instance.connection == GameManager.Connection.Online)
        {
            List<Photon.Realtime.Player> newListPlayers = PhotonNetwork.CurrentRoom.Players.Values.ToList();
            foreach (Photon.Realtime.Player player in newListPlayers)
            {
                print($"CreateNameplates Player {player.NickName} - {(string)player.CustomProperties["username"]} " +
                    $"has {(int)player.CustomProperties["localPlayerCount"] - 1} invites");
                Instantiate(_namePlatePrefab, _namePlatesParent).GetComponent<PlayerNamePlate>().SetUp(player, false);



                // Online Splitscreen
                if ((int)player.CustomProperties["localPlayerCount"] > 1)
                {
                    for (int i = 1; i < (int)player.CustomProperties["localPlayerCount"]; i++)
                    {
                        if (CurrentRoomManager.instance.playerDataCells.Where(item => item.occupied
                        && item.rewiredId == i && item.playerExtendedPublicData.player_id == int.Parse(player.NickName)).Count() > 0)
                        {
                            // there is already a cell for that invite. Do this
                            print($"CreateNameplates there is already a cell for that invite {int.Parse(player.NickName)} - {(string)player.CustomProperties["username"]}: {i}");
                            Instantiate(_namePlatePrefab, _namePlatesParent).GetComponent<PlayerNamePlate>().Setup(CurrentRoomManager.instance.playerDataCells.Where(item => item.occupied
                        && item.rewiredId == i && item.playerExtendedPublicData.player_id == int.Parse(player.NickName)).FirstOrDefault().playerExtendedPublicData.username
                        , CurrentRoomManager.instance.playerDataCells.IndexOf(CurrentRoomManager.instance.playerDataCells.Where(item => item.occupied && item.rewiredId == i && item.playerExtendedPublicData.player_id == int.Parse(player.NickName)).FirstOrDefault()));
                        }
                        else
                        {
                            print($"CreateNameplates making for invite {int.Parse(player.NickName)} - {(string)player.CustomProperties["username"]}: {i}");
                            int ii = CurrentRoomManager.GetUnoccupiedDataCell();
                            ScriptObjPlayerData s = CurrentRoomManager.GetLocalPlayerData(ii);
                            s.playerExtendedPublicData = new PlayerDatabaseAdaptor.PlayerExtendedPublicData();
                            s.occupied = true;
                            s.rewiredId = i;
                            s.photonRoomIndex = PhotonNetwork.CurrentRoom.Players.FirstOrDefault(x => x.Value == player).Key;
                            s.playerExtendedPublicData.player_id = int.Parse(player.NickName);
                            s.playerExtendedPublicData.username = $"{(string)player.CustomProperties["username"]} - {i}";
                            s.playerExtendedPublicData.armor_data_string = "helmet1";
                            s.playerExtendedPublicData.armor_color_palette = "grey";
                            s.playerExtendedPublicData.level = 1;

                            s.local = (PhotonNetwork.NickName == player.NickName);

                            Instantiate(_namePlatePrefab, _namePlatesParent).GetComponent<PlayerNamePlate>().Setup(s.playerExtendedPublicData.username, ii);
                        }
                    }
                }
            }
        }
        else
        {
            print($"CreateNameplates local: {_nbLocalPlayersInputed.text} {GameManager.instance.nbLocalPlayersPreset}");
            if (_nbLocalPlayersInputed.text.Equals("")) _nbLocalPlayersInputed.text = "1";
            for (int i = 0; i < int.Parse(_nbLocalPlayersInputed.text.ToString()); i++)
                Instantiate(_namePlatePrefab, _namePlatesParent).GetComponent<PlayerNamePlate>().Setup($"player{i + 1}", i);
        }

    }

    public static void CreateLocalModePlayerDataCells()
    {
        print($"CreateLocalModePlayerDataCells {GameManager.instance.nbLocalPlayersPreset}");

        for (int i = 0; i < GameManager.instance.nbLocalPlayersPreset; i++)
        {
            CurrentRoomManager.GetLocalPlayerData(i).playerExtendedPublicData = new PlayerDatabaseAdaptor.PlayerExtendedPublicData();
            CurrentRoomManager.GetLocalPlayerData(i).occupied = true;
            CurrentRoomManager.GetLocalPlayerData(i).local = true;
            CurrentRoomManager.GetLocalPlayerData(i).photonRoomIndex = i + 1;
            CurrentRoomManager.GetLocalPlayerData(i).rewiredId = i;
            CurrentRoomManager.GetLocalPlayerData(i).playerExtendedPublicData.player_id = i;
            CurrentRoomManager.GetLocalPlayerData(i).playerExtendedPublicData.username = $"player{i + 1}";
            CurrentRoomManager.GetLocalPlayerData(i).playerExtendedPublicData.armor_data_string = "helmet1";
            CurrentRoomManager.GetLocalPlayerData(i).playerExtendedPublicData.armor_color_palette = "grey";
            CurrentRoomManager.GetLocalPlayerData(i).playerExtendedPublicData.level = 1;


            if (i == 1)
            {
                CurrentRoomManager.GetLocalPlayerData(i).playerExtendedPublicData.armor_data_string = "vanguard_hc-commando_rsa-commando_lsa-overseer_ca";
                CurrentRoomManager.GetLocalPlayerData(i).playerExtendedPublicData.armor_color_palette = "blue";
            }
            else if (i == 2)
            {
                CurrentRoomManager.GetLocalPlayerData(i).playerExtendedPublicData.armor_data_string = "infiltrator_hc-pilot_rsa-pilot_lsa-grenadier_ca";
                CurrentRoomManager.GetLocalPlayerData(i).playerExtendedPublicData.armor_color_palette = "yellow";
            }
            else if (i == 3)
            {
                CurrentRoomManager.GetLocalPlayerData(i).playerExtendedPublicData.armor_data_string = "sentry_hc-guerilla_ca-security_lsa-security_rsa";
                CurrentRoomManager.GetLocalPlayerData(i).playerExtendedPublicData.armor_color_palette = "green";
            }
        }



        CurrentRoomManager.GetLocalPlayerData(0).playerExtendedPublicData.armor_data_string = "helmet1-operator_lsa-operator_rsa-patrol_ca";
        CurrentRoomManager.GetLocalPlayerData(0).playerExtendedPublicData.armor_color_palette = "red";
    }

    public static void TogglePlayerModel(bool b)
    {
        FindObjectOfType<ArmoryManager>(true).ResetPlayerModelRotation();
        instance.playerModel.GetComponent<PlayerArmorManager>().PreventReloadArmor = true;
        instance.playerModel.SetActive(b);
    }


    public void ChangeNumberLocalPlayersBtn(int i)
    {
        PlayClickSound();
        GameManager.instance.nbLocalPlayersPreset += i;
        _nbLocalPlayersInputed.text = GameManager.instance.nbLocalPlayersPreset.ToString();
    }

    public void EnableGamePadCursorIn2Seconds()
    {
        StartCoroutine(EnableGamePadCursorIn2Seconds_Coroutine());
    }

    public void FindMasterClientAndToggleIcon()
    {
        if (namePlatesParent.childCount > 0)
        {
            namePlatesParent.transform.GetChild(0).GetComponent<PlayerNamePlate>().ToggleLeaderIcon(true);

            foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
            {
                //print($"FindMasterClientAndToggleIcon: {player.NickName} {player.IsMasterClient}");
                if (player.IsMasterClient)
                {
                    foreach (Transform child in namePlatesParent)
                    {
                        try
                        {
                            if (child.GetComponent<PlayerNamePlate>())
                            {
                                //print($"FindMasterClientAndToggleIcon: {child.GetComponent<PlayerNamePlate>().playerDataCell.playerExtendedPublicData.player_id}");
                                if (child.GetComponent<PlayerNamePlate>().playerDataCell)
                                {
                                    if (child.GetComponent<PlayerNamePlate>().playerDataCell.playerExtendedPublicData.player_id == int.Parse(player.NickName)
                                        && child.GetComponent<PlayerNamePlate>().playerDataCell.rewiredId == 0)
                                        child.GetComponent<PlayerNamePlate>().ToggleLeaderIcon(true);
                                    else
                                        child.GetComponent<PlayerNamePlate>().ToggleLeaderIcon(false);
                                }
                                else
                                {
                                    child.GetComponent<PlayerNamePlate>().ToggleLeaderIcon(false);
                                }
                            }
                        }
                        catch { }
                    }
                }
            }
        }
    }

    IEnumerator EnableGamePadCursorIn2Seconds_Coroutine()
    {
        yield return new WaitForSeconds(1);
        menuGamePadCursorScript.gameObject.SetActive(true);
    }

    IEnumerator SendGameParamsWithDelay()
    {
        yield return new WaitForSeconds(0.2f);
        NetworkGameManager.instance.SendGameParams();
    }
}
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

    public bool CountdownStarted { get { return CurrentRoomManager.instance.roomGameStartCountdown != Launcher.DEFAULT_ROOM_COUNTDOWN; } }

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

            _mapSelectedPreview.gameObject.SetActive(true);
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
    [SerializeField] TMP_Text _sprintModeText, _hitMarkersMode, _flyingCameraModeText, _thirdPersonModeOptionsHeader, _oneObjModeOptionsHeader;
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
    [SerializeField] TMP_Text _teamText, _difficultyText, _mapPreviewText;
    [SerializeField] GameObject _teamModeBtns;
    [SerializeField] GameObject _swarmDifficultyBtns;
    [SerializeField] GameObject _vetoBtn;
    [SerializeField] TMP_Text _matchStartCountdownText;

    [SerializeField] TMP_InputField _loginUsernameText;
    [SerializeField] TMP_InputField registerUsernameText;
    [SerializeField] TMP_InputField _loginPasswordText;
    [SerializeField] TMP_InputField registerPasswordText;

    [Header("Master Client Only")]
    [SerializeField] GameObject _mapSelectionBtn, _startGameButton;
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
    public TMP_Text thirdPersonModeOptionsHeader { get { return _thirdPersonModeOptionsHeader; } }
    public TMP_Text flyingCameraMode { get { return _flyingCameraModeText; } }
    public TMP_Text oneObjMode { get { return _oneObjModeOptionsHeader; } }

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

    [SerializeField] List<RoomInfo> _roomsCached = new List<RoomInfo>();



    public TMP_Text mapSelectedText
    {
        get { return _mapSelectedText; }
    }

    public TMP_Text gametypeSelectedText
    {
        get { return _gametypeSelectedText; }
    }



    void Awake()
    {
        _creatingRoomTimeOut = 999;

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
        _nbLocalPlayersInputed.text = GameManager.instance.nbLocalPlayersPreset.ToString();

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

        if (_creatingRoomTimeOut > 0 && _creatingRoomTimeOut != 999)
        {
            _creatingRoomTimeOut -= Time.deltaTime;
            if (_creatingRoomTimeOut <= 0)
            {
                _creatingRoomTimeOut = 999;
                errorText.text = "Error while creating room";
                GameManager.instance.previousScenePayloads.Add(PreviousScenePayload.ErrorWhileCreatingRoom);
                MenuManager.Instance.OpenPopUpMenu("error");
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

    float _creatingRoomTimeOut;

    public void CreatePrivateRoom()
    {
        if (roomNameInputField.text.Length != 0)
        {


            if (!RoomBrowserMenu.GAMEPAD_ROOM_NAMES.Contains(roomNameInputField.text) && !RoomBrowserMenu.FORBIDDEN_ROOM_NAMES.Contains(roomNameInputField.text))
            {
                Debug.Log($"CreateMultiplayerRoom. Client State: {PhotonNetwork.NetworkClientState}");

                _creatingRoomTimeOut = 3;

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


                if (GameManager.instance.connection == GameManager.NetworkType.Internet && PhotonNetwork.NetworkClientState != ClientState.Disconnected)
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
                else if (GameManager.instance.connection == GameManager.NetworkType.Local)
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
        else
        {
            errorText.text = "Please enter a name for your room";
            MenuManager.Instance.OpenPopUpMenu("error");
        }
    }




    public void CreatePrivateRoomFromGamepad()
    {
        if (GameManager.instance.connection == GameManager.NetworkType.Local)
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


            if (GameManager.instance.connection == GameManager.NetworkType.Internet && PhotonNetwork.NetworkClientState != ClientState.Disconnected)
            {
                //PhotonNetwork.CreateRoom(roomNameInputField.text, options); // Create a room with the text in parameter
                CreateRoom(roomNameToUse, options);
                MenuManager.Instance.OpenLoadingMenu("Creating Multiplayer Room..."); // Show the loading menu/message

                // When creating a room is done, OnJoinedRoom() will automatically trigger
                OnCreateMultiplayerRoomButton?.Invoke(this);
            }
            else if (GameManager.instance.connection == GameManager.NetworkType.Local)
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

        ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable { { "username", GameManager.ROOT_PLAYER_NAME }, { "localPlayerCount", GameManager.instance.nbLocalPlayersPreset }/*, { "databaseID", GameManager.STEAM_ID }*/ };
        PhotonNetwork.LocalPlayer.SetCustomProperties(h);


        CurrentRoomManager.instance.expectedNbPlayers = 0;
        GameManager.instance.RecalculateExpectedNbPlayersUsingPlayerCustomProperties();
        CurrentRoomManager.instance.vetoCountdown = CurrentRoomManager.instance.roomGameStartCountdown = DEFAULT_ROOM_COUNTDOWN;

        if (tl == null)
        {
            try
            {
                PhotonNetwork.CreateRoom(roomNam, ro);
            }
            catch
            {
                errorText.text = "Failed to create room";
                Debug.LogError("Failed to create room");

                GameManager.instance.previousScenePayloads.Add(PreviousScenePayload.ErrorWhileCreatingRoom);
                MenuManager.Instance.OpenPopUpMenu("error");
            }
        }
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
        Debug.Log($"Joined room {PhotonNetwork.CurrentRoom.Name}");
        _creatingRoomTimeOut = 999;
        CreatePrimitiveDataCellsFromRoomDataWhenJoiningRoom_Online(JoinType.IJoinedARoom);
        TriggerOnJoinedRoomBehaviour();

        //FindMasterClientAndToggleIcon();
    }

    public void TriggerOnJoinedRoomBehaviour(bool changeMenusAlso = true)
    {
        GameManager.instance.RecalculateExpectedNbPlayersUsingPlayerCustomProperties();

        if (PhotonNetwork.InRoom)
        {
            DestroyNameplates();


            if (PhotonNetwork.CurrentRoom.Name == quickMatchRoomName)
                CurrentRoomManager.instance.roomType = CurrentRoomManager.RoomType.QuickMatch;
            else
                CurrentRoomManager.instance.roomType = CurrentRoomManager.RoomType.Private;

            { // Room is private
                if (changeMenusAlso == true)
                {

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

                    _startGameButton.SetActive(PhotonNetwork.IsMasterClient && CurrentRoomManager.instance.roomType == CurrentRoomManager.RoomType.Private);
                }


                if (PhotonNetwork.IsMasterClient)
                {

                    Debug.Log("Joined room IsMasterClient");

                    //if (listPlayersDiff[0].NickName.Contains(GameManager.instance.rootPlayerNickname))
                    //    GameManager.instance.gameMode = GameManager.GameMode.Multiplayer;



                    if (CurrentRoomManager.instance.roomType == CurrentRoomManager.RoomType.QuickMatch)
                    {

                    }



                    PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs/Managers", "NetworkGameManager"), Vector3.zero, Quaternion.identity);
                    NetworkGameManager.instance.SendGameParams();
                }
            }



            CreateNameplates();
        }
    }

    // Runs only when OTHER player joined room.
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        print("Other player joined room");
        Debug.Log("LAUNCHER OnPlayerEnteredRoom");

        CreatePrimitiveDataCellsFromRoomDataWhenJoiningRoom_Online(JoinType.AnotherPlayerJoinedARoom);
        DestroyAndCreateNameplates();

        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(InstantiateNetworkGameManager_Coroutine());
            //StartCoroutine(SendGameParamsWithDelay());
        }
        //GameManager.instance.RecalculateExpectedNbPlayersUsingPlayerCustomProperties(); // Triggers Error here. Not enough time given for DB to send data and populate DataCell
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {

    }









    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        print($"OnMasterClientSwitched {newMasterClient.NickName} {PhotonNetwork.IsMasterClient}");


        GameManager.instance.gameMode = GameMode.Versus;
        GameManager.instance.teamMode = TeamMode.None;
        GameManager.instance.sprintMode = SprintMode.On;
        GameManager.instance.hitMarkersMode = HitMarkersMode.On;
        GameManager.instance.difficulty = SwarmManager.Difficulty.Normal;

        if (PhotonNetwork.IsMasterClient)
        {
            NetworkGameManager.instance.CancelStartGameButton();
            PhotonNetwork.CurrentRoom.IsOpen = true;
        }

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
        //PhotonNetwork.CurrentRoom.IsVisible = false;

        _startGameButton.SetActive(false);



        int c = 0;
        foreach (ScriptObjPlayerData s in CurrentRoomManager.instance.playerDataCells.Where(item => item.occupied))
        {
            if (GameManager.instance.teamMode == TeamMode.None)
            {
                print($"{s.steamId} {s.rewiredId} will get spawn {c}");
                NetworkGameManager.instance.SetPlayerDataCellStartingSpawnPositionIndex(s.steamId, s.rewiredId, c);
                c++;
            }
            else
            {
                int blue = 0;
                if (s.team == Team.Red)
                {
                    print($"{s.steamId} {s.rewiredId} will get spawn {c} Red Team");
                    NetworkGameManager.instance.SetPlayerDataCellStartingSpawnPositionIndex(s.steamId, s.rewiredId, c);
                    c++;
                }
                else if (s.team == Team.Blue)
                {
                    print($"{s.steamId} {s.rewiredId} will get spawn {blue} Blue Team");
                    NetworkGameManager.instance.SetPlayerDataCellStartingSpawnPositionIndex(s.steamId, s.rewiredId, blue);
                    blue++;
                }
            }
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
        if (GameManager.instance.connection == GameManager.NetworkType.Internet)
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
        ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable { { "username", GameManager.ROOT_PLAYER_NAME }, { "localPlayerCount", GameManager.instance.nbLocalPlayersPreset }/*, { "databaseID", GameManager.STEAM_ID }*/ };
        PhotonNetwork.LocalPlayer.SetCustomProperties(h);


        PhotonNetwork.JoinRoom(info.Name);
        MenuManager.Instance.OpenLoadingMenu("Joining Room...");
    }

    // Triggers when YOU left room

    public override void OnLeftRoom() // Is also called when quitting a game while connected to the internet. Does not trigger when offline
    {
        Debug.Log("LAUNCHER: OnLeftRoom");
        CurrentRoomManager.instance.expectedNbPlayers = 0;
        try
        {
            Destroy(FindObjectOfType<NetworkGameManager>().gameObject);
        }
        catch (System.Exception e) { Debug.LogWarning(e); }

        if (_tryingToLeaveRoomFromMenu && GameManager.instance.connection == GameManager.NetworkType.Internet)
        {
            _tryingToLeaveRoomFromMenu = false;
            MenuManager.Instance.OpenMenu("online title");
        }

        DestroyNameplates();
        //MenuManager.Instance.OpenMenu("offline title");
    }

    // Runs only when other player left room
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        if (CurrentRoomManager.instance.PlayerDataContains(long.Parse(otherPlayer.NickName)))
        {
            foreach (Transform child in instance.namePlatesParent)
                if (child.GetComponent<PlayerNamePlate>().playerDataCell.steamId == long.Parse(otherPlayer.NickName))
                    Destroy(child.gameObject);

            CurrentRoomManager.instance.RemoveExtendedPlayerData(long.Parse(otherPlayer.NickName));
        }
    }


    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        print("OnRoomListUpdate");
        _roomsCached.Clear();

        foreach (Transform trans in roomListContent)
        {
            Destroy(trans.gameObject);
        }

        for (int i = 0; i < roomList.Count; i++)
        {
            _roomsCached.Add(roomList[i]);

            if (!roomList[i].IsOpen || roomList[i].RemovedFromList)
                continue;

            Instantiate(roomListItemPrefab, roomListContent).GetComponent<RoomListItem>().SetUp(roomList[i]);
        }
    }

    public void OpenMapSelectionPopUpMenu()
    {
        if (!CountdownStarted)
            if (PhotonNetwork.IsMasterClient && PhotonNetwork.InRoom)
                MenuManager.Instance.OpenPopUpMenu("map selection");
    }

    public void OpenGametypeSelectionPopUpMenu()
    {
        if (!CountdownStarted)
            if (PhotonNetwork.IsMasterClient && PhotonNetwork.InRoom)
                MenuManager.Instance.OpenPopUpMenu("gametype selection");
    }

    public void OpenGameOptionsPopUpMenu()
    {
        if (!CountdownStarted)
            if (PhotonNetwork.IsMasterClient && PhotonNetwork.InRoom)
                MenuManager.Instance.OpenPopUpMenu("game options");
    }

    public void ChangeLevelToLoadWithIndex(int index)
    {
        if (!CountdownStarted)
        {
            if (MenuManager.Instance.GetActiveMenusName().Contains("map selection"))
                MenuManager.Instance.CloseMenu("map selection");

            Launcher.instance.levelToLoadIndex = index;
            NetworkGameManager.instance.SendGameParams();
        }
    }

    public void ChangeGameType(string gt)
    {
        if (!CountdownStarted)
        {
            if (MenuManager.Instance.GetActiveMenusName().Contains("gametype selection"))
                MenuManager.Instance.CloseMenu("gametype selection");

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

    public void ToggleGameMode()
    {
        if (!CountdownStarted)
            if (PhotonNetwork.IsMasterClient && PhotonNetwork.InRoom)
            {
                if (GameManager.instance.gameMode == GameMode.Versus) GameManager.instance.gameMode = GameMode.Coop;
                else if (GameManager.instance.gameMode == GameMode.Coop) GameManager.instance.gameMode = GameMode.Versus;

                //GameManager.instance.gameMode = (GameManager.GameMode)System.Enum.Parse(typeof(GameManager.GameMode), gt);
                if (GameManager.instance.gameMode == GameManager.GameMode.Versus) GameManager.instance.teamMode = GameManager.TeamMode.None;
                if (GameManager.instance.gameMode == GameManager.GameMode.Coop) GameManager.instance.teamMode = GameManager.TeamMode.Classic;

                FindObjectOfType<NetworkGameManager>().SendGameParams();
            }
    }

    public void ChangeTeamMode(string tm)
    {
        Debug.Log("ChangeTeamMode Btn");

        if (!CountdownStarted)
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
        if (!CountdownStarted)
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
        if (!CountdownStarted)
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
        if (!CountdownStarted)
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

    public void ChangeThirdPersonMode()
    {
        if (!CountdownStarted)
        {
            if (GameManager.instance.thirdPersonMode == ThirdPersonMode.On)
            {
                GameManager.instance.thirdPersonMode = ThirdPersonMode.Off;
            }
            else
            {
                GameManager.instance.thirdPersonMode = ThirdPersonMode.On;
            }

            if (PhotonNetwork.IsMasterClient)
                NetworkGameManager.instance.SendGameParams();
        }
    }

    public void ChangeFloatingCameraPermission()
    {
        if (!CountdownStarted)
        {
            if (GameManager.instance.flyingCameraMode == FlyingCamera.Enabled)
            {
                GameManager.instance.flyingCameraMode = FlyingCamera.Disabled;
            }
            else
            {
                GameManager.instance.flyingCameraMode = FlyingCamera.Enabled;
            }

            if (PhotonNetwork.IsMasterClient)
                NetworkGameManager.instance.SendGameParams();
        }
    }

    public void ChangeOneObjMode()
    {
        if (!CountdownStarted)
        {
            if (GameManager.instance.oneObjMode == OneObjMode.On)
            {
                GameManager.instance.oneObjMode = OneObjMode.Off;
            }
            else
            {
                GameManager.instance.oneObjMode = OneObjMode.On;
            }

            if (PhotonNetwork.IsMasterClient)
                NetworkGameManager.instance.SendGameParams();
        }
    }


    public void ChangeTeamOfLocalPlayer(int localPlayerInd)
    {
        if (!CountdownStarted)
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


    public void UpdateMapPreviewText()
    {
        if (GameManager.instance.gameMode == GameMode.Versus)
        {
            _mapPreviewText.text = $"{GameManager.instance.gameType} on {GameManager.instance.mapDataCells.Where(obj => obj.sceneBuildIndex.Equals(levelToLoadIndex)).SingleOrDefault().mapName}";
            if (GameManager.instance.gameType == GameType.Hill)
                _mapPreviewText.text = $"King of the Hill on {GameManager.instance.mapDataCells.Where(obj => obj.sceneBuildIndex.Equals(levelToLoadIndex)).SingleOrDefault().mapName}";

            if (GameManager.instance.teamMode == TeamMode.Classic)
                _mapPreviewText.text = "Team " + _mapPreviewText.text;
        }
        else if (GameManager.instance.gameMode == GameMode.Coop)
        {
            _mapPreviewText.text = $"{GameManager.instance.gameType} on {GameManager.instance.mapDataCells.Where(obj => obj.sceneBuildIndex.Equals(levelToLoadIndex)).SingleOrDefault().mapName}";
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

        MenuManager.Instance.OpenLoadingMenu("Loging in...");
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




    public enum JoinType { IJoinedARoom, AnotherPlayerJoinedARoom }
    public void CreatePrimitiveDataCellsFromRoomDataWhenJoiningRoom_Online(JoinType jt)
    {
        if (GameManager.instance.connection == GameManager.NetworkType.Internet)
        {
            // PHOTON DOES NOT REMOVE LEFT PLAYERS FROM THEIR LIST OF PLAYERS IN CURRENT ROOM. I MUST HANDLE THAT MYSELF
            // DO NOT DELETE THIS
            List<Photon.Realtime.Player> newListPlayers = PhotonNetwork.CurrentRoom.Players.Values.ToList();

            List<(long steamIdd, string steamName, int supposedRoomIndex, int trueRoomIndex, int nbLocalPlayers)> test
                = new List<(long, string, int, int, int)>();
            foreach (Photon.Realtime.Player player in newListPlayers)
            {
                test.Add((long.Parse(player.NickName),
                    player.CustomProperties["username"].ToString(),
                    PhotonNetwork.CurrentRoom.Players.FirstOrDefault(x => x.Value == player).Key, 0, (int)player.CustomProperties["localPlayerCount"]));
            }
            test.Sort((a, b) => a.supposedRoomIndex.CompareTo(b.supposedRoomIndex));
            for (int i = 0; i < test.Count; i++)
            {
                print($"CreateDataCellsFromRoomDataWhenJoiningRoom_Online {test[i].steamName} {test[i].supposedRoomIndex} will become {i}");
                test[i] = (test[i].steamIdd, test[i].steamName, test[i].supposedRoomIndex, i + 1, test[i].nbLocalPlayers);
            }
            // DO NOT DELETE THIS


            foreach (var entry in test)
            {
                if (entry.steamName.Equals(CurrentRoomManager.instance.playerDataCells[0].steamName))
                {
                    CurrentRoomManager.instance.playerDataCells[0].photonRoomIndex = entry.trueRoomIndex;
                }
                else
                {
                    int ii = CurrentRoomManager.GetUnoccupiedDataCell();
                    ScriptObjPlayerData s = CurrentRoomManager.GetLocalPlayerData(ii);
                    s.steamId = entry.steamIdd;
                    s.steamName = entry.steamName;
                    s.playerExtendedPublicData = new PlayerDatabaseAdaptor.PlayerExtendedPublicData();
                    s.occupied = true;
                    s.photonRoomIndex = entry.trueRoomIndex;

                    s.local = (PhotonNetwork.NickName == entry.steamIdd.ToString());
                }


                // Online Splitscreen
                if (entry.nbLocalPlayers > 1 &&
                    (jt == JoinType.IJoinedARoom ||
                    (jt == JoinType.AnotherPlayerJoinedARoom && !entry.steamName.Equals(CurrentRoomManager.instance.playerDataCells[0].steamName))))
                {
                    for (int i = 1; i < entry.nbLocalPlayers; i++)
                    {
                        int ii = CurrentRoomManager.GetUnoccupiedDataCell();
                        ScriptObjPlayerData s = CurrentRoomManager.GetLocalPlayerData(ii);
                        s.steamId = entry.steamIdd;
                        s.steamName = $"{entry.steamName} - {i}";
                        s.playerExtendedPublicData = new PlayerDatabaseAdaptor.PlayerExtendedPublicData();
                        s.occupied = true;
                        s.rewiredId = i;
                        s.photonRoomIndex = entry.trueRoomIndex;

                        s.local = (PhotonNetwork.NickName == entry.steamIdd.ToString());
                    }
                }
            }

            FetchExtendedPlayerStats();
        }
        else
        {

        }
    }


    public void FetchExtendedPlayerStats()
    {
        print("FetchExtendedPlayerStats");
        foreach (ScriptObjPlayerData s in CurrentRoomManager.instance.playerDataCells.Where(x => x.occupied && x.rewiredId == 0))
        {
            WebManager.webManagerInstance.SetPlayerListItemInRoom(s.steamId);
        }
    }



    void CreateNameplates()
    {
        if (GameManager.instance.connection == GameManager.NetworkType.Internet)
        {
            foreach (ScriptObjPlayerData s in CurrentRoomManager.instance.playerDataCells.Where(x => x.occupied))
            {
                Instantiate(_namePlatePrefab, _namePlatesParent).
                    GetComponent<PlayerNamePlate>().Setup(s.steamName, CurrentRoomManager.instance.playerDataCells.IndexOf(s));
            }
        }
        else
        {
            print($"CreateNameplates local: {_nbLocalPlayersInputed.text} {GameManager.instance.nbLocalPlayersPreset}");
            if (_nbLocalPlayersInputed.text.Equals("")) _nbLocalPlayersInputed.text = "1";
            for (int i = 0; i < int.Parse(_nbLocalPlayersInputed.text.ToString()); i++)
                Instantiate(_namePlatePrefab, _namePlatesParent).GetComponent<PlayerNamePlate>().Setup($"player{i + 1}", i);
        }











        //Debug.Log($"CreateNameplates. Steam State: {SteamAPI.IsSteamRunning()}. Nb local: {_nbLocalPlayersInputed.text}");
        //if (GameManager.instance.connection == GameManager.NetworkType.Internet)
        //{
        //    List<Photon.Realtime.Player> newListPlayers = PhotonNetwork.CurrentRoom.Players.Values.ToList();
        //    foreach (Photon.Realtime.Player player in newListPlayers)
        //    {
        //        print($"CreateNameplates Player {player.NickName} - {(string)player.CustomProperties["username"]} " +
        //            $"has {(int)player.CustomProperties["localPlayerCount"] - 1} invites");
        //        Instantiate(_namePlatePrefab, _namePlatesParent).GetComponent<PlayerNamePlate>().SetUp(player, false);



        //        // Online Splitscreen
        //        if ((int)player.CustomProperties["localPlayerCount"] > 1)
        //        {
        //            for (int i = 1; i < (int)player.CustomProperties["localPlayerCount"]; i++)
        //            {
        //                if (CurrentRoomManager.instance.playerDataCells.Where(item => item.occupied
        //                && item.rewiredId == i && item.steamId == long.Parse(player.NickName)).Count() > 0)
        //                {
        //                    // there is already a cell for that invite. Do this
        //                    print($"CreateNameplates there is already a cell for that invite {long.Parse(player.NickName)} - {(string)player.CustomProperties["username"]}: {i}");
        //                    Instantiate(_namePlatePrefab, _namePlatesParent).GetComponent<PlayerNamePlate>().Setup(CurrentRoomManager.instance.playerDataCells.Where(item => item.occupied
        //                && item.rewiredId == i && item.steamId == long.Parse(player.NickName)).FirstOrDefault().steamName
        //                , CurrentRoomManager.instance.playerDataCells.IndexOf(CurrentRoomManager.instance.playerDataCells.Where(item => item.occupied && item.rewiredId == i && item.steamId == long.Parse(player.NickName)).FirstOrDefault()));
        //                }
        //                else
        //                {
        //                    print($"CreateNameplates making for invite {long.Parse(player.NickName)} - {(string)player.CustomProperties["username"]}: {i}");
        //                    int ii = CurrentRoomManager.GetUnoccupiedDataCell();
        //                    ScriptObjPlayerData s = CurrentRoomManager.GetLocalPlayerData(ii);
        //                    s.steamId = long.Parse(player.NickName);
        //                    s.steamName = $"{(string)player.CustomProperties["username"]} - {i}";
        //                    s.playerExtendedPublicData = new PlayerDatabaseAdaptor.PlayerExtendedPublicData();
        //                    s.occupied = true;
        //                    s.rewiredId = i;
        //                    s.photonRoomIndex = PhotonNetwork.CurrentRoom.Players.FirstOrDefault(x => x.Value == player).Key;
        //                    s.playerExtendedPublicData.armorDataString = "helmet1";
        //                    s.playerExtendedPublicData.armorColorPalette = "grey";
        //                    s.playerExtendedPublicData.level = 1;

        //                    s.local = (PhotonNetwork.NickName == player.NickName);

        //                    Instantiate(_namePlatePrefab, _namePlatesParent).GetComponent<PlayerNamePlate>().Setup(s.steamName, ii);
        //                }
        //            }
        //        }
        //    }
        //}
        //else
        //{
        //    print($"CreateNameplates local: {_nbLocalPlayersInputed.text} {GameManager.instance.nbLocalPlayersPreset}");
        //    if (_nbLocalPlayersInputed.text.Equals("")) _nbLocalPlayersInputed.text = "1";
        //    for (int i = 0; i < int.Parse(_nbLocalPlayersInputed.text.ToString()); i++)
        //        Instantiate(_namePlatePrefab, _namePlatesParent).GetComponent<PlayerNamePlate>().Setup($"player{i + 1}", i);
        //}

    }

    public static void CreateLocalModePlayerDataCells()
    {
        print($"CreateLocalModePlayerDataCells {GameManager.instance.nbLocalPlayersPreset}");

        for (int i = 0; i < GameManager.instance.nbLocalPlayersPreset; i++)
        {
            CurrentRoomManager.GetLocalPlayerData(i).steamId = i;
            CurrentRoomManager.GetLocalPlayerData(i).steamName = $"Player {i + 1}";
            CurrentRoomManager.GetLocalPlayerData(i).playerExtendedPublicData = new PlayerDatabaseAdaptor.PlayerExtendedPublicData();
            CurrentRoomManager.GetLocalPlayerData(i).occupied = true;
            CurrentRoomManager.GetLocalPlayerData(i).local = true;
            CurrentRoomManager.GetLocalPlayerData(i).photonRoomIndex = i + 1;
            CurrentRoomManager.GetLocalPlayerData(i).rewiredId = i;
            CurrentRoomManager.GetLocalPlayerData(i).playerExtendedPublicData.armorDataString = "helmet1";
            CurrentRoomManager.GetLocalPlayerData(i).playerExtendedPublicData.armorColorPalette = "grey";


            if (i == 1)
            {
                CurrentRoomManager.GetLocalPlayerData(i).playerExtendedPublicData.armorDataString = "vanguard_hc-commando_rsa-commando_lsa-overseer_ca";
                CurrentRoomManager.GetLocalPlayerData(i).playerExtendedPublicData.armorColorPalette = "blue";
            }
            else if (i == 2)
            {
                CurrentRoomManager.GetLocalPlayerData(i).playerExtendedPublicData.armorDataString = "infiltrator_hc-pilot_rsa-pilot_lsa-grenadier_ca";
                CurrentRoomManager.GetLocalPlayerData(i).playerExtendedPublicData.armorColorPalette = "yellow";
            }
            else if (i == 3)
            {
                CurrentRoomManager.GetLocalPlayerData(i).playerExtendedPublicData.armorDataString = "sentry_hc-guerilla_ca-security_lsa-security_rsa";
                CurrentRoomManager.GetLocalPlayerData(i).playerExtendedPublicData.armorColorPalette = "green";
            }
        }



        CurrentRoomManager.GetLocalPlayerData(0).playerExtendedPublicData.armorDataString = "helmet1-operator_lsa-operator_rsa-patrol_ca";
        CurrentRoomManager.GetLocalPlayerData(0).playerExtendedPublicData.armorColorPalette = "red";
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
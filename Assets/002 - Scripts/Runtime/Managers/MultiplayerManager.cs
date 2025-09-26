using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using System;
using System.Linq;
using System.Net.Mail;
using UnityEngine.SocialPlatforms.Impl;

public class MultiplayerManager : MonoBehaviourPunCallbacks
{
    // public variables
    public static MultiplayerManager instance;

    public Dictionary<Vector3, LootableWeapon> lootableWeaponsDict = new Dictionary<Vector3, LootableWeapon>();


    public int scoreToWin
    {
        get
        {
            if (GameManager.instance.gameType == GameManager.GameType.GunGame)
            {
                {
                    Debug.Log("scoreToWin");
                    Debug.Log(GameManager.GetRootPlayer().playerInventory.playerGunGameManager.gunIndex.Count);
                }
                return GameManager.GetRootPlayer().playerInventory.playerGunGameManager.gunIndex.Count;
            }
            if (GameManager.instance.gameType == GameManager.GameType.Hill || GameManager.instance.gameType == GameManager.GameType.Oddball)
            {

#if (UNITY_EDITOR)
                // your code here
                //if (GameManager.instance.gameType == GameManager.GameType.Hill)
                //    return 5;
#endif



                if (GameManager.instance.teamMode == GameManager.TeamMode.Classic) return 120;
                return 75;
            }
            if (GameManager.instance.gameType == GameManager.GameType.CTF)
            {
                return 3;
            }
            if (GameManager.instance.teamMode == GameManager.TeamMode.None)
            {
#if (UNITY_EDITOR)
                // your code here
                //return 3;
#endif


                if (CurrentRoomManager.instance.expectedNbPlayers == 2) return 10;
                else return 15;
            }
            else if (GameManager.instance.teamMode == GameManager.TeamMode.Classic)
                return 25;


            // This would return null if the list was empty
            // Or the first element of the sorted lost -> smallest distance
            //if (GameTime.instance.timeRemaining == 0)
            //    return FindObjectsOfType<PlayerMultiplayerMatchStats>().OrderBy(pms => pms.score).FirstOrDefault().score;

            return 5;
        }
    }
    public int highestScore
    {
        get
        {
            int hs = 0;

            if (GameManager.instance.teamMode == GameManager.TeamMode.None)
            {
                foreach (Player p in GameManager.instance.GetAllPhotonPlayers())
                {
                    if (p && p.GetComponent<PlayerMultiplayerMatchStats>().score > hs)
                        hs = p.GetComponent<PlayerMultiplayerMatchStats>().score;
                }
            }
            else if (GameManager.instance.teamMode == GameManager.TeamMode.Classic)
            {
                hs = Math.Max(redTeamScore, blueTeamScore);
            }

            return hs;
        }
    }
    public int redTeamScore { get { return _redTeamScore; } private set { _redTeamScore = value; foreach (Player p in GameManager.instance.GetAllPhotonPlayers()) if (p) p.playerUI.UpdateScoreWitnesses(); } }
    public int blueTeamScore { get { return _blueTeamScore; } private set { _blueTeamScore = value; foreach (Player p in GameManager.instance.GetAllPhotonPlayers()) if (p) p.playerUI.UpdateScoreWitnesses(); } }


    public GameManager.Team winningTeam
    {
        get
        {
            return _winningTeam;
        }

        private set
        {
            _winningTeam = value;
        }
    }

    public bool isADraw
    {
        get
        {
            return _isADraw;
        }

        private set
        {
            _isADraw = value;

            if (_isADraw)
            {
                CurrentRoomManager.instance.winningPlayerStructs.Clear();
            }
        }
    }


    [SerializeField] int _redTeamScore;
    [SerializeField] int _blueTeamScore;


    int _initialRoomPlayercount;
    bool _isADraw;
    GameManager.Team _winningTeam;

    Player _gunGameWinner;

    // private variables
    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        try { _initialRoomPlayercount = PhotonNetwork.CurrentRoom.PlayerCount; } catch { }

        GameManager.instance.OnGameManagerFinishedLoadingScene_Late -= OnSceneLoaded;
        GameManager.instance.OnGameManagerFinishedLoadingScene_Late += OnSceneLoaded;
    }

    void OnSceneLoaded()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        Debug.Log("Multiplayer Manager OnSceneLoaded");

        redTeamScore = 0;
        blueTeamScore = 0;
        _isADraw = false;
        _winningTeam = GameManager.Team.None;
        _gunGameWinner = null;
    }
    public void AddPlayerKill(AddPlayerKillStruct struc)
    {
        try
        {
            PlayerMultiplayerMatchStats winningPlayerMS = GameManager.GetPlayerWithPhotonView(struc.winningPlayerPhotonId).GetComponent<PlayerMultiplayerMatchStats>();
            PlayerMultiplayerMatchStats losingPlayerMS = GameManager.GetPlayerWithPhotonView(struc.losingPlayerPhotonId).GetComponent<PlayerMultiplayerMatchStats>();

            if (highestScore >= scoreToWin)
                return;


            if (winningPlayerMS != losingPlayerMS) // NOT Suicide
            {


                winningPlayerMS.kills++;

                if (struc.headshot)
                    winningPlayerMS.headshots++;
                if (struc.melee)
                    winningPlayerMS.meleeKills++;
                if (struc.grenade)
                    winningPlayerMS.grenadeKills++;
                if (struc.stuck) winningPlayerMS.player.playerDataCell.playerCurrentGameScore.stickyKills++;
                if (struc.nuthshot) winningPlayerMS.player.playerDataCell.playerCurrentGameScore.nutshots++;




                //#if (UNITY_EDITOR)
                //                // your code here
                //                if (GameManager.instance.gameType == GameManager.GameType.CTF)
                //                {
                //                    if (winningPlayerMS.team == GameManager.Team.Red)
                //                        redTeamScore++;
                //                    else if (winningPlayerMS.team == GameManager.Team.Blue)
                //                        blueTeamScore++;
                //                }
                //#endif


                if (GameManager.instance.gameType != GameManager.GameType.Hill
                && GameManager.instance.gameType != GameManager.GameType.Oddball
                && GameManager.instance.gameType != GameManager.GameType.GunGame
                && GameManager.instance.gameType != GameManager.GameType.CTF)
                {
                    if (winningPlayerMS.team == GameManager.Team.Red)
                        redTeamScore++;
                    else if (winningPlayerMS.team == GameManager.Team.Blue)
                        blueTeamScore++;
                }

                if (GameManager.instance.gameType != GameManager.GameType.Hill
                && GameManager.instance.gameType != GameManager.GameType.Oddball
                && GameManager.instance.gameType != GameManager.GameType.GunGame
                && GameManager.instance.gameType != GameManager.GameType.CTF)
                {
                    winningPlayerMS.score++;
                }




                if (GameManager.instance.gameType == GameManager.GameType.GunGame)
                //if (winningPlayerMS.player == GameManager.GetLocalPlayer(winningPlayerMS.player.rid))
                {
                    if (struc.cleanDamageSource != WeaponProperties.KillFeedOutput.Melee && struc.cleanDamageSource != WeaponProperties.KillFeedOutput.Frag_Grenade
                        && struc.cleanDamageSource != WeaponProperties.KillFeedOutput.Plasma_Grenade
                        && struc.cleanDamageSource != WeaponProperties.KillFeedOutput.Stuck
                        && struc.cleanDamageSource != WeaponProperties.KillFeedOutput.Barrel
                        && struc.cleanDamageSource != WeaponProperties.KillFeedOutput.Assasination)
                    {
                        winningPlayerMS.score++;
                        winningPlayerMS.player.playerInventory.playerGunGameManager.index++;
                    }

                    Debug.Log(struc.cleanDamageSource);
                    if (struc.cleanDamageSource == WeaponProperties.KillFeedOutput.Plasma_Pistol)
                    {
                        winningPlayerMS.score++;
                        _gunGameWinner = winningPlayerMS.player;
                        if (PhotonNetwork.IsMasterClient) NetworkGameManager.instance.EndGame();
                    }
                }
            }
            else // Suicide
            {
                if (GameManager.instance.gameType != GameManager.GameType.Hill
               && GameManager.instance.gameType != GameManager.GameType.Oddball
               && GameManager.instance.gameType != GameManager.GameType.GunGame
               && GameManager.instance.gameType != GameManager.GameType.CTF)
                {
                    losingPlayerMS.score--;

                    if (winningPlayerMS.team == GameManager.Team.Red)
                        redTeamScore--;
                    else if (winningPlayerMS.team == GameManager.Team.Blue)
                        blueTeamScore--;
                }
            }

            losingPlayerMS.deaths++;

            if (PhotonNetwork.IsMasterClient && GameManager.instance.gameType != GameManager.GameType.GunGame)
            {
                CheckForEndGame();
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }

    public void AddPlayerPoint(int pid)
    {
        if (highestScore >= scoreToWin)
            return;
        PlayerMultiplayerMatchStats winningPlayerMS = GameManager.GetPlayerWithPhotonView(pid).GetComponent<PlayerMultiplayerMatchStats>();
        winningPlayerMS.score++;

        if (GameManager.instance.teamMode == GameManager.TeamMode.Classic)
        {
            if (GameManager.instance.gameType == GameManager.GameType.Hill || GameManager.instance.gameType == GameManager.GameType.Oddball
                || GameManager.instance.gameType == GameManager.GameType.CTF)
            {
                if (winningPlayerMS.player.team == GameManager.Team.Red)
                    redTeamScore++;
                else
                    blueTeamScore++;


                foreach (Player p in GameManager.instance.GetAllPhotonPlayers())
                {
                    if (p.isMine)
                        p.allPlayerScripts.scoreboardManager.UpdateTeamScores();
                }
            }
        }

        CheckForEndGame();
    }
    public void CheckForEndGame()
    {
        Log.Print(() => $"CheckForEndGame {highestScore} / {scoreToWin}");
        if (highestScore == scoreToWin)
            FindObjectOfType<NetworkGameManager>().EndGame();
        //EndGame();
    }
    public void EndGame(bool saveXp = true, bool actuallyQuit = false)
    {
        CurrentRoomManager.instance.gameOver = true;
        Log.Print(() => $"EndGame {highestScore} / {scoreToWin}");

        CreateWinningPlayersList();
        SpawnWinnerKillFeeds();


        foreach (Player pp in FindObjectsOfType<Player>())
        {

            // https://techdifferences.com/difference-between-break-and-continue.html#:~:text=The%20main%20difference%20between%20break,next%20iteration%20of%20the%20loop.
            // return will stop this method, break will stop the loop, continue will stop the current iteration
            if (!pp.PV.IsMine) continue;

            pp.playerUI.gamepadCursor.gameObject.SetActive(false);



            //CarnageReportMenu.winningTeam = GameManager.Team.None;


            //if (highestScore >= scoreToWin)
            //{
            //    PrintOnlyInEditor.Log($"EndGame {highestScore} / {scoreToWin}");
            //    if (GameManager.instance.teamMode == GameManager.TeamMode.None)
            //    {
            //        if (GameManager.instance.gameType != GameManager.GameType.GunGame)
            //            foreach (PlayerMultiplayerMatchStats pms in FindObjectsOfType<PlayerMultiplayerMatchStats>())
            //            {
            //                PrintOnlyInEditor.Log($"EndGame {pms.score} / {scoreToWin}");
            //                if (pms.score >= scoreToWin)
            //                {
            //                    pp.GetComponent<KillFeedManager>().EnterNewFeed($"<color=#31cff9>GAME OVER! {pms.GetComponent<Player>().username} wins!");
            //                    this.winningPlayers.Add(pms.GetComponent<Player>());
            //                    this.winningPlayersId.Add(pms.GetComponent<Player>().playerId);
            //                }
            //            }
            //        else if (GameManager.instance.gameType == GameManager.GameType.GunGame)
            //        {
            //            this.winningPlayers.Clear();
            //            this.winningPlayersId.Clear();

            //            foreach (PlayerMultiplayerMatchStats pms in FindObjectsOfType<PlayerMultiplayerMatchStats>())
            //            {
            //                if (pms.player.playerInventory.activeWeapon.killFeedOutput == WeaponProperties.KillFeedOutput.Plasma_Pistol)
            //                {
            //                    this.winningPlayers.Add(pms.GetComponent<Player>());
            //                    this.winningPlayersId.Add(pms.GetComponent<Player>().playerId);
            //                    break;
            //                }
            //            }

            //            foreach (PlayerMultiplayerMatchStats pms in FindObjectsOfType<PlayerMultiplayerMatchStats>())
            //            {
            //                pp.GetComponent<KillFeedManager>().EnterNewFeed($"<color=#31cff9>GAME OVER! {winningPlayers[0].username} wins!");
            //            }
            //        }
            //    }
            //    else if (GameManager.instance.teamMode == GameManager.TeamMode.Classic)
            //    {
            //        if (redTeamScore >= scoreToWin)
            //        {
            //            CarnageReportMenu.winningTeam = GameManager.Team.Red;

            //            pp.GetComponent<KillFeedManager>().EnterNewFeed($"<color=#31cff9>GAME OVER! Red Team wins!");


            //            foreach (PlayerMultiplayerMatchStats pms in FindObjectsOfType<PlayerMultiplayerMatchStats>())
            //                if (pms.GetComponent<Player>().team == GameManager.Team.Red)
            //                {
            //                    this.winningPlayers.Add(pms.GetComponent<Player>());
            //                    this.winningPlayersId.Add(pms.GetComponent<Player>().playerId);
            //                }
            //        }
            //        else if (blueTeamScore >= scoreToWin)
            //        {
            //            CarnageReportMenu.winningTeam = GameManager.Team.Blue;

            //            pp.GetComponent<KillFeedManager>().EnterNewFeed($"<color=#31cff9>GAME OVER! Blue Team wins!");

            //            foreach (PlayerMultiplayerMatchStats pms in FindObjectsOfType<PlayerMultiplayerMatchStats>())
            //                if (pms.GetComponent<Player>().team == GameManager.Team.Blue)
            //                {
            //                    this.winningPlayers.Add(pms.GetComponent<Player>());
            //                    this.winningPlayersId.Add(pms.GetComponent<Player>().playerId);
            //                }
            //        }
            //    }
            //}
            //else
            //{
            //    pp.GetComponent<KillFeedManager>().EnterNewFeed($"<color=#31cff9>GAME OVER!");


            //    if (GameManager.instance.teamMode == GameManager.TeamMode.Classic)
            //        if (redTeamScore >= blueTeamScore)
            //        {
            //            CarnageReportMenu.winningTeam = GameManager.Team.Red;
            //        }
            //        else
            //        {
            //            CarnageReportMenu.winningTeam = GameManager.Team.Blue;
            //        }
            //}

            if (saveXp)
            {
                if (pp.controllerId == 0)
                {
                    pp.allPlayerScripts.announcer.PlayGameOverClip();

                    //if (!GameManager.instance.devMode)
                    if (CurrentRoomManager.instance.youHaveInvites)
                    {
                        if (CurrentRoomManager.instance.halfOfPlayersInRoomAreRandos)
                        {
                            List<(long, int)> steamIdRewiredIdList = CurrentRoomManager.instance.winningPlayerStructs
    .Select(player => (player.steamId, player.rewiredId))
    .ToList();


                            WebManager.webManagerInstance.SaveMultiplayerStats(pp.GetComponent<PlayerMultiplayerMatchStats>(), steamIdRewiredIdList, isADraw);
                        }
                    }
                    else
                    {
                        List<(long, int)> steamIdRewiredIdList = CurrentRoomManager.instance.winningPlayerStructs
    .Select(player => (player.steamId, player.rewiredId))
    .ToList();
                        WebManager.webManagerInstance.SaveMultiplayerStats(pp.GetComponent<PlayerMultiplayerMatchStats>(), steamIdRewiredIdList, isADraw);
                    }

                    {

                        pp.LeaveLevelButStayInRoom();
                    }
                }
            }
            else
            {
                if (GameManager.instance.connection == GameManager.NetworkType.Internet)
                {
                    //if (!GameManager.instance.devMode)
                    {
                        PlayerProgressionManager.Rank rank =
                            PlayerProgressionManager.GetClosestAndNextRank(pp.playerDataCell.playerExtendedPublicData.honor)[0];

                        GameManager.instance.carnageReport = new CarnageReport(
                            rank,
                            pp.playerDataCell.playerExtendedPublicData.level,
                            pp.playerDataCell.playerExtendedPublicData.xp, 0,
                            pp.playerDataCell.playerExtendedPublicData.honor, 0, false, 0);
                    }
                }

                if (pp.controllerId == 0)
                {
                    GameManager.instance.LeaveCurrentRoomAndLoadLevelZero();
                    //pp.LeaveLevelButStayInRoom();

                }
            }

            pp.playerUI.scoreboard.OpenScoreboard(true);
        }
    }





    void CreateWinningPlayersList()
    {
        CurrentRoomManager.instance.winningPlayerStructs.Clear();

        if (GameManager.instance.teamMode == GameManager.TeamMode.None)
        {
            if (GameManager.instance.gameType != GameManager.GameType.GunGame)
                foreach (PlayerMultiplayerMatchStats pms in GameManager.instance.GetAllPhotonPlayers().Select(item => item.GetComponent<PlayerMultiplayerMatchStats>()))
                {
                    if (pms.score >= highestScore)
                    {
                        CurrentRoomManager.instance.winningPlayerStructs.Add(new WinningPlayerStruct(pms.player,
                            pms.player.playerSteamId,
                            pms.player.username, pms.player.controllerId,
                            $"score: {pms.score}"));
                    }
                }
            else if (GameManager.instance.gameType == GameManager.GameType.GunGame)
            {
                CurrentRoomManager.instance.winningPlayerStructs.Add(new WinningPlayerStruct(_gunGameWinner,
                    _gunGameWinner.playerSteamId,
                    _gunGameWinner.username, _gunGameWinner.controllerId,
                    "Gun Game winner."));
            }

            if (CurrentRoomManager.instance.winningPlayerStructs.Count > 1)
            {
                isADraw = true;
            }
        }
        else if (GameManager.instance.teamMode == GameManager.TeamMode.Classic)
        {
            if (redTeamScore == blueTeamScore)
            {
                isADraw = true;
            }
            else
            {
                winningTeam = GameManager.Team.Red; if (blueTeamScore > redTeamScore) { winningTeam = GameManager.Team.Blue; }

                if (winningTeam == GameManager.Team.Red)
                {
                    CarnageReportMenu.winningTeam = GameManager.Team.Red;

                    foreach (PlayerMultiplayerMatchStats pms in GameManager.instance.GetAllPhotonPlayers().Select(item => item.GetComponent<PlayerMultiplayerMatchStats>()))
                        if (pms.player.team == GameManager.Team.Red)
                        {
                            CurrentRoomManager.instance.winningPlayerStructs.Add(new WinningPlayerStruct(pms.player,
                                pms.player.playerSteamId,
                                pms.player.username, pms.player.controllerId,
                                ""));
                        }
                }
                else
                {
                    CarnageReportMenu.winningTeam = GameManager.Team.Blue;

                    foreach (PlayerMultiplayerMatchStats pms in GameManager.instance.GetAllPhotonPlayers().Select(item => item.GetComponent<PlayerMultiplayerMatchStats>()))
                        if (pms.player.team == GameManager.Team.Blue)
                        {
                            CurrentRoomManager.instance.winningPlayerStructs.Add(new WinningPlayerStruct(pms.player,
                                pms.player.playerSteamId,
                                pms.player.username, pms.player.controllerId,
                                ""));
                        }
                }
            }
        }
    }

    void SpawnWinnerKillFeeds()
    {
        foreach (Player p in GameManager.GetLocalPlayers())
        {
            Log.Print(() => "SpawnWinnerKillFeeds");
            if (!isADraw)
            {
                if (CurrentRoomManager.instance.leftRoomManually)
                {
                    // do nothing
                }
                else if (GameManager.instance.teamMode == GameManager.TeamMode.None && CurrentRoomManager.instance.winningPlayerStructs.Count > 0)
                {
                    p.killFeedManager.EnterNewFeed($"<color=#31cff9>GAME OVER! {CurrentRoomManager.instance.winningPlayerStructs[0].playerScript.username} wins!");
                }
                else if (GameManager.instance.teamMode == GameManager.TeamMode.Classic)
                {
                    if (winningTeam == GameManager.Team.Red)
                        p.killFeedManager.EnterNewFeed($"<color=#31cff9>GAME OVER! Red Team wins!");
                    else
                        p.killFeedManager.EnterNewFeed($"<color=#31cff9>GAME OVER! Blue Team wins!");
                }
            }
            else
            {
                p.killFeedManager.EnterNewFeed($"<color=#31cff9>DRAW!");
            }
        }
    }





    public struct AddPlayerKillStruct
    {
        // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/readonly

        public readonly int winningPlayerPhotonId;
        public readonly int losingPlayerPhotonId;
        public readonly bool headshot, melee, grenade, nuthshot, stuck;
        public readonly WeaponProperties.KillFeedOutput cleanDamageSource;

        public AddPlayerKillStruct(int winningPlayerPhotonId, int losingPlayerPhotonId, Player.DeathNature kn, WeaponProperties.KillFeedOutput damageSource)
        {
            this.headshot = this.melee = this.grenade = this.nuthshot = this.stuck = false;

            this.winningPlayerPhotonId = winningPlayerPhotonId;
            this.losingPlayerPhotonId = losingPlayerPhotonId;

            this.cleanDamageSource = damageSource;

            if (kn == Player.DeathNature.Groin) this.nuthshot = true;
            else if (kn == Player.DeathNature.Headshot || kn == Player.DeathNature.Sniped) this.headshot = true;
            else if (kn == Player.DeathNature.Melee) this.melee = true;
            else if (kn == Player.DeathNature.Stuck) this.stuck = true;
            else if (kn.ToString().Contains("renade")) this.grenade = true;
        }
    }












    public void StartLootableWeaponRespawn(Vector3 v)
    {
        LootableWeapon lw = lootableWeaponsDict[v];
        int t = GameManager.GetNextTiming((int)lw.tts);

        Debug.Log(t);

        StartCoroutine(LootableWeaponSpawn_Coroutine(lw, t));
    }




    IEnumerator LootableWeaponSpawn_Coroutine(LootableWeapon lw, int t)
    {
        yield return new WaitForSeconds(t);

        lw.gameObject.SetActive(true);
    }
}

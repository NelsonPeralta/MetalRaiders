using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using System;
using System.Linq;
using System.Net.Mail;

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
                return 60;
            if (GameManager.instance.teamMode == GameManager.TeamMode.None)
            {
                //return 10 + (Mathf.Clamp((CurrentRoomManager.instance.nbPlayersJoined - 2) * 5, 0, 15));
                return CurrentRoomManager.instance.expectedNbPlayers * 5;
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
                foreach (Player p in GameManager.instance.pid_player_Dict.Values)
                {
                    if (p && p.GetComponent<PlayerMultiplayerMatchStats>().kills > hs)
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
    public int redTeamScore { get { return _redTeamScore; } private set { _redTeamScore = value; } }
    public int blueTeamScore { get { return _blueTeamScore; } private set { _blueTeamScore = value; } }
    public List<Player> winningPlayers { get { return _winningPlayers; } }
    public List<int> winningPlayersId { get { return _winninPlayersId; } }

    [SerializeField] int _redTeamScore;
    [SerializeField] int _blueTeamScore;

    [SerializeField] List<Player> _winningPlayers = new List<Player>();
    [SerializeField] List<int> _winninPlayersId = new List<int>();

    int _initialRoomPlayercount;

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

        GameManager.instance.OnSceneLoadedEvent -= OnSceneLoaded;
        GameManager.instance.OnSceneLoadedEvent += OnSceneLoaded;
    }

    void OnSceneLoaded()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        Debug.Log("Multiplayer Manager OnSceneLoaded");

        winningPlayers.Clear();
        redTeamScore = 0;
        blueTeamScore = 0;
    }
    public void AddPlayerKill(AddPlayerKillStruct struc)
    {
        try
        {
            PlayerMultiplayerMatchStats winningPlayerMS = GameManager.GetPlayerWithPhotonViewId(struc.winningPlayerPhotonId).GetComponent<PlayerMultiplayerMatchStats>();
            PlayerMultiplayerMatchStats losingPlayerMS = GameManager.GetPlayerWithPhotonViewId(struc.losingPlayerPhotonId).GetComponent<PlayerMultiplayerMatchStats>();

            if (highestScore >= scoreToWin)
                return;


            if (winningPlayerMS != losingPlayerMS)
            {
                if (GameManager.instance.gameType == GameManager.GameType.GunGame)
                    if (winningPlayerMS.player == GameManager.GetLocalPlayer(winningPlayerMS.player.rid))
                    {
                        if (!struc.cleanDamageSource.Contains("elee") && !struc.cleanDamageSource.Contains("renade")
                            && !struc.cleanDamageSource.Contains("tuck"))
                            winningPlayerMS.player.playerInventory.playerGunGameManager.index++;

                        Debug.Log(struc.cleanDamageSource);
                        if (struc.cleanDamageSource.Contains("istol"))
                            NetworkGameManager.instance.EndGame();
                    }

                winningPlayerMS.kills++;

                if (struc.headshot)
                    winningPlayerMS.headshots++;
                if (struc.melee)
                    winningPlayerMS.meleeKills++;
                if (struc.grenade)
                    winningPlayerMS.grenadeKills++;

                if (winningPlayerMS.team == GameManager.Team.Red)
                    redTeamScore++;
                else if (winningPlayerMS.team == GameManager.Team.Blue)
                    blueTeamScore++;
            }
            else
            {
                losingPlayerMS.kills--;

                if (winningPlayerMS.team == GameManager.Team.Red)
                    redTeamScore--;
                else if (winningPlayerMS.team == GameManager.Team.Blue)
                    blueTeamScore--;
            }

            losingPlayerMS.deaths++;

            if (PhotonNetwork.IsMasterClient)
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
        PlayerMultiplayerMatchStats winningPlayerMS = GameManager.GetPlayerWithPhotonViewId(pid).GetComponent<PlayerMultiplayerMatchStats>();
        winningPlayerMS.score++;

        CheckForEndGame();
    }
    public void CheckForEndGame()
    {
        print($"CheckForEndGame {highestScore} / {scoreToWin}");
        if (highestScore == scoreToWin)
            FindObjectOfType<NetworkGameManager>().EndGame();
        //EndGame();
    }
    public void EndGame(bool saveXp = true, bool actuallyQuit = false)
    {
        CurrentRoomManager.instance.gameOver = true;
        print("EndGame");
        foreach (Player pp in FindObjectsOfType<Player>())
        {

            // https://techdifferences.com/difference-between-break-and-continue.html#:~:text=The%20main%20difference%20between%20break,next%20iteration%20of%20the%20loop.
            // return will stop this method, break will stop the loop, continue will stop the current iteration
            if (!pp.PV.IsMine)
                continue;

            if (highestScore >= scoreToWin)
            {
                print($"EndGame {highestScore} / {scoreToWin}");
                if (GameManager.instance.teamMode == GameManager.TeamMode.None)
                {
                    foreach (PlayerMultiplayerMatchStats pms in FindObjectsOfType<PlayerMultiplayerMatchStats>())
                    {
                        print($"EndGame {pms.score} / {scoreToWin}");
                        if (pms.score >= scoreToWin)
                        {
                            pp.GetComponent<KillFeedManager>().EnterNewFeed($"<color=#31cff9>GAME OVER! {pms.GetComponent<Player>().username} wins!");
                            this.winningPlayers.Add(pms.GetComponent<Player>());
                            this.winningPlayersId.Add(pms.GetComponent<Player>().playerId);
                        }
                    }
                }
                else if (GameManager.instance.teamMode == GameManager.TeamMode.Classic)
                {
                    if (redTeamScore >= scoreToWin)
                    {

                        pp.GetComponent<KillFeedManager>().EnterNewFeed($"<color=#31cff9>GAME OVER! Red Team wins!");


                        foreach (PlayerMultiplayerMatchStats pms in FindObjectsOfType<PlayerMultiplayerMatchStats>())
                            if (pms.GetComponent<Player>().team == GameManager.Team.Red)
                            {
                                this.winningPlayersId.Add(pms.GetComponent<Player>().playerId);
                            }
                    }
                    else if (blueTeamScore >= scoreToWin)
                    {

                        pp.GetComponent<KillFeedManager>().EnterNewFeed($"<color=#31cff9>GAME OVER! Blue Team wins!");

                        foreach (PlayerMultiplayerMatchStats pms in FindObjectsOfType<PlayerMultiplayerMatchStats>())
                            if (pms.GetComponent<Player>().team == GameManager.Team.Blue)
                            {
                                this.winningPlayersId.Add(pms.GetComponent<Player>().playerId);
                            }
                    }
                }
            }
            else
            {
                pp.GetComponent<KillFeedManager>().EnterNewFeed($"<color=#31cff9>GAME OVER!");
            }

            if (saveXp)
            {
                if (pp.controllerId == 0)
                {
                    pp.allPlayerScripts.announcer.PlayGameOverClip();
                    WebManager.webManagerInstance.SaveMultiplayerStats(pp.GetComponent<PlayerMultiplayerMatchStats>(), this.winningPlayersId);

                    pp.LeaveRoomWithDelay();
                }
            }
            else
            {
                if (GameManager.instance.connection == GameManager.Connection.Online)
                {
                    PlayerDatabaseAdaptor pda = WebManager.webManagerInstance.pda;
                    PlayerProgressionManager.Rank rank = PlayerProgressionManager.GetClosestAndNextRank(pda.playerBasicOnlineStats.honor)[0];
                    GameManager.instance.carnageReport = new CarnageReport(rank, pda.level, pda.xp, 0, pda.honor, 0, false, 0);
                }

                if (pp.controllerId == 0)
                    GameManager.instance.LeaveRoom();
            }

            pp.playerUI.scoreboard.OpenScoreboard();
        }
    }

    public struct AddPlayerKillStruct
    {
        // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/readonly

        public readonly int winningPlayerPhotonId;
        public readonly int losingPlayerPhotonId;
        public readonly bool headshot;
        public readonly bool melee;
        public readonly bool grenade;
        public readonly string cleanDamageSource;

        public AddPlayerKillStruct(int winningPlayerPhotonId, int losingPlayerPhotonId, Player.DeathNature kn, string damageSource)
        {
            this.headshot = false;
            this.melee = false;
            this.grenade = false;

            this.winningPlayerPhotonId = winningPlayerPhotonId;
            this.losingPlayerPhotonId = losingPlayerPhotonId;

            this.cleanDamageSource = damageSource;

            if (kn == Player.DeathNature.Headshot || kn == Player.DeathNature.Sniped)
                this.headshot = true;
            if (kn == Player.DeathNature.Melee)
                this.melee = true;
            if (kn == Player.DeathNature.Grenade)
                this.grenade = true;
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

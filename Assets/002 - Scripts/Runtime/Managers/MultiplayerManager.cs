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
            if (GameManager.instance.gameType == GameManager.GameType.Hill)
                return 60;
            if (GameManager.instance.teamMode == GameManager.TeamMode.None)
                return 10;
            else if (GameManager.instance.teamMode == GameManager.TeamMode.Classic)
                return 25;

            return 5;
        }
    }
    public int highestScore
    {
        get
        {
            int hs = 0;

            if (GameManager.instance.gameType == GameManager.GameType.GunGame)
            {
                {
                    Debug.Log("highestScore");
                    Debug.Log(GameManager.GetRootPlayer().playerInventory.playerGunGameManager.index);
                }
                return GameManager.GetRootPlayer().playerInventory.playerGunGameManager.index;
            }

            if (GameManager.instance.gameType == GameManager.GameType.Hill)
            {

                foreach(Player p in GameManager.instance.pid_player_Dict.Values)
                {
                    if(p.GetComponent<PlayerMultiplayerMatchStats>().score > hs)
                        hs = p.GetComponent<PlayerMultiplayerMatchStats>().score;
                }
            }


            if (GameManager.instance.teamMode == GameManager.TeamMode.None)
            {
                foreach (Player p in GameManager.instance.pid_player_Dict.Values)
                {
                    if (p.GetComponent<PlayerMultiplayerMatchStats>().kills > hs)
                        hs = p.GetComponent<PlayerMultiplayerMatchStats>().kills;
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

    [SerializeField] int _redTeamScore;
    [SerializeField] int _blueTeamScore;

    [SerializeField] List<Player> _winningPlayers = new List<Player>();

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

                        if (struc.cleanDamageSource.Contains("pistol"))
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

        if (highestScore == scoreToWin)
            FindObjectOfType<NetworkGameManager>().EndGame();
        //EndGame();
    }
    public void EndGame(bool saveXp = true)
    {
        CurrentRoomManager.instance.gameOver = true;
        string winningEntity = "";
        foreach (Player pp in FindObjectsOfType<Player>())
        {
            if (pp.GetComponent<PlayerMultiplayerMatchStats>().kills >= scoreToWin && GameManager.instance.gameType == GameManager.GameType.Slayer)
                winningEntity = pp.PV.Owner.NickName;

            // https://techdifferences.com/difference-between-break-and-continue.html#:~:text=The%20main%20difference%20between%20break,next%20iteration%20of%20the%20loop.
            // return will stop this method, break will stop the loop, continue will stop the current iteration
            if (!pp.PV.IsMine)
                continue;

            if (highestScore >= scoreToWin)
            {
                if (GameManager.instance.teamMode == GameManager.TeamMode.None)
                {
                    foreach (PlayerMultiplayerMatchStats pms in FindObjectsOfType<PlayerMultiplayerMatchStats>())
                        if (pms.score >= scoreToWin)
                        {
                            pp.GetComponent<KillFeedManager>().EnterNewFeed($"GAME OVER! {pms.GetComponent<Player>().username} wins!");
                            winningPlayers.Add(pms.GetComponent<Player>());
                        }
                }
                else if (GameManager.instance.teamMode == GameManager.TeamMode.Classic)
                {
                    if (redTeamScore >= scoreToWin)
                        pp.GetComponent<KillFeedManager>().EnterNewFeed($"GAME OVER! Red Team wins!");
                    else if (blueTeamScore >= scoreToWin)
                        pp.GetComponent<KillFeedManager>().EnterNewFeed($"GAME OVER! Blue Team wins!");
                }
            }
            else
            {
                pp.GetComponent<KillFeedManager>().EnterNewFeed($"GAME OVER!");
            }

            if (saveXp)
            {
                if (pp.controllerId == 0)
                {
                    pp.allPlayerScripts.announcer.PlayGameOverClip();
                    WebManager.webManagerInstance.SaveMultiplayerStats(pp.GetComponent<PlayerMultiplayerMatchStats>(), winningPlayers);

                    pp.LeaveRoomWithDelay();
                }
            }
            else
            {
                PlayerDatabaseAdaptor pda = WebManager.webManagerInstance.pda;
                PlayerProgressionManager.Rank rank = PlayerProgressionManager.GetClosestAndNextRank(pda.playerBasicOnlineStats.honor)[0];
                GameManager.instance.carnageReport = new CarnageReport(rank,pda.level, pda.xp, 0, pda.honor, 0, false, 0);

                GameManager.instance.LeaveRoom();
            }
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

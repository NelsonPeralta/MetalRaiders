using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using System;
using System.Linq;

public class MultiplayerManager : MonoBehaviourPunCallbacks
{
    // public variables
    public static MultiplayerManager instance;

    public int scoreToWin
    {
        get
        {
            if (GameManager.instance.teamMode == GameManager.TeamMode.None)
                return 15;
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
            if (GameManager.instance.teamMode == GameManager.TeamMode.None)
            {
                foreach (PlayerMultiplayerMatchStats pms in FindObjectsOfType<PlayerMultiplayerMatchStats>().ToList())
                {
                    if (pms.kills > hs)
                        hs = pms.kills;
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

    [SerializeField] int _redTeamScore;
    [SerializeField] int _blueTeamScore;

    // private variables
    PhotonView PV;

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
        PV = GetComponent<PhotonView>();

        GameManager.instance.OnSceneLoadedEvent += OnSceneLoaded;
    }

    void OnSceneLoaded()
    {
        Scene currentScene = SceneManager.GetActiveScene();
    }
    public void AddPlayerKill(AddPlayerKillStruct struc)
    {
        PlayerMultiplayerMatchStats winningPlayerMS = GameManager.GetPlayerWithPhotonViewId(struc.winningPlayerPhotonId).GetComponent<PlayerMultiplayerMatchStats>();
        PlayerMultiplayerMatchStats losingPlayerMS = GameManager.GetPlayerWithPhotonViewId(struc.losingPlayerPhotonId).GetComponent<PlayerMultiplayerMatchStats>();

        if (highestScore >= scoreToWin)
            return;

        if (winningPlayerMS != losingPlayerMS)
        {
            winningPlayerMS.kills++;

            if (winningPlayerMS.team == PlayerMultiplayerMatchStats.Team.Red)
                redTeamScore++;
            else if (winningPlayerMS.team == PlayerMultiplayerMatchStats.Team.Blue)
                blueTeamScore++;
        }
        else
        {

        }

        losingPlayerMS.deaths++;

        CheckForEndGame();
    }
    public void CheckForEndGame()
    {
        if (highestScore == scoreToWin)
            EndGame();
    }
    public void EndGame(bool saveXp = true)
    {
        string winningEntity = "";
        foreach (Player pp in FindObjectsOfType<Player>())
        {
            if (pp.GetComponent<PlayerMultiplayerMatchStats>().kills >= scoreToWin && GameManager.instance.gameType == GameManager.GameType.Slayer)
                winningEntity = pp.PV.Owner.NickName;

            // https://techdifferences.com/difference-between-break-and-continue.html#:~:text=The%20main%20difference%20between%20break,next%20iteration%20of%20the%20loop.
            // return will stop this method, break will stop the loop, continue will stop the current iteration
            if (!pp.PV.IsMine)
                continue;
            pp.GetComponent<KillFeedManager>().EnterNewFeed($"GAME OVER! {winningEntity} wins.");

            if (saveXp)
            {
                if (pp.controllerId == 0)
                {
                    pp.allPlayerScripts.announcer.PlayGameOverClip();
                    WebManager.webManagerInstance.SaveMultiplayerStats(pp.GetComponent<PlayerMultiplayerMatchStats>());

                    pp.LeaveRoomWithDelay();
                }
            }
            else
                GameManager.instance.LeaveRoom();
        }
    }

    public struct AddPlayerKillStruct
    {
        // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/readonly

        public readonly int winningPlayerPhotonId;
        public readonly int losingPlayerPhotonId;
        public readonly bool headshot;

        public AddPlayerKillStruct(int winningPlayerPhotonId, int losingPlayerPhotonId, bool headshot)
        {
            this.winningPlayerPhotonId = winningPlayerPhotonId;
            this.losingPlayerPhotonId = losingPlayerPhotonId;
            this.headshot = headshot;
        }
    }
}

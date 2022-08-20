using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using System;

public class MultiplayerManager : MonoBehaviourPunCallbacks
{
    // public variables
    public static MultiplayerManager instance;

    [Header("Score")]
    public int scoreToWin;

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

        if (currentScene.buildIndex > 0) // We are not in the menu
        {
            if (GameManager.instance.gameMode != GameManager.GameMode.Multiplayer)
                return;

            instance = this;

            //if ((GameManager.instance.multiplayerMode == GameManager.MultiplayerMode.Slayer) || )
                scoreToWin = 10;

            
        }
        else // We are in the menu
        {
            scoreToWin = 0;
        }
    }
    public void AddPlayerKill(AddPlayerKillStruct struc)
    {
        PlayerMultiplayerMatchStats winningPlayerMS = GameManager.GetPlayerWithPhotonViewId(struc.winningPlayerPhotonId).GetComponent<PlayerMultiplayerMatchStats>();
        PlayerMultiplayerMatchStats losingPlayerMS = GameManager.GetPlayerWithPhotonViewId(struc.losingPlayerPhotonId).GetComponent<PlayerMultiplayerMatchStats>();


        List<Player> allPlayers = new List<Player>();
        foreach (Player pp in FindObjectsOfType<Player>())
            allPlayers.Add(pp);



        if (winningPlayerMS != losingPlayerMS)
        {
            winningPlayerMS.kills++;
            //foreach (Player pp in allPlayers)
            //    if (pp.PV.IsMine && pp)
            //    {
            //        KillFeedManager killFeedManager = GetComponent<KillFeedManager>();
            //        int damageSourceSpriteCode = KillFeedManager.killFeedWeaponCodeDict[damageSource];
            //        string colorCode = KillFeedManager.killFeedColorCodeDict["orange"];

            //        killFeedManager.EnterNewFeed($"You took {damage} <sprite={damageSourceSpriteCode} color={colorCode}> damage");
            //    }
        }
        else
        {
            //foreach (Player pp in allPlayers)
            //    if (pp.PV.IsMine && pp)
            //        pp.allPlayerScripts.killFeedManager.EnterNewFeed($"{losingPlayerMS.playerName} committed suicide");
        }
        losingPlayerMS.deaths++;

        CheckForEndGame();
    }
    public void CheckForEndGame()
    {
        foreach (PlayerMultiplayerMatchStats pms in FindObjectsOfType<PlayerMultiplayerMatchStats>())
            if (pms.kills >= scoreToWin)
                EndGame();
    }
    public void EndGame(bool saveXp = true)
    {
        string winningEntity = "";
        foreach (Player pp in FindObjectsOfType<Player>())
        {
            if (pp.GetComponent<PlayerMultiplayerMatchStats>().kills >= scoreToWin && GameManager.instance.multiplayerMode == GameManager.MultiplayerMode.Slayer)
                winningEntity = pp.PV.Owner.NickName;

            // https://techdifferences.com/difference-between-break-and-continue.html#:~:text=The%20main%20difference%20between%20break,next%20iteration%20of%20the%20loop.
            // return will stop this method, break will stop the loop, continue will stop the current iteration
            if (!pp.PV.IsMine)
                continue;


            if (saveXp)
            {
                pp.allPlayerScripts.announcer.PlayGameOverClip();
                pp.GetComponent<KillFeedManager>().EnterNewFeed($"GAME OVER! {winningEntity} wins.");
                WebManager.webManagerInstance.SaveMultiplayerStats(pp.GetComponent<PlayerMultiplayerMatchStats>());
                pp.LeaveRoomWithDelay();

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

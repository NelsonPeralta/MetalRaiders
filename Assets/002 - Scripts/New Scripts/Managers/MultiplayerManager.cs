using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

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
            {
                //enabled = false;
                return;
            }

            if (GameManager.instance.multiplayerMode == GameManager.MultiplayerMode.Deathmatch)
                scoreToWin = 10;
        }
        else // We are in the menu
        {
            scoreToWin = 0;
        }
    }
    public void AddPlayerKill(AddPlayerKillStruct struc)
    {
        PlayerMultiplayerStats winningPlayerMS = GameManager.instance.GetPlayerWithPhotonViewId(struc.winningPlayerPhotonId).GetComponent<PlayerMultiplayerStats>();
        PlayerMultiplayerStats losingPlayerMS = GameManager.instance.GetPlayerWithPhotonViewId(struc.losingPlayerPhotonId).GetComponent<PlayerMultiplayerStats>();


        List<PlayerProperties> allPlayers = new List<PlayerProperties>();
        foreach (PlayerProperties pp in FindObjectsOfType<PlayerProperties>())
            allPlayers.Add(pp);



        if (winningPlayerMS != losingPlayerMS)
        {
            winningPlayerMS.AddKill();
            foreach (PlayerProperties pp in allPlayers)
                if (pp.PV.IsMine && pp)
                    pp.allPlayerScripts.killFeedManager.EnterNewFeed(winningPlayerMS.playerName, losingPlayerMS.playerName, struc.headshot);
        }
        else
        {
            foreach (PlayerProperties pp in allPlayers)
                if (pp.PV.IsMine && pp)
                    pp.allPlayerScripts.killFeedManager.EnterNewFeed($"{losingPlayerMS.playerName} committed suicide");
        }
        losingPlayerMS.AddDeath();

        CheckForEndGame();
    }
    public void CheckForEndGame()
    {
        foreach (PlayerMultiplayerStats pms in FindObjectsOfType<PlayerMultiplayerStats>())
            if (pms.kills >= scoreToWin)
                EndGame();
    }
    public void EndGame()
    {
        string winningEntity = "";
        foreach (PlayerProperties pp in FindObjectsOfType<PlayerProperties>())
        {
            if (pp.GetComponent<PlayerMultiplayerStats>().kills >= scoreToWin && GameManager.instance.multiplayerMode == GameManager.MultiplayerMode.Deathmatch)
                winningEntity = pp.PV.Owner.NickName;

            // https://techdifferences.com/difference-between-break-and-continue.html#:~:text=The%20main%20difference%20between%20break,next%20iteration%20of%20the%20loop.
            // return will stop this method, break will stop the loop, continue will stop the current iteration
            if (!pp.PV.IsMine)
                continue;
            WebManager.webManagerInstance.SaveMultiplayerStats(pp.GetComponent<PlayerMultiplayerStats>());

            pp.allPlayerScripts.announcer.PlayGameOverClip();
            pp.LeaveRoomWithDelay();
            pp.allPlayerScripts.killFeedManager.EnterNewFeed($"GAME OVER! {winningEntity} wins.");

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

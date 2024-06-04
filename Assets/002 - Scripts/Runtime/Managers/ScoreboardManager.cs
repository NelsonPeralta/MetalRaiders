using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ScoreboardManager : MonoBehaviour
{
    [SerializeField] Player _player;
    [SerializeField] GameObject _scoreboardHolder, _teamScoreboard, _multiplayerHeaders, _swarmHeaders, _winnerWitness, _loserWitness;
    [SerializeField] TMPro.TMP_Text redTeamText;
    [SerializeField] TMPro.TMP_Text blueTeamText;


    public List<ScoreboardRowRuntime> scoreboardRows = new List<ScoreboardRowRuntime>();


    // Private Variables
    bool scoreboardOpen;

    private void Awake()
    {
    }

    private void Start()
    {

        _scoreboardHolder.SetActive(false);
    }

    public void OpenScoreboard()
    {
        _player.playerUI.ToggleUIExtremities(false);
        UpdateScoreboard();
        ToggleTeamScoreboard(GameManager.instance.teamMode == GameManager.TeamMode.Classic);
        if (!scoreboardOpen)
        {
            _scoreboardHolder.SetActive(true);
            scoreboardOpen = true;

            _multiplayerHeaders.SetActive(GameManager.instance.gameMode == GameManager.GameMode.Multiplayer);
            _swarmHeaders.SetActive(GameManager.instance.gameMode == GameManager.GameMode.Swarm);
        }

        _winnerWitness.SetActive(CurrentRoomManager.instance.gameOver && MultiplayerManager.instance.winningPlayers.Contains(_player) && !CurrentRoomManager.instance.leftRoomManually);
        _loserWitness.SetActive(CurrentRoomManager.instance.gameOver && !MultiplayerManager.instance.winningPlayers.Contains(_player) && !CurrentRoomManager.instance.leftRoomManually);
    }

    public void CloseScoreboard()
    {
        if (!CurrentRoomManager.instance.gameOver)
        {
            _player.playerUI.ToggleUIExtremities(true);

            CloseTeamScoreboard();
            _scoreboardHolder.SetActive(false);
            scoreboardOpen = false;
        }
    }

    public void UpdateScoreboard()
    {
        DisableAllRows();
        MultiplayerManager multiplayerManager = MultiplayerManager.instance;
        //onlineSwarmManager = OnlineSwarmManager.onlineSwarmManagerInstance;

        if (!multiplayerManager)
            multiplayerManager = FindObjectOfType<MultiplayerManager>();

        //if (!onlineSwarmManager)
        //    onlineSwarmManager = FindObjectOfType<OnlineSwarmManager>();

        if (GameManager.instance.gameMode == GameManager.GameMode.Multiplayer)
        {
            List<PlayerMultiplayerMatchStats> allPlayersMS = new List<PlayerMultiplayerMatchStats>();

            foreach (GameObject go in GameObject.FindGameObjectsWithTag("Player"))
                allPlayersMS.Add(go.GetComponent<PlayerMultiplayerMatchStats>());

            for (int i = 0; i < allPlayersMS.Count; i++)
            {
                scoreboardRows[i].playerScoreStruct = allPlayersMS[i].player.playerDataCell;
                //scoreboardRows[i].playerNameText.text = allPlayersMS[i].GetComponent<Player>().username;
                //scoreboardRows[i].playerKillsText.text = allPlayersMS[i].score.ToString();
                //scoreboardRows[i].playerDeathsText.text = allPlayersMS[i].deaths.ToString();
                //scoreboardRows[i].playerHeadshotsText.text = $"{allPlayersMS[i].headshots.ToString()}/{allPlayersMS[i].meleeKills}/{allPlayersMS[i].grenadeKills}";
                //scoreboardRows[i].playerHeadshotsText.text = $"{allPlayersMS[i].headshots}";
                //scoreboardRows[i].playerTotalDamageText.text = allPlayersMS[i].damage.ToString();
                //scoreboardRows[i].team = allPlayersMS[i].team;
                //if (allPlayersMS[i].deaths > 0)
                //    scoreboardRows[i].playerCurrentPointsText.text = (allPlayersMS[i].kills / (float)allPlayersMS[i].deaths).ToString();
                //else
                //    scoreboardRows[i].playerCurrentPointsText.text = "0";


                scoreboardRows[i].gameObject.SetActive(true);
            }
        }
        else if (GameManager.instance.gameMode == GameManager.GameMode.Swarm)
        {
            List<PlayerSwarmMatchStats> allPlayersMS = new List<PlayerSwarmMatchStats>();

            try
            {
                foreach (GameObject go in GameObject.FindGameObjectsWithTag("player"))
                    allPlayersMS.Add(go.GetComponent<PlayerSwarmMatchStats>());
            }
            catch { }

            for (int i = 0; i < allPlayersMS.Count; i++)
            {
                scoreboardRows[i].playerScoreStruct = allPlayersMS[i].player.playerDataCell;
                //scoreboardRows[i].playerNameText.text = allPlayersMS[i].GetComponent<Player>().username;
                //scoreboardRows[i].playerKillsText.text = allPlayersMS[i].kills.ToString();
                //scoreboardRows[i].playerDeathsText.text = allPlayersMS[i].deaths.ToString();
                //scoreboardRows[i].playerHeadshotsText.text = allPlayersMS[i].headshots.ToString();
                //scoreboardRows[i].playerCurrentPointsText.text = allPlayersMS[i].points.ToString();

                scoreboardRows[i].gameObject.SetActive(true);
            }
        }
    }

    void DisableAllRows()
    {
        foreach (ScoreboardRowRuntime sr in scoreboardRows)
            sr.gameObject.SetActive(false);
    }

    void ToggleTeamScoreboard(bool t)
    {
        try { _teamScoreboard.SetActive(t); } catch { }

        try
        {
            redTeamText.text = $"Red: {MultiplayerManager.instance.redTeamScore}";
            blueTeamText.text = $"Blue: {MultiplayerManager.instance.blueTeamScore}";
        }
        catch { }
    }

    void CloseTeamScoreboard()
    {
        try { _teamScoreboard.SetActive(false); } catch { }
    }
}

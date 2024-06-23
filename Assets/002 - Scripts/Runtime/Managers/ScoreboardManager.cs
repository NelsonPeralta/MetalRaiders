using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;

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
        DisableAllRows();



        _scoreboardHolder.SetActive(false);
    }

    public void OpenScoreboard()
    {
        _player.playerUI.ToggleUIExtremities(false);

        foreach (var row in scoreboardRows) { row.gameObject.SetActive(row.playerScoreStruct); }


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

    public void SetScoreboardRows()
    {
        print($"SetScoreboardRows {GameManager.instance.pid_player_Dict.Values.Count}");
        for (int i = 0; i < GameManager.instance.pid_player_Dict.Values.Count; i++)
        {
            scoreboardRows[i].playerScoreStruct = GameManager.instance.pid_player_Dict.ElementAt(i).Value.playerDataCell;
            scoreboardRows[i].gameObject.SetActive(true);
        }
    }
}

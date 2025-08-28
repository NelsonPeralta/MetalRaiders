using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;
using System;

public class ScoreboardManager : MonoBehaviour
{
    [SerializeField] Player _player;
    [SerializeField] GameObject _scoreboardHolder, _teamScoreboard, _multiplayerHeaders, _swarmHeaders, _winnerWitness, _loserWitness, _drawWitness;
    [SerializeField] TMPro.TMP_Text redTeamText;
    [SerializeField] TMPro.TMP_Text blueTeamText;


    public List<ScoreboardRowRuntime> scoreboardRows = new List<ScoreboardRowRuntime>();
    public bool scoreboardOpen { get { return _scoreboardOpen; } }


    Dictionary<int, int> _sorted = new Dictionary<int, int>();



    // Private Variables
    [SerializeField] bool _scoreboardOpen;


    int _bc, _c;



    private void Awake()
    {
        //print($"ScoreboardManager Awake {GetComponentsInChildren<ScoreboardRowRuntime>(includeInactive: true).Length}");
        //scoreboardRows.Clear();
        //scoreboardRows = GetComponentsInChildren<ScoreboardRowRuntime>(includeInactive: true).ToList(); // this gets 40 instead of 8
    }

    private void Start()
    {
        DisableAllRows();


        _winnerWitness.SetActive(false);
        _loserWitness.SetActive(false);
        _drawWitness.SetActive(false);
        _scoreboardHolder.SetActive(false);
    }

    public void OpenScoreboard(bool triggerEndGameBehaviour = false)
    {
        _player.playerUI.ToggleUIExtremities(false);

        foreach (var row in scoreboardRows) { row.gameObject.SetActive(row.playerScoreStruct); }


        ToggleTeamScoreboard(GameManager.instance.teamMode == GameManager.TeamMode.Classic);
        if (!_scoreboardOpen)
        {
            SortScoreBoardByScore();





            _scoreboardHolder.SetActive(true);
            _scoreboardOpen = true;

            _multiplayerHeaders.SetActive(GameManager.instance.gameMode == GameManager.GameMode.Versus);
            _swarmHeaders.SetActive(GameManager.instance.gameMode == GameManager.GameMode.Coop);
        }

        if (CurrentRoomManager.instance.gameOver)
        {
            if (!MultiplayerManager.instance.isADraw)
            {
                List<long> steamIds = CurrentRoomManager.instance.winningPlayerStructs.Select(player => player.steamId).ToList();


                _winnerWitness.SetActive(CurrentRoomManager.instance.gameOver && steamIds.Contains(_player.playerSteamId) && !CurrentRoomManager.instance.leftRoomManually);
                _loserWitness.SetActive(CurrentRoomManager.instance.gameOver && !steamIds.Contains(_player.playerSteamId) && !CurrentRoomManager.instance.leftRoomManually);
            }
            else
            {
                _winnerWitness.SetActive(false);
                _loserWitness.SetActive(false);

                if (!CurrentRoomManager.instance.leftRoomManually) _drawWitness.SetActive(true); else _drawWitness.SetActive(false);
            }
        }

        if (triggerEndGameBehaviour) _player.playerUI.ShowBlackScreenForEndOfGame();
    }

    public void CloseScoreboard()
    {
        if (!CurrentRoomManager.instance.gameOver)
        {
            _player.playerUI.ToggleUIExtremities(true);

            CloseTeamScoreboard();
            _scoreboardHolder.SetActive(false);
            _scoreboardOpen = false;
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
        print($"SetScoreboardRows {GameManager.instance.GetAllPhotonPlayers().Count}");
        for (int i = 0; i < GameManager.instance.GetAllPhotonPlayers().Count; i++)
        {
            if (CurrentRoomManager.instance.playerDataCells[i].occupied)
            {
                scoreboardRows[i].playerScoreStruct = CurrentRoomManager.instance.playerDataCells[i];
                scoreboardRows[i].gameObject.SetActive(true);
            }
        }
    }

    public void UpdateTeamScores()
    {
        try
        {
            redTeamText.text = $"Red: {MultiplayerManager.instance.redTeamScore}";
            blueTeamText.text = $"Blue: {MultiplayerManager.instance.blueTeamScore}";
        }
        catch { }
    }

    public void SortScoreBoardByScore()
    {
        _sorted.Clear();

        foreach (var row in scoreboardRows)
        {
            if (row && row.playerScoreStruct)
            {
                print($"ScoreboardManager adding {Array.IndexOf(CurrentRoomManager.instance.playerDataCells.ToArray(), row.playerScoreStruct)}  {row.playerScoreStruct.playerCurrentGameScore.score}");
                _sorted.Add(Array.IndexOf(CurrentRoomManager.instance.playerDataCells.ToArray(), row.playerScoreStruct), row.playerScoreStruct.playerCurrentGameScore.score);
            }
        }

        _sorted = _sorted.OrderByDescending(pair => pair.Value).ToDictionary(pair => pair.Key, pair => pair.Value);

        GameManager.Team _winningTeam = GameManager.Team.Red; if (!MultiplayerManager.instance.isADraw) { _winningTeam = MultiplayerManager.instance.winningTeam; }

        if (GameManager.instance.gameMode == GameManager.GameMode.Versus)
        {
            for (int i = 0; i < _sorted.Count; i++)
            {
                foreach (var row in scoreboardRows)
                {
                    if (row && row.playerScoreStruct)
                    {
                        if (Array.IndexOf(CurrentRoomManager.instance.playerDataCells.ToArray(), row.playerScoreStruct) == _sorted.ElementAt(i).Key)
                        {
                            print($"{row.playerScoreStruct.playerExtendedPublicData.username} is equal to key: {_sorted.ElementAt(i).Key}");

                            if (GameManager.instance.teamMode == GameManager.TeamMode.None)
                                row.transform.SetAsLastSibling();
                            else
                            {
                                if (row.playerScoreStruct.team == _winningTeam)
                                    row.transform.SetAsLastSibling();
                            }
                        }
                    }
                }
            }


            if (GameManager.instance.teamMode == GameManager.TeamMode.Classic)
            {
                for (int i = 0; i < _sorted.Count; i++)
                {
                    foreach (var row in scoreboardRows)
                    {
                        if (row && row.playerScoreStruct)
                        {
                            if (Array.IndexOf(CurrentRoomManager.instance.playerDataCells.ToArray(), row.playerScoreStruct) == _sorted.ElementAt(i).Key)
                            {
                                if (row.playerScoreStruct.team != _winningTeam)
                                    row.transform.SetAsLastSibling();
                            }
                        }
                    }
                }
            }
        }
    }
}

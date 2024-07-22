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

            _multiplayerHeaders.SetActive(GameManager.instance.gameMode == GameManager.GameMode.Versus);
            _swarmHeaders.SetActive(GameManager.instance.gameMode == GameManager.GameMode.Coop);
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
        print($"SetScoreboardRows {GameManager.instance.GetAllPhotonPlayers().Count}");
        for (int i = 0; i < CurrentRoomManager.instance.expectedNbPlayers; i++)
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

    void SortScoreBoardByScore()
    {
        //if(GameManager.instance.teamMode == GameManager.TeamMode.None)
        //{

        //}







        //int c = 0;

        //for (int i = 0; i < scoreboardRows.Count; i++)
        //{
        //    if (scoreboardRows[i].playerScoreStruct.team == GameManager.instance.)
        //    {
        //        print($"{_carnageReportStrucs[i].playerName} {_carnageReportStrucs[i].team} {winningTeam}");
        //        ColorUtility.TryParseHtmlString(_carnageReportStrucs[i].team.ToString().ToLower(), out _tCol);
        //        carnageReportRowArray[c].mainColor.color = new Color(_tCol.r, _tCol.g, _tCol.b, 1);


        //        carnageReportRowArray[c].playerName.text = _carnageReportStrucs[i].playerName.ToString();
        //        carnageReportRowArray[c].kills.text = _carnageReportStrucs[i].kills.ToString();
        //        carnageReportRowArray[c].deaths.text = _carnageReportStrucs[i].deaths.ToString();
        //        carnageReportRowArray[c].damage.text = _carnageReportStrucs[i].damage.ToString();
        //        carnageReportRowArray[c].score.text = _carnageReportStrucs[i].score.ToString();
        //        carnageReportRowArray[c].headshots.text = _carnageReportStrucs[i].headshots.ToString();


        //        if (_carnageReportStrucs[i].deaths > 0)
        //            carnageReportRowArray[c].kdr.text = $"{_carnageReportStrucs[i].kills / (float)_carnageReportStrucs[i].deaths}";
        //        else
        //            carnageReportRowArray[c].kdr.text = "0";


        //        carnageReportRowArray[c].gameObject.SetActive(true); c++;
        //    }
        //}

        //for (int i = 0; i < _carnageReportStrucs.Count; i++)
        //{
        //    if (_carnageReportStrucs[i].team != winningTeam)
        //    {
        //        print($"{_carnageReportStrucs[i].playerName} {_carnageReportStrucs[i].team} {winningTeam}");
        //        ColorUtility.TryParseHtmlString(_carnageReportStrucs[i].team.ToString().ToLower(), out _tCol);
        //        carnageReportRowArray[c].mainColor.color = new Color(_tCol.r, _tCol.g, _tCol.b, 1);


        //        carnageReportRowArray[c].playerName.text = _carnageReportStrucs[i].playerName.ToString();
        //        carnageReportRowArray[c].kills.text = _carnageReportStrucs[i].kills.ToString();
        //        carnageReportRowArray[c].deaths.text = _carnageReportStrucs[i].deaths.ToString();
        //        carnageReportRowArray[c].damage.text = _carnageReportStrucs[i].damage.ToString();
        //        carnageReportRowArray[c].score.text = _carnageReportStrucs[i].score.ToString();
        //        carnageReportRowArray[c].headshots.text = _carnageReportStrucs[i].headshots.ToString();


        //        if (_carnageReportStrucs[i].deaths > 0)
        //            carnageReportRowArray[c].kdr.text = $"{_carnageReportStrucs[i].kills / (float)_carnageReportStrucs[i].deaths}";
        //        else
        //            carnageReportRowArray[c].kdr.text = "0";


        //        carnageReportRowArray[c].gameObject.SetActive(true); c++;
        //    }
        //}
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public struct CarnageReportStruc
{
    public string playerName { get { return _playerName; } }
    public string colorPalette { get { return _colorPalet; } }
    public int kills { get { return _kills; } }
    public int deaths { get { return _deaths; } }
    public int damage { get { return _damage; } }
    public int score { get { return _score; } }
    public int headshots { get { return _headshots; } }
    public GameManager.Team team { get { return _team; } }


    [SerializeField] string _playerName, _colorPalet;
    [SerializeField] int _kills, _deaths, _damage, _score, _headshots, _nutshots, _meleeKills, _grenadeKills, _stickyKills, _points, _totalPoints;
    [SerializeField] GameManager.Team _team;


    public CarnageReportStruc(PlayerCurrentGameScore score, string _n, string colorPal, GameManager.Team t)
    {
        _playerName = _n;
        _colorPalet = colorPal;
        _kills = score.kills;
        _deaths = score.deaths;
        _damage = score.damage;
        _score = score.score;
        _headshots = score.headshots;
        _nutshots = score.nutshots;
        _meleeKills = score.meleeKills;
        _grenadeKills = score.grenadeKills;
        _stickyKills = score.stickyKills;
        _points = score.points;
        _totalPoints = score.totalPoints;
        _team = t;
    }
}

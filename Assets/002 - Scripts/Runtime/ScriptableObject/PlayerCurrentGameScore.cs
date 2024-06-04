using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerCurrentGameScore
{
    public int score
    {
        //set { _score = value; }

        get
        {
            return _score;


            if (GameManager.instance.gameType == GameManager.GameType.Hill
                || GameManager.instance.gameType == GameManager.GameType.Oddball
                || GameManager.instance.gameType == GameManager.GameType.GunGame)
                return _score;
            else
                return _kills;
        }
    }

    public int kills
    {
        get { return _kills; }
        set
        {
            _kills = value;
        }
    }


    public int points
    {
        get { return _points; }
        set
        {
            _points = value;
        }
    }
    public int totalPoints
    {
        get { return _totalPoints; }
        set
        {
            _totalPoints = value;
        }
    }


    public int deaths, damage, headshots, nutshots, meleeKills, grenadeKills, stickyKills;
    public float kd;

    [SerializeField] int _score, _kills, _points, _totalPoints;




    public void ChangeScore(int s)
    {
        _score = s;
    }
}

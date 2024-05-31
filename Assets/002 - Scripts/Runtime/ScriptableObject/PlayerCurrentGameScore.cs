using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerCurrentGameScore
{
    public int score
    {
        set { _score = value; }

        get
        {
            if (GameManager.instance.gameType == GameManager.GameType.Hill
                || GameManager.instance.gameType == GameManager.GameType.Oddball
                || GameManager.instance.gameType == GameManager.GameType.GunGame)
                return _score;
            else
                return kills;
        }
    }


    public int kills, deaths, damage, headshots, nutshots, meleeKills, grenadeKills, stickyKills;
    public float kd;

    [SerializeField] int _score;
}

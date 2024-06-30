using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarnageReport 
{
    public PlayerProgressionManager.Rank rank { get { return _rank; } }
    public int playerLevel { get { return _pLvl; } }
    public int xpGained { get { return _xpGained; } }
    public int currentXp { get { return _currentXp; } }

    public int honorGained
    {
        get { return _honorGained; }
        set { _currentHonor = value; }
    }

    public bool leveledUp { get { return _leveledUp; } }

    PlayerProgressionManager.Rank _rank;
    public int currentHonor { get { return _currentHonor; } }
    public int newLevel { get { return _newLevel; } }
    public bool rankedUp { get { return _rankedUp; } }

    int _xpGained, _currentXp, _honorGained, _currentHonor, _pLvl, _newLevel;
    bool _leveledUp, _rankedUp;

    public CarnageReport(PlayerProgressionManager.Rank prank, int pLvl, int curXp, int gxp, int curH, int gh, bool lvledUp, int nLevel)
    {
        _rank = prank;
        _pLvl = pLvl;
        _xpGained = gxp; _currentXp = curXp;
        _leveledUp = lvledUp;
        _newLevel = nLevel;

        _honorGained = gh; _currentHonor = curH;

        _rankedUp = (_honorGained + _currentHonor) >= PlayerProgressionManager.GetClosestAndNextRank(currentHonor)[1].honorRequired;
    }
}

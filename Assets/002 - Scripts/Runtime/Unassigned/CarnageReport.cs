using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarnageReport : MonoBehaviour
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

    PlayerProgressionManager.Rank _rank;
    public int currentHonor { get {  return _currentHonor; } }


    int _xpGained, _currentXp, _honorGained, _currentHonor, _pLvl;

    public CarnageReport(PlayerProgressionManager.Rank prank,int pLvl, int curXp, int gxp, int curH, int gh)
    {
        _rank = prank;
        _pLvl = pLvl;
        _xpGained = gxp; _currentXp = curXp;

        _honorGained = gh; _currentHonor = curH;
    }
}

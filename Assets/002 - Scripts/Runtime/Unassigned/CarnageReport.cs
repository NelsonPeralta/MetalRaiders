using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarnageReport : MonoBehaviour
{
    public int xpGained { get { return _xpGained; } }
    public int finalXp { get { return _finalXp; } }

    public int honorGained
    {
        get { return _honorGained; }
        set { _finalHonor = value; }
    }

    public int finalHonor { get {  return _finalHonor; } }


    int _xpGained, _finalXp, _honorGained, _finalHonor;

    public CarnageReport(int xpg, int fxp, int hg, int fh)
    {
        _xpGained = xpg; _finalXp = fxp;

        _honorGained = hg; _finalHonor = fh;
    }
}

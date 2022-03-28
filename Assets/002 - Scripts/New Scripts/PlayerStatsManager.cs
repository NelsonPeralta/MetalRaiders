using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatsManager : MonoBehaviour
{
    public delegate void PlayerStatsManagerEvent(PlayerStatsManager playerStatsManager);
    // Events
    public PlayerStatsManagerEvent OnLevelChanged;
    public PlayerStatsManagerEvent OnXpChanged;
    public PlayerStatsManagerEvent OnCrChanged;

    // private variables
    int _level;
    int _xp;
    int _cr;

    public int level
    {
        get
        {
            return _level;
        }
        set
        {

        }
    }

    public int xp
    {
        get
        {
            return _xp;
        }
        set
        {

        }
    }

    public int cr
    {
        get
        {
            return _cr;
        }
        set
        {

        }
    }
}

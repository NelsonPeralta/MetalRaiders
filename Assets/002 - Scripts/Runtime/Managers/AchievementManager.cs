using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager instance;






    [SerializeField] int _sticksInCurrentGame = 0;
    [SerializeField] int _plamaKillsInCurrentGame = 0;
    [SerializeField] bool _gotAKillByBlowingUpABarrel, _gotANutshotKill;




    public int stickiesThisGame
    {
        get { return _sticksInCurrentGame; }
        set
        {
            _sticksInCurrentGame = value;


            if (_sticksInCurrentGame == 3)
            {
                Steamworks.SteamUserStats.GetAchievement("STICKY_FINGERS", out _achUnlocked);

                if (!_achUnlocked)
                {
                    Debug.Log("UNLOCKED ACHIVEMENT STICKY_FINGERS");
                    UnlockAchievement("STICKY_FINGERS");
                }
            }
        }
    }

    public int plasmaKillsInThisGame
    {
        get { return _plamaKillsInCurrentGame; }
        set
        {
            _plamaKillsInCurrentGame = value;


            if (_plamaKillsInCurrentGame == 5)
            {
                Steamworks.SteamUserStats.GetAchievement("MARTIAN_HANDS", out _achUnlocked);

                if (!_achUnlocked)
                {
                    Debug.Log("UNLOCKED ACHIVEMENT Martian Hands");
                    UnlockAchievement("MARTIAN_HANDS");
                }
            }
        }
    }

    public bool gotAKillByBlowingUpABarrel
    {
        get { return _gotAKillByBlowingUpABarrel; }
        set
        {
            _gotAKillByBlowingUpABarrel = value;


            if (value)
            {
                Steamworks.SteamUserStats.GetAchievement("HZ", out _achUnlocked);

                if (!_achUnlocked)
                {
                    Debug.Log("UNLOCKED ACHIVEMENT Hazard");
                    UnlockAchievement("HZ");
                }
            }
        }
    }


    public bool gotANutshotKill
    {
        get { return _gotANutshotKill; }
        set
        {
            _gotANutshotKill = value;


            if (value)
            {
                Steamworks.SteamUserStats.GetAchievement("RWIH", out _achUnlocked);

                if (!_achUnlocked)
                {
                    Debug.Log("UNLOCKED ACHIVEMENT RWIH");
                    UnlockAchievement("RWIH");
                }
            }
        }
    }









    float _achievCheckDelay;
    bool _achUnlocked = false;



    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {

    }


    void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        _sticksInCurrentGame = _plamaKillsInCurrentGame = 0;
        _gotAKillByBlowingUpABarrel = _gotANutshotKill = false;
    }


    public static void UnlockAchievement(string an)
    {
        Log.Print($"UnlockAchievement {an}");
        Steamworks.SteamUserStats.SetAchievement(an);
        Steamworks.SteamUserStats.StoreStats();
    }

    public static void ResetTempAchivementVariables()
    {

    }
}

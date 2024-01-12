using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager instance;






    [SerializeField] int _sticksInCurrentGame = 0;

    float _achievCheckDelay;
    bool _achUnlocked = false;


    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        bool tutorialCompleted;
        Steamworks.SteamUserStats.GetAchievement("tutorial", out tutorialCompleted);
        if (tutorialCompleted == false)
        {
            Steamworks.SteamUserStats.SetAchievement("tutorial");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_achievCheckDelay > 0)
        {
            _achievCheckDelay -= Time.deltaTime;

            if (_achievCheckDelay < 0)
            {
                if (_sticksInCurrentGame >= 3)
                {
                    Steamworks.SteamUserStats.GetAchievement("STICKY_FINGERS", out _achUnlocked);

                    if (!_achUnlocked)
                    {
                        Debug.Log("UNLOCKED ACHIVEMENT STICKY_FINGERS");
                        //UnlockAchievement("STICKY_FINGERS");
                    }
                }


                _achievCheckDelay = 0.5f;
            }
        }
    }



    public static void UnlockAchievement(string an)
    {
        Steamworks.SteamUserStats.SetAchievement(an);
    }

    public static void ResetTempAchivementVariables()
    {

    }
}

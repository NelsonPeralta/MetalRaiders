using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;

public class PlayerMotionTracker : MonoBehaviourPun
{
    public Player player;
    public Camera minimapCamera;
    [SerializeField] MotionTrackerDot[] _motionTrackerDotsList;




    private void Awake()
    {
        _motionTrackerDotsList = GetComponentsInChildren<MotionTrackerDot>();
    }

    public void Setup()
    {
        //print($"PlayerMotionTracker for {player.name}: Setup {_motionTrackerDotsList.Length} {GameManager.instance.GetAllPhotonPlayers().Count}");

        for (int i = 0; i < _motionTrackerDotsList.Length; i++)
        {
            if (i < 8)
            {
                //print($"PlayerMotionTracker for {player.name}: Setup {_motionTrackerDotsList[i].targetPlayerController}");

                if (!_motionTrackerDotsList[i].targetPlayerController)
                {
                    if (i < GameManager.instance.GetAllPhotonPlayers().Count)
                    {
                        print($"PlayerMotionTracker Setup {i} : {GameManager.instance.GetAllPhotonPlayers().ElementAt(i).name}");
                        _motionTrackerDotsList[i].biped = GameManager.instance.GetAllPhotonPlayers().ElementAt(i).GetComponent<Biped>();

                        if (_motionTrackerDotsList[i].biped == null) Debug.LogError($"Could not setup MT {i}. There is no Player in GetAllPhotonPlayers({i})");
                    }
                }
            }
            else
            {
                if (GameManager.instance.gameMode == GameManager.GameMode.Coop)
                {
                    if (SwarmManager.instance.actorsAliveList.Count > 0 && (i - 8 < SwarmManager.instance.actorsAliveList.Count))
                    {
                        print($"PlayerMotionTracker assigning actor {i - 8} for MT {i}. Total Actors: {SwarmManager.instance.actorsAliveList.Count}");
                        _motionTrackerDotsList[i].biped = SwarmManager.instance.actorsAliveList[i - 8];
                        if (_motionTrackerDotsList[i].biped == null) Debug.LogError($"Could not setup MT {i}");
                    }
                }
            }
        }
    }
}

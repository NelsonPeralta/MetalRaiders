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
        for (int i = 0; i < _motionTrackerDotsList.Length; i++)
        {
            if (i < 8)
            {
                if (!_motionTrackerDotsList[i].targetPlayerController)
                {
                    if (i < GameManager.instance.pid_player_Dict.Count)
                    {
                        print($"PlayerMotionTracker Setup {i} : {GameManager.instance.pid_player_Dict.ElementAt(i).Value.name}");
                        _motionTrackerDotsList[i].biped = GameManager.instance.pid_player_Dict.ElementAt(i).Value.GetComponent<Biped>();
                    }
                }
            }
            else
            {
                if (GameManager.instance.gameMode == GameManager.GameMode.Coop)
                {
                    if (SwarmManager.instance.actorsAliveList.Count > 0 && (i - 8 < SwarmManager.instance.actorsAliveList.Count))
                    {
                        print($"PlayerMotionTracker assigning actor {i - 8} {i}. {SwarmManager.instance.actorsAliveList.Count}");
                        _motionTrackerDotsList[i].biped = SwarmManager.instance.actorsAliveList[i - 8];
                    }
                }
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerUIComponents : MonoBehaviour
{
    [Header("Singletons")]
    public OnlineGameTime onlineGameTimeInstance;
    public PhotonView PV;

    [Header("Cameras", order = 0)]
    public Camera mainCamera;
    public Camera gunCamera;

    [Header("Top Left", order = 1)]
    public Transform topLeft;
    public GameObject fragGrenadeIcon;
    public GameObject stickyGrenadeIcon;

    [Header("Top Middle", order = 2)]
    public Transform topMiddle;

    [Header("Top Right", order = 3)]
    public Transform topRight;

    [Header("Center", order = 4)]
    public Transform center;

    [Header("Bottom Left", order = 5)]
    public Transform bottomLeft;

    [Header("Bottom Right", order = 6)]
    public Text Timer;
    public Transform bottomRight;
    public GameObject multiplayerPoints;
    public Text multiplayerPointsRed;
    public Text multiplayerPointsBlue;
    public GameObject swarmPoints;
    public Text swarmPointsText;

    [Header("General", order = 7)]
    public GameObject singlePlayerPauseMenu;
    public GameObject splitScreenPauseMenu;

    private void Start()
    {
        onlineGameTimeInstance = OnlineGameTime.onlineGameTimeInstance;

        onlineGameTimeInstance.playerTimerTexts.Add(Timer);
    }
}

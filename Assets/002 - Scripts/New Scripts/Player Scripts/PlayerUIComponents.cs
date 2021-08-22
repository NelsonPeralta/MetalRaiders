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
    public GameObject swarmLivesHolder;
    public Text swarmLivesText;

    [Header("Top Middle", order = 2)]
    public Transform topMiddle;

    [Header("Top Right", order = 3)]
    public Transform topRight;

    [Header("Center", order = 4)]
    public Transform center;
    public ScoreboardManager scoreboard;
    public GameObject headshotIndicator;
    Coroutine showHeadshotIndicatorCoroutine;
    Coroutine hideHeadshotIndicatorCoroutine;

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

        headshotIndicator.SetActive(false);
    }

    public void ShowHeadshotIndicator()
    {
        if (hideHeadshotIndicatorCoroutine != null)
            StopCoroutine(hideHeadshotIndicatorCoroutine);
        if (showHeadshotIndicatorCoroutine != null)
            StopCoroutine(showHeadshotIndicatorCoroutine);

        hideHeadshotIndicatorCoroutine = StartCoroutine(HideHeadshotIndicator_Coroutine());
        showHeadshotIndicatorCoroutine = StartCoroutine(ShowHeadshotIndicator_Coroutine());
    }

    IEnumerator HideHeadshotIndicator_Coroutine()
    {
        if (headshotIndicator.activeSelf)
        {
            if (showHeadshotIndicatorCoroutine != null)
                StopCoroutine(showHeadshotIndicatorCoroutine);
            headshotIndicator.SetActive(false);
            yield return new WaitForSeconds(0.1f);
            StartCoroutine(ShowHeadshotIndicator_Coroutine());
        }else
            StartCoroutine(ShowHeadshotIndicator_Coroutine());
    }
    IEnumerator ShowHeadshotIndicator_Coroutine()
    {
        if (!headshotIndicator.activeSelf)
            headshotIndicator.SetActive(true);

        yield return new WaitForSeconds(0.25f);

        headshotIndicator.SetActive(false);
    }

}

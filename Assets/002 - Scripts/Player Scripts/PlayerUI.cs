using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerUI : MonoBehaviour
{
    public Canvas canvas;
    [Header("Scripts")]
    public PlayerSwarmMatchStats onlinePlayerSwarmScript;
    [Header("Singletons")]
    public OnlineGameTime onlineGameTimeInstance;
    public PhotonView PV;

    [Header("Cameras", order = 0)]
    public Camera mainCamera;
    public Camera gunCamera;

    [Header("Top Left", order = 1)]
    public Transform topLeft;
    public GameObject fragGrenadeIcon;
    public Text grenadeText;
    public GameObject stickyGrenadeIcon;
    public GameObject swarmLivesHolder;
    public Text swarmLivesText;

    [Header("Top Center", order = 2)]
    public Transform topMiddle;
    public GameObject healthBar;
    public GameObject shieldBar;
    [SerializeField] Text objectiveInformerText;
    [SerializeField] Text swarmAisLeftText;

    [Header("Top Right", order = 3)]
    public Transform topRight;

    [Header("Center", order = 4)]
    public Transform center;
    public ScoreboardManager scoreboard;
    public GameObject headshotIndicator;
    Coroutine showHeadshotIndicatorCoroutine;
    Coroutine hideHeadshotIndicatorCoroutine;
    [SerializeField] Text weaponInformerText;

    [Header("Bottom Left", order = 5)]
    public Transform bottomLeft;
    public GameObject motionTracker;

    [Header("Bottom Right", order = 6)]
    public Text Timer;
    public Transform bottomRight;
    public GameObject multiplayerPointsHolder;
    public Text multiplayerPointsRed;
    public Text multiplayerPointsBlue;
    public GameObject swarmPointsHolder;
    public Text swarmPointsText;

    [Header("General", order = 7)]
    public GameObject singlePlayerPauseMenu;
    public GameObject splitScreenPauseMenu;
    public KillFeedManager killFeedManager;
    public PlayerDebuggerOnUI PlayerDebuggerOnUI;

    private void Start()
    {
        onlineGameTimeInstance = OnlineGameTime.onlineGameTimeInstance;

        onlineGameTimeInstance.playerTimerTexts.Add(Timer);

        headshotIndicator.SetActive(false);

        OnSwarmKillsChanged(onlinePlayerSwarmScript);

        if (!PV.IsMine)
        {
            canvas.gameObject.SetActive(false);
            return;
        }

        if (GameManager.instance.gameMode == GameManager.GameMode.Swarm)
            EnableSwarmUIComponents();
        else if (GameManager.instance.gameMode == GameManager.GameMode.Multiplayer)
            EnableMultiplayerUIComponents();

        swarmAisLeftText.text = "";

        SwarmManager.instance.OnWaveIncrease += OnNewWave_Delegate;
        GetComponent<PlayerController>().OnPLayerThrewGrenade += OnPlayerThrewGrenade_Delegate;
        SwarmManager.instance.OnPlayerLivesChanged += OnPlayerLivesChanged_Delegate;
        SwarmManager.instance.OnAiSpawn += OnSwarmAiDeathOrSpawn;
        SwarmManager.instance.OnAiDeath += OnSwarmAiDeathOrSpawn;
    }

    void OnSwarmKillsChanged(PlayerSwarmMatchStats onlinePlayerSwarmScript)
    {
        // Change the ui using onlinePlayerSwarmScript.kills
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
        }
        else
            StartCoroutine(ShowHeadshotIndicator_Coroutine());
    }
    IEnumerator ShowHeadshotIndicator_Coroutine()
    {
        if (!headshotIndicator.activeSelf)
            headshotIndicator.SetActive(true);

        yield return new WaitForSeconds(0.25f);

        headshotIndicator.SetActive(false);
    }

    void EnableSwarmUIComponents()
    {
        swarmPointsHolder.SetActive(true);
        swarmPointsText.text = "0";

        swarmLivesHolder.SetActive(true);
        swarmLivesText.text = SwarmManager.instance.livesLeft.ToString();

        DisableMultiplayerUIComponents();
    }

    void DisableSwarmUIComponents()
    {
        swarmPointsHolder.SetActive(false);
        swarmLivesHolder.SetActive(false);
    }

    void EnableMultiplayerUIComponents()
    {
        shieldBar.SetActive(true);
        multiplayerPointsHolder.SetActive(true);
        motionTracker.SetActive(true);

        DisableSwarmUIComponents();
    }

    void DisableMultiplayerUIComponents()
    {
        shieldBar.SetActive(false);
        multiplayerPointsHolder.SetActive(false);
        motionTracker.SetActive(false);
    }

    public void AddInformerText(string message)
    {
        StartCoroutine(AddInformerText_Coroutine(message));
    }

    IEnumerator AddInformerText_Coroutine(string message)
    {
        objectiveInformerText.text = $"{message}";
        yield return new WaitForSeconds(3);
    }
    void OnNewWave_Delegate(SwarmManager swarmManager)
    {
        AddInformerText($"Wave {swarmManager.currentWave}");
    }

    void OnPlayerLivesChanged_Delegate(SwarmManager swarmManager)
    {
        swarmLivesText.text = swarmManager.livesLeft.ToString();
    }
    void OnPlayerThrewGrenade_Delegate(PlayerController playerController)
    {
        grenadeText.text = GetComponent<Player>().pInventory.grenades.ToString();
    }

    void OnSwarmAiDeathOrSpawn(SwarmManager swarmManager)
    {
        swarmAisLeftText.text = $"H: {swarmManager.hellhoundsLeft + swarmManager.hellhoundsAlive} W: {swarmManager.watchersLeft + swarmManager.watchersAlive} Knights: {swarmManager.knightsLeft + swarmManager.knightsAlive}";
    }
}

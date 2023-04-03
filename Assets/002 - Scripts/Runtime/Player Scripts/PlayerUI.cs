using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    public Canvas canvas;
    [Header("Scripts")]
    public PlayerSwarmMatchStats onlinePlayerSwarmScript;
    [Header("Singletons")]
    public GameTime onlineGameTimeInstance;
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
    public Text mapNameText;
    public Text roomNameText;
    public GameObject leftWeaponUiHolder;
    public TMP_Text leftActiveAmmoText;
    public TMP_Text leftSpareAmmoText;
    public Image leftWeaponIcon;

    [Header("Top Center", order = 2)]
    public Transform topMiddle;
    [SerializeField] GameObject barsHolder;
    public GameObject healthBar;
    public GameObject shieldBar;
    [SerializeField] Text objectiveInformerText;
    [SerializeField] Text swarmAisLeftText;

    [Header("Top Right", order = 3)]
    public Transform topRight;
    public TMP_Text activeAmmoText;
    public TMP_Text spareAmmoText;
    public Image activeWeaponIcon;
    public Image holsteredWeaponIcon;

    [Header("Center", order = 4)]
    public Transform center;
    public ScoreboardManager scoreboard;
    public GameObject headshotIndicator;
    Coroutine showHeadshotIndicatorCoroutine;
    Coroutine hideHeadshotIndicatorCoroutine;
    public Text weaponInformerText;

    [Header("Bottom Left", order = 5)]
    public Transform bottomLeft;
    public GameObject motionTracker;
    public Text isMineText;
    public Text camSensWitnessText;

    [Header("Bottom Right", order = 6)]
    public Text Timer;
    public Text gameType;
    public Transform bottomRight;
    public GameObject multiplayerPointsHolder;
    public Text multiplayerPointsGrey;
    public Text multiplayerPointsRed;
    public Text multiplayerPointsBlue;
    public GameObject swarmPointsHolder;
    public Text swarmPointsText;

    [Header("General", order = 7)]
    public GameObject singlePlayerPauseMenu;
    public GameObject splitScreenPauseMenu;
    public PlayerDebuggerOnUI PlayerDebuggerOnUI;

    [Header("Hit Markers", order = 8)]
    public Transform hitMarkerSpawnPoint;
    public Transform hitMarker;
    public Transform headshotMarker;
    public Transform killMarker;
    public Transform headshotKill;
    private void OnEnable()
    {
        try
        {
            //GameManager.instance.OnCameraSensitivityChanged -= OnCameraSensitivityChanged;
            GameManager.instance.OnCameraSensitivityChanged += OnCameraSensitivityChanged;
        }
        catch { }

        //GetComponent<Player>().playerInventory.OnGrenadeChanged -= OnGrenadeChanged_Delegate;
        GetComponent<Player>().playerInventory.OnGrenadeChanged += OnGrenadeChanged_Delegate;
        GetComponent<Player>().playerInventory.OnActiveWeaponChanged += OnActiveWeaponChanged_Delegate;
        GetComponent<Player>().playerInventory.OnHolsteredWeaponChanged += OnHolsteredWeaponChanged_Delegate;
        if (GameManager.instance.gameMode == GameManager.GameMode.Swarm)
        {
            //SwarmManager.instance.OnPlayerLivesChanged -= OnPlayerLivesChanged_Delegate;
            //SwarmManager.instance.OnAiSpawn -= OnSwarmAiDeathOrSpawn;
            //SwarmManager.instance.OnAiDeath -= OnSwarmAiDeathOrSpawn;
            //SwarmManager.instance.OnAIsCalculated -= OnSwarmAiDeathOrSpawn;
            //SwarmManager.instance.OnWaveIncrease -= OnNewWave_Delegate;

            SwarmManager.instance.OnPlayerLivesChanged += OnPlayerLivesChanged_Delegate;
            SwarmManager.instance.OnAiSpawn += OnSwarmAiDeathOrSpawn;
            SwarmManager.instance.OnAiDeath += OnSwarmAiDeathOrSpawn;
            SwarmManager.instance.OnAIsCalculated += OnSwarmAiDeathOrSpawn;
            SwarmManager.instance.OnWaveIncrease += OnNewWave_Delegate;
        }
    }

    private void OnDisable()
    {
        GameManager.instance.OnCameraSensitivityChanged -= OnCameraSensitivityChanged;

        GetComponent<Player>().playerInventory.OnGrenadeChanged -= OnGrenadeChanged_Delegate;
        if (GameManager.instance.gameMode == GameManager.GameMode.Swarm)
        {
            SwarmManager.instance.OnPlayerLivesChanged -= OnPlayerLivesChanged_Delegate;
            SwarmManager.instance.OnAiSpawn -= OnSwarmAiDeathOrSpawn;
            SwarmManager.instance.OnAiDeath -= OnSwarmAiDeathOrSpawn;
            SwarmManager.instance.OnAIsCalculated -= OnSwarmAiDeathOrSpawn;
            SwarmManager.instance.OnWaveIncrease -= OnNewWave_Delegate;
        }
    }

    private void Start()
    {
        Debug.Log("PlayerUI Start");
        try { gameType.text = GameManager.instance.gameType.ToString(); }
        catch (System.Exception e) { Debug.LogWarning($"{e}"); }

        try { mapNameText.text = GameManager.GetActiveSceneName().Replace("PVP - ", ""); }
        catch (System.Exception e) { Debug.LogWarning($"{e}"); }

        try { mapNameText.text = GameManager.GetActiveSceneName().Replace("Coop - ", ""); }
        catch (System.Exception e) { Debug.LogWarning($"{e}"); }

        try { roomNameText.text = PhotonNetwork.CurrentRoom.Name; }
        catch (System.Exception e) { Debug.LogWarning($"{e}"); }

        try
        {
            camSensWitnessText.text = $"Sens: {GameManager.instance.camSens.ToString()}";
            //GameManager.instance.OnCameraSensitivityChanged -= OnCameraSensitivityChanged;
            //GameManager.instance.OnCameraSensitivityChanged += OnCameraSensitivityChanged;
        }
        catch { }
        try
        {
            //onlineGameTimeInstance = OnlineGameTime.onlineGameTimeInstance;
            //onlineGameTimeInstance.playerTimerTexts.Add(Timer);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"{name}: {e}");
        }


        headshotIndicator.SetActive(false);

        OnSwarmKillsChanged(onlinePlayerSwarmScript);

        if (!PV.IsMine)
        {
            canvas.gameObject.SetActive(false);
            return;
        }

        //GetComponent<Player>().playerInventory.OnGrenadeChanged += OnGrenadeChanged_Delegate;
        if (GameManager.instance.gameMode == GameManager.GameMode.Swarm)
        {
            EnableSwarmUIComponents();
            swarmAisLeftText.text = "";

            //SwarmManager.instance.OnPlayerLivesChanged += OnPlayerLivesChanged_Delegate;
            //SwarmManager.instance.OnAiSpawn += OnSwarmAiDeathOrSpawn;
            //SwarmManager.instance.OnAiDeath += OnSwarmAiDeathOrSpawn;
            //SwarmManager.instance.OnAIsCalculated += OnSwarmAiDeathOrSpawn;
            //SwarmManager.instance.OnWaveIncrease += OnNewWave_Delegate;
        }
        else if (GameManager.instance.gameMode == GameManager.GameMode.Multiplayer)
        {
            EnableMultiplayerUIComponents();
            GetComponent<PlayerMultiplayerMatchStats>().OnPlayerScoreChanged += OnPlayerScoreChanged_Delegate;
        }
        if(GameManager.instance.gameType == GameManager.GameType.Swat)
            barsHolder.SetActive(false);

        OnGrenadeChanged_Delegate(GetComponent<Player>().playerInventory);
    }

    void OnSwarmKillsChanged(PlayerSwarmMatchStats onlinePlayerSwarmScript)
    {
        // Change the ui using onlinePlayerSwarmScript.kills
    }

    void OnCameraSensitivityChanged()
    {
        camSensWitnessText.text = $"Sens: {GameManager.instance.camSens.ToString()}";
    }

    //public void ShowHeadshotIndicator()
    //{
    //    if (hideHeadshotIndicatorCoroutine != null)
    //        StopCoroutine(hideHeadshotIndicatorCoroutine);
    //    if (showHeadshotIndicatorCoroutine != null)
    //        StopCoroutine(showHeadshotIndicatorCoroutine);

    //    hideHeadshotIndicatorCoroutine = StartCoroutine(HideHeadshotIndicator_Coroutine());
    //    showHeadshotIndicatorCoroutine = StartCoroutine(ShowHeadshotIndicator_Coroutine());
    //}

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
        healthBar.SetActive(true);

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
        swarmAisLeftText.gameObject.SetActive(false);
    }

    void EnableMultiplayerUIComponents()
    {
        multiplayerPointsHolder.SetActive(true);

        DisableSwarmUIComponents();
    }

    void DisableMultiplayerUIComponents()
    {
        shieldBar.SetActive(false);
        multiplayerPointsHolder.SetActive(false);
        motionTracker.SetActive(false);
    }

    public void EnableArmorUI()
    {
        shieldBar.SetActive(true);
        healthBar.SetActive(false);
        if ((GameManager.instance.gameType == GameManager.GameType.Slayer) || GameManager.instance.gameType == GameManager.GameType.Fiesta)
            motionTracker.SetActive(true);
        else
            motionTracker.SetActive(false);

    }
    public void AddInformerText(string message)
    {
        StartCoroutine(AddInformerText_Coroutine(message));
    }

    IEnumerator AddInformerText_Coroutine(string message)
    {
        objectiveInformerText.gameObject.SetActive(false);

        objectiveInformerText.text = $"{message}";
        
        yield return new WaitForSeconds(0.2f);
        objectiveInformerText.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.2f);
        objectiveInformerText.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.2f);
        objectiveInformerText.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.2f);
        objectiveInformerText.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.2f);
        objectiveInformerText.gameObject.SetActive(true);

    }
    void OnNewWave_Delegate(SwarmManager swarmManager)
    {
        AddInformerText($"Wave {swarmManager.currentWave}");
    }

    void OnPlayerLivesChanged_Delegate(SwarmManager swarmManager)
    {
        swarmLivesText.text = swarmManager.livesLeft.ToString();
    }

    void OnActiveWeaponChanged_Delegate(PlayerInventory playerInventory)
    {
        activeAmmoText.text = playerInventory.activeWeapon.currentAmmo.ToString();
        spareAmmoText.text = playerInventory.activeWeapon.spareAmmo.ToString();

        try { activeWeaponIcon.sprite = playerInventory.activeWeapon.weaponIcon; } catch { activeWeaponIcon.sprite = null; }
    }

    void OnHolsteredWeaponChanged_Delegate(PlayerInventory playerInventory)
    {
        WeaponProperties wp = playerInventory.holsteredWeapon;
        Debug.Log(wp.name);
        try { holsteredWeaponIcon.sprite = wp.weaponIcon; } catch { holsteredWeaponIcon.sprite = null; }
    }
    void OnGrenadeChanged_Delegate(PlayerInventory playerInventory)
    {
        grenadeText.text = playerInventory.grenades.ToString();
    }

    void OnSwarmAiDeathOrSpawn(SwarmManager swarmManager)
    {
        //swarmAisLeftText.text = $"H: {swarmManager.hellhoundsLeft + swarmManager.hellhoundsAlive} W: {swarmManager.watchersLeft + swarmManager.watchersAlive} K: {swarmManager.knightsLeft + swarmManager.knightsAlive}";

        Debug.Log("Here");
        if (!swarmManager.waveEnded)
        {
            try
            {
                swarmAisLeftText.text = $"Aliens: {swarmManager.watchersLeft + swarmManager.watchersAlive}\nBreathers: {swarmManager.knightsLeft + swarmManager.knightsAlive}\nZombies: {swarmManager.zombiesLeft + swarmManager.zombiesAlive}";

                if (swarmManager.currentWave % 5 == 0)
                    swarmAisLeftText.text = $"Tyrants: {swarmManager.tyrantsLeft + swarmManager.tyrantsAlive}";
            }
            catch
            {
                GameManager.instance.OnCameraSensitivityChanged -= OnCameraSensitivityChanged;
                if (GameManager.instance.gameMode == GameManager.GameMode.Swarm)
                {
                    SwarmManager.instance.OnPlayerLivesChanged -= OnPlayerLivesChanged_Delegate;
                    SwarmManager.instance.OnAiSpawn -= OnSwarmAiDeathOrSpawn;
                    SwarmManager.instance.OnAiDeath -= OnSwarmAiDeathOrSpawn;
                    SwarmManager.instance.OnAIsCalculated -= OnSwarmAiDeathOrSpawn;
                    SwarmManager.instance.OnWaveIncrease -= OnNewWave_Delegate;
                }
            }
        }
    }

    public enum HitMarkerType { Hit, Headshot, Kill, HeadshotKill };
    public void SpawnHitMarker(HitMarkerType hitMarkerType = HitMarkerType.Hit)
    {
        if (!GetComponent<PhotonView>().IsMine)
            return;
        Transform hm = null;
        if (hitMarkerType == HitMarkerType.Hit)
            hm = Instantiate(hitMarker, hitMarkerSpawnPoint.transform);
        else if (hitMarkerType == HitMarkerType.Headshot)
            hm = Instantiate(headshotMarker, hitMarkerSpawnPoint.transform);
        else if (hitMarkerType == HitMarkerType.Kill)
            hm = Instantiate(killMarker, hitMarkerSpawnPoint.transform);
        else if (hitMarkerType == HitMarkerType.HeadshotKill)
            hm = Instantiate(headshotKill, hitMarkerSpawnPoint.transform);

        try
        {
            Destroy(hm.gameObject, 0.5f);
        }
        catch { }
    }

    void OnPlayerScoreChanged_Delegate(PlayerMultiplayerMatchStats playerMultiplayerMatchStats)
    {
        multiplayerPointsGrey.text = playerMultiplayerMatchStats.score.ToString();
    }
}

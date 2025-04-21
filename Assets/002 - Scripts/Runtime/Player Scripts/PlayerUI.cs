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
    public PlayerMultiplayerMatchStats playerMultiplayerMatchStats;
    public PlayerSwarmMatchStats onlinePlayerSwarmScript;
    [Header("Singletons")]
    public GameTime onlineGameTimeInstance;
    public PhotonView PV;


    [Header("Top Left", order = 1)]
    public Transform topLeft;
    public Text fragGrenadeText, plasmaGrenadeText;
    public GameObject fragGrenadeBox, plasmaGrenadeBox;
    public Image fragGrenadeImage, plasmaGrenadeImage;
    public GameObject swarmLivesHolder;
    public Text swarmLivesText;
    public Text mapNameText;
    public Text roomNameText;
    public GameObject leftWeaponUiHolder;
    public TMP_Text leftActiveAmmoText;
    public TMP_Text leftSpareAmmoText;
    public Image leftWeaponIcon;
    [SerializeField] TMP_Text _pickedUpLeftWeaponAmmoWitnessText;

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
    public TMP_Text pickedUpAmmoWitnessText;
    public TMP_Text pickedUpGrenadeWitnessText;

    [Header("Center", order = 4)]
    public Transform center, _notScoreboard;
    public ScoreboardManager scoreboard;
    public GameObject headshotIndicator;
    Coroutine showHeadshotIndicatorCoroutine;
    Coroutine hideHeadshotIndicatorCoroutine;
    [SerializeField] GameObject _informerHolder;
    [SerializeField] TMP_Text _informerText;
    [SerializeField] Image _weaponIconInformer;
    [SerializeField] PlayerPointWitness _pointWitness;
    public OptionsMenu optionsMenuScript;
    public MenuGamePadCursor gamepadCursor;
    [SerializeField] GameObject _yourFlagStolenHolder;
    [SerializeField] TMP_Text _yourFlagStolenText;
    [SerializeField] TMP_Text _editorText;
    [SerializeField] Animator _blackscreenDefault, _blackscreenSplitScreen;

    [Header("Bottom Left", order = 5)]
    public Transform bottomLeft, notKillFeed;
    [SerializeField] GameObject _motionTracker, _motionTrackerSwarmHolder;
    public Text isMineText;
    public Text camSensWitnessText;

    [Header("Bottom Right", order = 6)]
    public Text Timer;
    public TMP_Text gameType;
    public Transform bottomRight;
    public GameObject multiplayerPointsHolder;
    public GameObject neutralPointsHolder, redPointsHolder, bluePointsHolder;
    public Text multiplayerPointsGrey;
    public Text multiplayerPointsRed;
    public Text multiplayerPointsBlue;
    public GameObject swarmPointsHolder;
    public Text swarmPointsText;
    public GameObject invincibleIcon;

    [Header("General", order = 7)]
    public GameObject singlePlayerPauseMenu;


    [SerializeField] List<GameObject> _hitMarkers = new List<GameObject>(); [SerializeField] List<GameObject> _killMarkers = new List<GameObject>();
    [SerializeField] State _state;









    Player _player;



    private void Awake()
    {
        _player = GetComponent<Player>();

        _editorText.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        try
        {
            //GameManager.instance.OnCameraSensitivityChanged -= OnCameraSensitivityChanged;
        }
        catch { }

        //GetComponent<Player>().playerInventory.OnGrenadeChanged -= OnGrenadeChanged_Delegate;
        GetComponent<Player>().playerInventory.OnGrenadeChanged += OnGrenadeChanged_Delegate;
        GetComponent<Player>().playerInventory.OnActiveWeaponChanged += OnActiveWeaponChanged_Delegate;
        GetComponent<Player>().playerInventory.OnHolsteredWeaponChanged += OnHolsteredWeaponChanged_Delegate;
        GetComponent<Player>().OnPlayerRespawningInOneSecond += OnPlayerRespawningInOneSecond;


        if (GameManager.instance.gameMode == GameManager.GameMode.Coop)
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

        GetComponent<Player>().playerInventory.OnGrenadeChanged -= OnGrenadeChanged_Delegate;
        if (GameManager.instance.gameMode == GameManager.GameMode.Coop)
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
        _yourFlagStolenHolder.SetActive(false);
        transform.GetComponent<Player>().playerInteractableObjectHandler.ClosestInteractableObjectAssigned += OnClosestInteractableObjectAssigned;
        playerMultiplayerMatchStats = GetComponent<PlayerMultiplayerMatchStats>();
        HideInformer();
        if (GameManager.instance.gameType != GameManager.GameType.Martian) plasmaGrenadeImage.gameObject.SetActive(false);
        gamepadCursor.gameObject.SetActive(false);
        GameObject thm = null;

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
        if (GameManager.instance.gameMode == GameManager.GameMode.Coop)
        {
            EnableSwarmUIComponents();
            swarmAisLeftText.text = "";

            //SwarmManager.instance.OnPlayerLivesChanged += OnPlayerLivesChanged_Delegate;
            //SwarmManager.instance.OnAiSpawn += OnSwarmAiDeathOrSpawn;
            //SwarmManager.instance.OnAiDeath += OnSwarmAiDeathOrSpawn;
            //SwarmManager.instance.OnAIsCalculated += OnSwarmAiDeathOrSpawn;
            //SwarmManager.instance.OnWaveIncrease += OnNewWave_Delegate;
        }
        else if (GameManager.instance.gameMode == GameManager.GameMode.Versus)
        {
            EnableMultiplayerUIComponents();
            GetComponent<PlayerMultiplayerMatchStats>().OnPlayerScoreChanged += OnPlayerScoreChanged_Delegate;
        }
        if (GameManager.instance.gameType == GameManager.GameType.Swat
             || GameManager.instance.gameType == GameManager.GameType.Retro)
        {
            healthBar.SetActive(true);
            barsHolder.SetActive(false);
        }

        OnGrenadeChanged_Delegate(GetComponent<Player>().playerInventory);


#if UNITY_EDITOR
        _editorText.gameObject.SetActive(true);
#endif
    }




    void OnSwarmKillsChanged(PlayerSwarmMatchStats onlinePlayerSwarmScript)
    {
        // Change the ui using onlinePlayerSwarmScript.kills
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
        ToggleMotionTracker_ForAiming(false);
    }

    public void EnableArmorUI()
    {
        ToggleMotionTrackerHolder(true);



        if (GameManager.instance.gameType != GameManager.GameType.Pro &&
                GameManager.instance.gameType != GameManager.GameType.Swat &&
                GameManager.instance.gameType != GameManager.GameType.Snipers &&
                 GameManager.instance.gameType != GameManager.GameType.Retro)
            ToggleMotionTracker_ForAiming(true);
        else
            ToggleMotionTracker_ForAiming(false);




        shieldBar.SetActive(true);
        healthBar.SetActive(false);

        shieldBar.GetComponent<Slider>().maxValue = GetComponent<Player>().maxShieldPoints;
        shieldBar.GetComponent<Slider>().value = GetComponent<Player>().shieldPoints;
    }


    public void DisableArmorUI()
    {
        ToggleMotionTrackerHolder(false);
        ToggleMotionTracker_ForAiming(false);
        shieldBar.SetActive(false);
        healthBar.SetActive(true);
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
        if (GetComponent<Player>().PV.IsMine)
        {
            activeAmmoText.text = playerInventory.activeWeapon.loadedAmmo.ToString();
            spareAmmoText.text = playerInventory.activeWeapon.spareAmmo.ToString();

            try { activeWeaponIcon.sprite = playerInventory.activeWeapon.weaponIcon; } catch { activeWeaponIcon.sprite = null; }
        }
    }

    void OnHolsteredWeaponChanged_Delegate(PlayerInventory playerInventory)
    {
        WeaponProperties wp = playerInventory.holsteredWeapon;
        try { holsteredWeaponIcon.sprite = wp.weaponIcon; } catch { holsteredWeaponIcon.sprite = null; }
    }
    void OnGrenadeChanged_Delegate(PlayerInventory playerInventory)
    {
        fragGrenadeText.text = playerInventory.fragGrenades.ToString();
        plasmaGrenadeText.text = playerInventory.plasmaGrenades.ToString();
    }

    void OnSwarmAiDeathOrSpawn(SwarmManager swarmManager)
    {
        //swarmAisLeftText.text = $"H: {swarmManager.hellhoundsLeft + swarmManager.hellhoundsAlive} W: {swarmManager.watchersLeft + swarmManager.watchersAlive} K: {swarmManager.knightsLeft + swarmManager.knightsAlive}";

        Debug.Log("Here");
        swarmAisLeftText.text = $"Aliens: {swarmManager.ribbiansLeft + swarmManager.ribbiansAlive}\nBreathers: {swarmManager.breathersLeft + swarmManager.breathersAlive}\nZombies: {swarmManager.zombiesLeft + swarmManager.zombiesAlive}";
        if (!swarmManager.waveEnded)
        {
            try
            {

                if (swarmManager.currentWave % 5 == 0)
                    swarmAisLeftText.text = $"Tyrants: {swarmManager.tyrantsLeft + swarmManager.tyrantsAlive}";
            }
            catch
            {
                if (GameManager.instance.gameMode == GameManager.GameMode.Coop)
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
        if (!GetComponent<PhotonView>().IsMine || GameManager.instance.hitMarkersMode == GameManager.HitMarkersMode.Off)
            return;


        if (hitMarkerType == HitMarkerType.Hit)
            foreach (GameObject hm in _hitMarkers) { if (!hm.activeInHierarchy) { hm.SetActive(true); break; } }
        else if (hitMarkerType == HitMarkerType.Kill)
            foreach (GameObject hm in _killMarkers) { if (!hm.activeInHierarchy) { hm.SetActive(true); break; } }

        //Transform hm = null;
        //if (hitMarkerType == HitMarkerType.Hit)
        //    hm = Instantiate(hitMarker, hitMarkerSpawnPoint.transform);
        //else if (hitMarkerType == HitMarkerType.Headshot)
        //    hm = Instantiate(headshotMarker, hitMarkerSpawnPoint.transform);
        //else if (hitMarkerType == HitMarkerType.Kill)
        //    hm = Instantiate(killMarker, hitMarkerSpawnPoint.transform);
        //else if (hitMarkerType == HitMarkerType.HeadshotKill)
        //    hm = Instantiate(headshotKill, hitMarkerSpawnPoint.transform);

        //try
        //{
        //    Destroy(hm.gameObject, 0.5f);
        //}
        //catch { }
    }

    void OnPlayerScoreChanged_Delegate(PlayerMultiplayerMatchStats playerMultiplayerMatchStats)
    {
        UpdateScoreWitnesses();
    }

    public void ShowInformer(string mess, Sprite icon = null)
    {
        _informerText.text = mess;
        _weaponIconInformer.sprite = icon;

        _weaponIconInformer.gameObject.SetActive(icon != null);

        _informerHolder.gameObject.SetActive(true);
    }

    public void HideInformer()
    {
        _informerHolder.gameObject.SetActive(false);
        _weaponIconInformer.sprite = null;
        _informerText.text = "";
    }

    public void ToggleUIExtremities(bool t)
    {
        _notScoreboard.gameObject.SetActive(t);
        topMiddle.gameObject.SetActive(t);
        topLeft.gameObject.SetActive(t);
        topRight.gameObject.SetActive(t);
        //bottomLeft.gameObject.SetActive(t);
        notKillFeed.gameObject.SetActive(t);
        bottomRight.gameObject.SetActive(t);
    }


    public void UpdateBottomRighScore()
    {

    }



    public void ShowPointWitness(int p)
    {
        _pointWitness.Add(p);
    }

    public void SetScoreWitnesses()
    {
        if (GameManager.instance.teamMode == GameManager.TeamMode.Classic)
        {
            print($"SetScoreWitnesses {GetComponent<Player>().team}");
            neutralPointsHolder.gameObject.SetActive(false);
            redPointsHolder.gameObject.SetActive(GetComponent<Player>().team == GameManager.Team.Red);
            bluePointsHolder.gameObject.SetActive(GetComponent<Player>().team == GameManager.Team.Blue);
        }
    }

    public void UpdateScoreWitnesses()
    {
        if (GameManager.instance.teamMode == GameManager.TeamMode.Classic)
        {
            if (GetComponent<Player>().team == GameManager.Team.Red)
                multiplayerPointsRed.text = MultiplayerManager.instance.redTeamScore.ToString();
            else
                multiplayerPointsBlue.text = MultiplayerManager.instance.blueTeamScore.ToString();
        }
        else
            multiplayerPointsGrey.text = playerMultiplayerMatchStats.score.ToString();
    }


    public void ToggleMotionTracker_ForAiming(bool b)
    {
        if (_motionTracker.gameObject.activeSelf != b)
        {
            print($"ToggleMotionTracker {b}");
            _motionTracker.SetActive(b);
        }
    }

    public void ToggleMotionTrackerHolder(bool b)// used more for Swarm
    {
        _motionTrackerSwarmHolder.gameObject.SetActive(b);
    }



    enum State { s0, s1, s2, s3, s4, s5, s6, s7, s8, s9, s10 }


    private void Update()
    {
        _state = State.s0;

        if (_player.playerInteractableObjectHandler.closestInteractableObject == null)
        {
            HideInformer();
        }
        else
        {
            if (_player.hasEnnemyFlag || _player.playerInventory.playerOddballActive)
            {
                HideInformer();
            }
            else
            {

                if (_player.playerInteractableObjectHandler.closestInteractableObject.GetComponent<LootableWeapon>())
                {
                    if (_player.playerInteractableObjectHandler.closestInteractableObjectIsDualWieldableAndActiveWeaponIsDualWieldableAlso)
                    {
                        _state = State.s1;
                        if (!_player.isDualWielding)
                        {
                            _state = State.s2;

                            if (_player.playerInteractableObjectHandler.closestInteractableObject.GetComponent<LootableWeapon>().codeName != _player.playerInventory.activeWeapon.codeName
                            && _player.playerInteractableObjectHandler.closestInteractableObject.GetComponent<LootableWeapon>().codeName != _player.playerInventory.holsteredWeapon.codeName)
                            {
                                _state = State.s3;

                                ShowInformer($"Hold [Mark] to Dual Wield or Hold [Interact] to swap for", transform.GetComponent<Player>().playerInventory.GetWeaponProperties(_player.playerInteractableObjectHandler.closestInteractableObject.GetComponent<LootableWeapon>().codeName).weaponIcon);
                            }
                            //else if (_player.playerInteractableObjectHandler.closestInteractableObject.GetComponent<LootableWeapon>().codeName != _player.playerInventory.activeWeapon.codeName
                            //&& _player.playerInteractableObjectHandler.closestInteractableObject.GetComponent<LootableWeapon>().codeName != _player.playerInventory.holsteredWeapon.codeName)
                            //{

                            //}
                            else
                            {
                                _state = State.s4;

                                ShowInformer($"Hold [Mark] to Dual Wield ", transform.GetComponent<Player>().playerInventory.GetWeaponProperties(_player.playerInteractableObjectHandler.closestInteractableObject.GetComponent<LootableWeapon>().codeName).weaponIcon);
                                //HideInformer();
                            }
                        }
                        else
                        {
                            _state = State.s5;
                            HideInformer();

                        }






                        //if (!_player.isDualWielding && GetComponent<Player>().playerInventory.activeWeapon.isDualWieldable /*&& (_player.playerInteractableObjectHandler.closestInteractableObject.GetComponent<LootableWeapon>().codeName.Equals(GetComponent<Player>().playerInventory.activeWeapon.codeName))*/)
                        //    ShowInformer($"Hold [Mark] to Dual Wield or Hold [Interact] to swap for", transform.GetComponent<Player>().playerInventory.GetWeaponProperties(_player.playerInteractableObjectHandler.closestInteractableObject.GetComponent<LootableWeapon>().codeName).weaponIcon);
                        //else
                        //    HideInformer();
                    }
                    else
                    {
                        if (_player.playerInteractableObjectHandler.closestInteractableObject.GetComponent<LootableWeapon>().codeName != _player.playerInventory.activeWeapon.codeName
                            && _player.playerInteractableObjectHandler.closestInteractableObject.GetComponent<LootableWeapon>().codeName != _player.playerInventory.holsteredWeapon.codeName)
                        {
                            if (!_player.isDualWielding)
                            {
                                _state = State.s6;
                                ShowInformer($"Hold [Interact] to swap for ", transform.GetComponent<Player>().playerInventory.GetWeaponProperties(_player.playerInteractableObjectHandler.closestInteractableObject.GetComponent<LootableWeapon>().codeName).weaponIcon);
                            }
                            else
                            {
                                _state = State.s7;
                                HideInformer();
                            }
                        }
                        else
                        {
                            _state = State.s8;
                            HideInformer();
                        }
                    }
                }
                else if (_player.playerInteractableObjectHandler.closestInteractableObject.GetComponent<ArmorSeller>())
                {

                    ShowInformer($"Hold [Interact] to buy Power Armor");


                    if (!transform.root.GetComponent<Player>().hasArmor)
                    {
                        if (transform.root.GetComponent<PlayerSwarmMatchStats>().points >= _player.playerInteractableObjectHandler.closestInteractableObject.GetComponent<ArmorSeller>().cost)
                            ShowInformer($"Buy Power Armor [Cost: {_player.playerInteractableObjectHandler.closestInteractableObject.GetComponent<ArmorSeller>().cost}]");
                        else
                            transform.root.GetComponent<PlayerUI>().ShowInformer($"Not enough points [Cost: {_player.playerInteractableObjectHandler.closestInteractableObject.GetComponent<ArmorSeller>().cost}]");
                    }
                    else
                    {
                        transform.root.GetComponent<PlayerUI>().ShowInformer($"You already have a Power Armor");
                    }
                }
            }
        }


        if (GameManager.instance.gameType == GameManager.GameType.CTF && CurrentRoomManager.instance.gameStarted)
        {
            if (_player.team == GameManager.Team.Red)
            {
                _yourFlagStolenHolder.SetActive(GameManager.instance.redFlag.state != Flag.State.atbase);

                if (GameManager.instance.redFlag.state == Flag.State.away) _yourFlagStolenText.text = "Your flag is away";
                if (GameManager.instance.redFlag.state == Flag.State.stolen) _yourFlagStolenText.text = "The enemy has your flag!";
            }
        }

        _editorText.text = $"hp: {_player.hitPoints}";
    }




    void OnClosestInteractableObjectAssigned(PlayerInteractableObjectHandler pioh)
    {
        //if (pioh.closestInteractableObject == null)
        //{
        //    HideInformer();

        //}
        //else
        //{
        //    print($"OnClosestInteractableObjectAssigned {pioh.closestInteractableObject.name}");

        //    if (pioh.closestInteractableObject.GetComponent<LootableWeapon>())
        //    {




        //        if (GetComponent<Player>().playerInventory.activeWeapon.isDualWieldable && (pioh.closestInteractableObject.GetComponent<LootableWeapon>().codeName.Equals(GetComponent<Player>().playerInventory.activeWeapon.codeName)))
        //        {
        //            ShowInformer($"You are stepping on a DW weapon");

        //        }
        //        else
        //        {

        //            ShowInformer($"Hold [Interact] to swap for ", transform.GetComponent<Player>().playerInventory.GetWeaponProperties(pioh.closestInteractableObject.GetComponent<LootableWeapon>().codeName).weaponIcon);
        //        }
        //    }
        //    else if (pioh.closestInteractableObject.GetComponent<ArmorSeller>())
        //    {

        //        ShowInformer($"Hold [Interact] to buy Power Armor");


        //        if (!transform.root.GetComponent<Player>().hasArmor)
        //        {
        //            if (transform.root.GetComponent<PlayerSwarmMatchStats>().points >= pioh.closestInteractableObject.GetComponent<ArmorSeller>().cost)
        //                ShowInformer($"Buy Power Armor [Cost: {pioh.closestInteractableObject.GetComponent<ArmorSeller>().cost}]");
        //            else
        //                transform.root.GetComponent<PlayerUI>().ShowInformer($"Not enough points [Cost: {pioh.closestInteractableObject.GetComponent<ArmorSeller>().cost}]");
        //        }
        //        else
        //        {
        //            transform.root.GetComponent<PlayerUI>().ShowInformer($"You already have a Power Armor");
        //        }
        //    }
        //}
    }

    public void ShowPickedUpAmmoWitness(int amm, bool leftWeapon)
    {
        if (!leftWeapon)
        {
            pickedUpAmmoWitnessText.text = $"+{amm}";
            pickedUpAmmoWitnessText.GetComponent<Animator>().Play("show");
        }
        else
        {
            _pickedUpLeftWeaponAmmoWitnessText.text = $"+{amm}";
            _pickedUpLeftWeaponAmmoWitnessText.GetComponent<Animator>().Play("show");
        }
    }

    public void ShowPickedUpGrenadeWitness(int amm)
    {
        pickedUpGrenadeWitnessText.text = $"+{amm}";
        pickedUpGrenadeWitnessText.GetComponent<Animator>().Play("show");
    }


    void OnPlayerRespawningInOneSecond(Player p)
    {
        if (GameManager.instance.nbLocalPlayersPreset == 1 || GameManager.instance.nbLocalPlayersPreset == 4)
            _blackscreenDefault.Play("play");
        else if (GameManager.instance.nbLocalPlayersPreset == 3)
        {
            if (p.playerController.rid == 0)
                _blackscreenSplitScreen.Play("play");
            else
                _blackscreenDefault.Play("play");
        }
        else
        {
            _blackscreenSplitScreen.Play("play");
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class WeaponProperties : MonoBehaviour
{
    public static int RECOIL_FRAMES = 6;
    public static float OVERCHARGE_TIME_LOW = 0.3f;
    public static float OVERCHARGE_TIME_FULL = 0.9f;


    public static int GRENADE_THROW_FORCE = 625;
    public static int ROCKET_LAUNCHER_CONTINUOUS_FORCE = 1500;

    public delegate void WeaponPropertiesEvent(WeaponProperties weaponProperties);
    public WeaponPropertiesEvent OnCurrentAmmoChanged, OnSpareAmmoChanged;

    //Enums
    public enum WeaponType { AR, DMR, Pistol, SMG, Shotgun, Sniper, LMG, Launcher, Heavy, None }
    public enum FiringMode { Auto, Burst, Single }
    public enum AmmoType { Heavy, Light, Power }
    public enum AmmoReloadType { Magazine, Shell, Single, Generic }
    public enum AmmoProjectileType { Bullet, Grenade, Rocket, Plasma }
    public enum AimingMechanic { None, Zoom, Scope }
    public enum IdleHandlingAnimationType { Rifle, Pistol }
    public enum ScopeMagnification { None, Close, Medium, Long }
    public enum PlasmaColor { Red, Green, Blue, Shard }
    public enum KillFeedOutput
    {
        Unassigned,
        Pistol, SMG, Assault_Rifle, Battle_Rifle, Sniper, RPG, Shotgun, Grenade_Launcher,
        Oddball, Splinter, Plasma_Rifle, Plasma_Blaster, Barrel, Ultra_Bind, Frag_Grenade,
        Plasma_Grenade, Melee, Stuck, Assasination, Plasma_Pistol, Revolver, Sword, Flag, Minigun, Killbox, Plasma_Pistol_Overcharged
    }

    public Player player { get { return pController.player; } }

    public Crosshair crosshair;

    float _aimAssistRadius;

    [Header("Weapon Info")]
    public Sprite weaponIcon;
    public string codeName; // Used for scripting purposes
    public string cleanName; // Used for UI purposes
    public WeaponType weaponType;
    public FiringMode firingMode;
    public KillFeedOutput killFeedOutput;
    public int damage = 50;
    public float shieldDamageMultiplier;
    public int bulletSize;
    public int numberOfPellets = 1;
    public int bulletSpeed = 250;
    public float range;
    public bool isShotgun, overcharge;
    public float redReticuleDefaultRadius, redReticuleScopedRadius;
    public bool targetTracking;
    public float trackingSpeed;
    public Transform trackingTarget;

    [Header("Ammo")]
    public AmmoType ammoType;
    public AmmoProjectileType ammoProjectileType;
    public PlasmaColor plasmaColor;
    public AmmoReloadType ammoReloadType;
    [SerializeField] int _currentAmmo;
    [SerializeField] int _spareAmmo;
    [SerializeField] int _maxAmmo;
    public int ammoCapacity;
    public float bulletSpray;
    public bool injectLootedAmmo, hybridHitscan;
    public int overheatPerShot;
    public GameObject overheatSteamHolder, tpsEquippedOverheatSteamHolder, tpsHolsteredOverheatSteamHolder;
    public float overheatCooldown;

    [Header("Range")]
    public bool fakeRRR;
    public float defaultRedReticuleRange;
    public float currentRedReticuleRange; // switches between default and scoped RRR

    [Header("Aiming")]
    public AimingMechanic aimingMechanic;
    public ScopeMagnification scopeMagnification;
    public float scopeFov;
    public float scopeRRR;
    public bool isHeadshotCapable;
    public float headshotMultiplier;
    public bool hasBloom;
    public float bloomIncrement;
    public float bloomAcceleration;
    public float bloomDecceleration;
    public float maxBloom;
    public float bloom;
    float bloomDecreaseTick, bloomIncreaseTick;
    [Tooltip("In Degrees")]
    public int scopeSway; // Weapon sway is the weapon moving all on its own while you just aim down sight.

    [Header("Recoil Behaviour")]
    public float yRecoil;
    public float xRecoil;
    public PlayerCamera camScript;

    [Header("Sounds")]
    public AudioClip draw;
    public AudioClip Fire;
    public AudioClip ReloadShort;
    public AudioClip ReloadLong;
    public AudioClip holster;

    [Header("Firing Mode")]
    public int fireRate; // To be used later to replace old variables

    [Header("Reload Properties")]
    public ReloadScript reloadScript;

    [Header("Components")]
    public PlayerController pController;

    [Header("Animation")]
    public IdleHandlingAnimationType idleHandlingAnimationType;

    [Header("Dual Wielding")]
    public GameObject rightHandGO;
    public GameObject leftHandGO;
    public bool isDualWieldable, ultraBind;
    public WeaponProperties leftWeapon;
    public WeaponProperties rightWeapon;

    public GameObject equippedModel;
    public GameObject holsteredModel;

    public GameObject weaponRessource;
    public GameObject fpsMuzzleFlash, tpsMuzzleFlash;

    public int degradingDamageStart, degradedDamage;


    public int loadedAmmo
    {
        get { return _currentAmmo; }
        set
        {
            if (SceneManager.GetActiveScene().buildIndex > 0)
            {
                if (injectLootedAmmo && (GameManager.instance.gameType == GameManager.GameType.Rockets
                || GameManager.instance.gameType == GameManager.GameType.Shotguns
                || GameManager.instance.gameType == GameManager.GameType.GunGame
                || GameManager.instance.gameType == GameManager.GameType.PurpleRain))
                {
                    _currentAmmo = ammoCapacity;
                    OnCurrentAmmoChanged?.Invoke(this);
                    pController.GetComponent<PlayerUI>().activeAmmoText.text = loadedAmmo.ToString();
                    return;
                }
            }

            Log.Print(() => $"{name} loaded ammo {_currentAmmo} -> {value}");
            _currentAmmo = Mathf.Clamp(value, 0, ammoCapacity);

            if ((!player.playerInventory.leftWeapon) || (player.playerInventory.leftWeapon && player.playerInventory.leftWeapon != this))
            {
                OnCurrentAmmoChanged?.Invoke(this);
                if (this == player.playerInventory.activeWeapon)
                    pController.GetComponent<PlayerUI>().activeAmmoText.text = loadedAmmo.ToString();
            }
            else if (player.playerInventory.leftWeapon && player.playerInventory.leftWeapon == this)
            {
                player.GetComponent<PlayerUI>().leftActiveAmmoText.text = loadedAmmo.ToString();
                player.GetComponent<PlayerUI>().leftSpareAmmoText.text = spareAmmo.ToString();
            }

            if (((_currentAmmo == 0 || _currentAmmo == ammoCapacity)
                || ammoReloadType == AmmoReloadType.Shell))
            {
                Log.Print(() => $"player {player.name} {player.isMine} about to send UPDATEAMMO RPC");
                if (player.isMine) NetworkGameManager.instance.UpdateAmmo(player.photonId, index, _currentAmmo,
                    isThirdWeapon: player.playerInventory.thirdWeapon && this == player.playerInventory.thirdWeapon, sender: true);
            }
        }
    }

    public int spareAmmo
    {
        get { return _spareAmmo; }
        set
        {
            if (SceneManager.GetActiveScene().buildIndex > 0)
            {
                if (GameManager.instance.gameType == GameManager.GameType.GunGame
                    || GameManager.instance.gameType == GameManager.GameType.Snipers
                    || GameManager.instance.gameType == GameManager.GameType.Rockets
                    || GameManager.instance.gameType == GameManager.GameType.Shotguns)
                {
                    _spareAmmo = maxSpareAmmo;
                    OnSpareAmmoChanged?.Invoke(this);
                    pController.GetComponent<PlayerUI>().spareAmmoText.text = spareAmmo.ToString();
                    return;
                }
            }

            int preVal = _spareAmmo; int newVal = value;

            try { if (GameManager.instance.gameType == GameManager.GameType.GunGame && newVal < preVal) return; } catch { }


            _spareAmmo = Mathf.Clamp(value, 0, _maxAmmo);
            OnSpareAmmoChanged?.Invoke(this);

            if (isActiveWeapon && !isLeftWeapon)
                pController.GetComponent<PlayerUI>().spareAmmoText.text = spareAmmo.ToString();

            if (player.isMine)
            {
                //UpdateAmmo(index, _spareAmmo, true);
            }
        }
    }

    public int maxSpareAmmo
    {
        get { return _maxAmmo; }
        set { _maxAmmo = value; }
    }

    public bool isOutOfAmmo
    {
        get { return loadedAmmo <= 0; }
    }

    public float aimAssistRadius
    {
        get { return _aimAssistRadius; }
        set { _aimAssistRadius = Mathf.Clamp(value, 0.1f, 10); }
    }

    public int index { get { return _index; } set { _index = value; } }
    public int previousLayer { get { return _preLayer; } set { _preLayer = value; } }
    public bool isActiveWeapon { get { return this == player.playerInventory.activeWeapon; } }
    public bool isLeftWeapon { get { return this == player.playerInventory.leftWeapon; } }
    public Animator animator { get { if (!_animator) _animator = GetComponent<Animator>(); return _animator; } }
    public bool hipSprayOnly;
    public bool degradingDamage;


    public int currentOverheat
    {
        get { return _currentOverheat; }
        set
        {
            _currentOverheat = value;
            Log.Print(() => $"Overheat {_currentOverheat} -> {value}");

            if (_currentOverheat >= 100 && overheatCooldown <= 0 && player.isMine)
                StartCoroutine(TriggerOverheat_Coroutine(this)); // Allow time for networking purposes
        }
    }


    public int _currentOverheat;

    int _index, _preLayer, _recoilCount;
    Animator _animator;

    bool _spawnMuzzleFlashNetFrame;


    private void Start()
    {
        _animator = GetComponent<Animator>();
        if (fireRate <= 0)
            fireRate = 600;
        //delayBetweenBullets = 1f / fireRate;
        //Log.Print(() =>$"Delay Between Bullets: {delayBetweenBullets}");

        if (headshotMultiplier <= 0)
            headshotMultiplier = 1;

        currentRedReticuleRange = defaultRedReticuleRange;

        pController.OnControllerTypeChangedToController += OnControllerTypeChanged;
        pController.OnControllerTypeChangedToMouseAndKeyboard += OnControllerTypeChanged;


        try
        {
            foreach (CrosshairStick cs in crosshair.GetComponentsInChildren<CrosshairStick>(true))
                cs.weaponProperties = this;
        }
        catch { }
    }

    private void Update()
    {
        if (_recoilCount > 0)
        {
            if (_recoilCount > RECOIL_FRAMES)
            {
                if (camScript)
                    if (xRecoil > 0 || yRecoil > 0)
                    {
                        float horRecoil = Random.Range(-xRecoil, xRecoil);
                        float verRecoil = -yRecoil;

                        if (pController.isCrouching)
                        {
                            verRecoil *= 0.8f;
                            horRecoil *= 0.8f;
                        }

                        player.playerCamera.AddToUpDownRotation(verRecoil);
                        player.playerCamera.AddToLeftRightRotation(horRecoil);
                    }
            }
            else
            {
                if (camScript && yRecoil > 0)
                {
                    float verRecoil = -yRecoil;

                    if (pController.isCrouching)
                    {
                        verRecoil *= 0.8f;
                    }
                    player.playerCamera.AddToUpDownRotation(-verRecoil * 0.65f);
                }
            }

            _recoilCount--;
        }





        BloomIncrease();
        BloomDecrease();
    }

    private void FixedUpdate()
    {
        ShowMuzzleFlash();
    }


    public void HandleOverheat()
    {
        if (_currentOverheat > 0) _currentOverheat -= (int)(Time.deltaTime * 100);

        if (overheatCooldown > 0) overheatCooldown -= Time.deltaTime;

        overheatSteamHolder.SetActive(overheatCooldown > 0);
        tpsHolsteredOverheatSteamHolder.SetActive(overheatCooldown > 0);
        tpsEquippedOverheatSteamHolder.SetActive(overheatCooldown > 0);
    }

    public void ResetOverheat()
    {
        currentOverheat = 0;
        overheatCooldown = 0;
    }


    public void Recoil()
    {
        if (firingMode == FiringMode.Burst && _recoilCount > 0)
        {
            Log.Print(() => "Recoil");
        }
        else
        {
            _recoilCount = RECOIL_FRAMES * 2;
        }
    }

    void BloomIncrease()
    {
        if (pController.pInventory.activeWeapon.firingMode == FiringMode.Auto)
            return;

        if (bloom <= 0) bloom = 0;

        bloomIncreaseTick -= Time.deltaTime;

        if (bloomIncreaseTick <= 0)
        {
            bloom = Mathf.Clamp(bloom + bloomAcceleration, 0, maxBloom);
            bloomIncreaseTick = 0.05f;
        }
    }

    void BloomDecrease()
    {
        if (bloom <= 0) bloom = 0;

        bloomDecreaseTick -= Time.deltaTime;

        if (bloomDecreaseTick <= 0)
        {
            if (bloom > 0)
            {
                bloom -= bloomDecceleration;
                bloomAcceleration -= bloomDecceleration;

                if (bloomAcceleration < 0) bloomAcceleration = 0;
            }

            bloomDecreaseTick = 0.05f;
        }
    }
    public void SpawnMuzzleflash()
    {
        if (fpsMuzzleFlash)
        {
            DisableMuzzleFlash();
            _spawnMuzzleFlashNetFrame = true;
            //StartCoroutine(SpawnMuzzleflash_Coroutine());
        }
    }

    void ShowMuzzleFlash()
    {
        if (_spawnMuzzleFlashNetFrame)
        {
            _spawnMuzzleFlashNetFrame = false;
            fpsMuzzleFlash.SetActive(true);
            tpsMuzzleFlash.SetActive(true);
        }
    }

    public void DisableMuzzleFlash()
    {
        if (fpsMuzzleFlash)
        {
            fpsMuzzleFlash.SetActive(false);
            tpsMuzzleFlash.SetActive(false);
        }
    }

    IEnumerator SpawnMuzzleflash_Coroutine()
    {
        DisableMuzzleFlash();

        yield return new WaitForEndOfFrame();

        fpsMuzzleFlash.SetActive(true);
        tpsMuzzleFlash.SetActive(true);
    }

    IEnumerator TriggerOverheat_Coroutine(WeaponProperties wp)
    {
        if (player.playerInventory.thirdWeapon) Log.Print(() => $"TriggerOverheat_Coroutine {wp == player.playerInventory.thirdWeapon} {wp.name} VS {player.playerInventory.thirdWeapon.name}");
        yield return new WaitForSeconds(0.2f);

        NetworkGameManager.instance.TriggerPlayerOverheatWeapon(player.photonId, (int)wp.killFeedOutput,
            leftHand: player.playerInventory.thirdWeapon && wp == player.playerInventory.thirdWeapon);
    }

    public Quaternion GetRandomSprayRotation()
    {
        float currentSpray = bulletSpray + bloom;
        if (hasBloom)
        {
            if (pController.pInventory.activeWeapon.firingMode == FiringMode.Auto)
                bloom = Mathf.Clamp(bloom + bloomIncrement, 0, maxBloom);
            else
                bloomAcceleration += bloomIncrement;
        }

        if (pController.isCrouching)
            currentSpray *= 0.75f;

        float ranX = Random.Range(-currentSpray, currentSpray);
        float ranY = Random.Range(-currentSpray, currentSpray);

        Quaternion ranSprayRotation = new Quaternion();
        ranSprayRotation.eulerAngles = new Vector3(ranX, ranY, 0) * (player.isDualWielding ? 1.8f : 1);

        return ranSprayRotation;
    }

    public int GetNumberOfBulletsToShoot()
    {
        return numberOfPellets;
    }


    public void UpdateLoadedAmmo(int a)
    {
        _currentAmmo = a;
    }
    public void UpdateSpareAmmo(int a)
    {
        _spareAmmo = a;
    }

    public void TriggerOverheat()
    {
        if (!player.isDead && !player.isRespawning && overheatCooldown <= 0)
        {
            Log.Print(() => "TriggerOverheat");
            overheatCooldown = 1.7f;
            overheatSteamHolder.SetActive(true);
            tpsEquippedOverheatSteamHolder.SetActive(true);
            tpsHolsteredOverheatSteamHolder.SetActive(true);
        }
    }





    public void OnTeamMateHitbox_Delegate(AimAssistCone aimAssistCone)
    {
        try
        {
            crosshair.color = Crosshair.Color.Green;
        }
        catch { }
    }
    private void OnEnable()
    {
        try
        {
            previousLayer = gameObject.layer;
            crosshair.gameObject.SetActive(true);
            crosshair.color = Crosshair.Color.Blue;
            equippedModel.SetActive(true);
        }
        catch
        {

        }

        try
        {
            if (GameManager.instance.gameMode == GameManager.GameMode.Coop)
                foreach (PlayerArmorPiece pap in GetComponentsInChildren<PlayerArmorPiece>(true))
                    pap.gameObject.SetActive(player.hasArmor && player.playerArmorManager.playerDataCell.playerExtendedPublicData.armorDataString.Contains(pap.entity));
        }
        catch { }

        if (GameManager.instance.gameType == GameManager.GameType.Fiesta && codeName.Equals("rpg") && ammoCapacity > 0) { ammoCapacity = 2; }

    }

    private void OnDisable()
    {
        _recoilCount = 0;

        try
        {
            crosshair.gameObject.SetActive(false);
            equippedModel.SetActive(false);
        }
        catch
        {

        }
    }

    void OnControllerTypeChanged(PlayerController playerController)
    {

    }

    public static Dictionary<string, int> spriteIdDic = new Dictionary<string, int>()
    {
        {"scar", 0 }, {"ak47", 3 }, {"m4", 4 }, {"rpg", 5 }, {"m1100", 6 },
        {"m1911", 8 }, {"mp5", 13 }, {"m249c", 15 }, {"r700", 17 }, {"barrett50cal", 18 },
        {"patriot", 19 }, {"colt", 20 }, {"m16", 21 }
    };








    public struct WeaponPropertiesSpawnStruc
    {

    }
}

#if UNITY_EDITOR // Reference: https://answers.unity.com/questions/1169764/type-or-namespace-unityeditor-could-not-be-found-w.html

[CustomEditor(typeof(WeaponProperties))]
public class WeaponPropertiesEditor : Editor
{
    // In the example the Inspector will show a slider only if flag is set to true.
    // Reference code: https://answers.unity.com/questions/192895/hideshow-properties-dynamically-in-inspector.html
    public override void OnInspectorGUI()
    {
        var wp = target as WeaponProperties;

        EditorGUILayout.LabelField("Other Scripts", EditorStyles.boldLabel);
        wp.pController = EditorGUILayout.ObjectField(wp.pController, typeof(PlayerController), false) as PlayerController;
        wp.reloadScript = EditorGUILayout.ObjectField(wp.reloadScript, typeof(ReloadScript), false) as ReloadScript;
        wp.camScript = EditorGUILayout.ObjectField(wp.camScript, typeof(PlayerCamera), false) as PlayerCamera;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Info", EditorStyles.boldLabel);
        wp.cleanName = EditorGUILayout.TextField("Clean Name:", wp.cleanName);
        wp.codeName = EditorGUILayout.TextField("Code Name:", wp.codeName);
        wp.fireRate = EditorGUILayout.IntField("Fire Rate:", wp.fireRate);
        wp.killFeedOutput = (WeaponProperties.KillFeedOutput)EditorGUILayout.EnumPopup("Kill Feed Output", wp.killFeedOutput);
        wp.weaponType = (WeaponProperties.WeaponType)EditorGUILayout.EnumPopup("Weapon Type", wp.weaponType);
        wp.firingMode = (WeaponProperties.FiringMode)EditorGUILayout.EnumPopup("Firing mode", wp.firingMode);
        wp.isShotgun = GUILayout.Toggle(wp.isShotgun, "Is shotgun");
        if (wp.isShotgun)
        {
            wp.numberOfPellets = EditorGUILayout.IntField("Pellets:", wp.numberOfPellets);
        }


        wp.overcharge = GUILayout.Toggle(wp.overcharge, "Overcharge");


        wp.redReticuleDefaultRadius = EditorGUILayout.FloatField("Red Reticule Default Angle:", wp.redReticuleDefaultRadius);
        if (wp.aimingMechanic != WeaponProperties.AimingMechanic.None)
            wp.redReticuleScopedRadius = EditorGUILayout.FloatField("Red Reticule Scoped Angle:", wp.redReticuleScopedRadius);

        wp.targetTracking = GUILayout.Toggle(wp.targetTracking, "Target Tracking");
        if (wp.targetTracking)
        {
            wp.trackingTarget = EditorGUILayout.ObjectField(wp.trackingTarget, typeof(Transform), false) as Transform;
            wp.trackingSpeed = EditorGUILayout.FloatField("Tracking speed:", wp.trackingSpeed);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Ammo", EditorStyles.boldLabel);
        wp.weaponIcon = (Sprite)EditorGUILayout.ObjectField("Weapon Icon", wp.weaponIcon, typeof(Sprite), false);
        wp.ammoType = (WeaponProperties.AmmoType)EditorGUILayout.EnumPopup("Ammo type", wp.ammoType);
        wp.ammoReloadType = (WeaponProperties.AmmoReloadType)EditorGUILayout.EnumPopup("Ammo reload type", wp.ammoReloadType);
        wp.ammoProjectileType = (WeaponProperties.AmmoProjectileType)EditorGUILayout.EnumPopup("Ammo projectile type", wp.ammoProjectileType);

        if (wp.ammoProjectileType == WeaponProperties.AmmoProjectileType.Plasma)
        {
            //wp.shieldDamageMultiplier = 1;

            wp.plasmaColor = (WeaponProperties.PlasmaColor)EditorGUILayout.EnumPopup("Plasma Color", wp.plasmaColor);
            wp.shieldDamageMultiplier = EditorGUILayout.FloatField("Shield Damage Mult:", wp.shieldDamageMultiplier);

            if (wp.plasmaColor != WeaponProperties.PlasmaColor.Shard)
            {
                wp.overheatPerShot = EditorGUILayout.IntField("Overheat Per Shot", wp.overheatPerShot);
                wp._currentOverheat = EditorGUILayout.IntField("Overheat", wp._currentOverheat);
                wp.overheatCooldown = EditorGUILayout.FloatField("Overheat Cooldown", wp.overheatCooldown);

                EditorGUILayout.LabelField("FPS Overheat Steam", EditorStyles.boldLabel);
                wp.overheatSteamHolder = EditorGUILayout.ObjectField(wp.overheatSteamHolder, typeof(GameObject), true) as GameObject;

                EditorGUILayout.LabelField("TPS Equipped Overheat Steam", EditorStyles.boldLabel);
                wp.tpsEquippedOverheatSteamHolder = EditorGUILayout.ObjectField(wp.tpsEquippedOverheatSteamHolder, typeof(GameObject), true) as GameObject;

                EditorGUILayout.LabelField("TPS Holstered Overheat Steam", EditorStyles.boldLabel);
                wp.tpsHolsteredOverheatSteamHolder = EditorGUILayout.ObjectField(wp.tpsHolsteredOverheatSteamHolder, typeof(GameObject), true) as GameObject;
            }
        }

        wp.loadedAmmo = EditorGUILayout.IntField("Loaded Ammo:", wp.loadedAmmo);
        wp.ammoCapacity = EditorGUILayout.IntField("Ammo Capacity:", wp.ammoCapacity);

        wp.injectLootedAmmo = GUILayout.Toggle(wp.injectLootedAmmo, "Inject Looted Ammo");

        if (!wp.injectLootedAmmo)
        {
            wp.spareAmmo = EditorGUILayout.IntField("Spare Ammo:", wp.spareAmmo);
            wp.maxSpareAmmo = EditorGUILayout.IntField("Max Spare Ammo:", wp.maxSpareAmmo);
        }
        else
        {
            wp.spareAmmo = wp.maxSpareAmmo = 0;
        }

        wp.damage = EditorGUILayout.IntField("Bullet damage:", wp.damage);
        if (wp.bulletSize <= 0) wp.bulletSize = 1;
        wp.bulletSize = EditorGUILayout.IntField("Bullet size:", wp.bulletSize);
        wp.bulletSpeed = EditorGUILayout.IntField("Bullet speed:", wp.bulletSpeed);
        wp.hybridHitscan = GUILayout.Toggle(wp.hybridHitscan, "Hybrid Hitscan");
        wp.range = EditorGUILayout.FloatField("Bullet range:", wp.range);
        wp.bulletSpray = EditorGUILayout.FloatField("Bullet spray:", wp.bulletSpray);
        wp.hipSprayOnly = GUILayout.Toggle(wp.hipSprayOnly, "Hip Spray Only");


        EditorGUILayout.Space();
        wp.degradingDamage = GUILayout.Toggle(wp.degradingDamage, "Degrading Damage");
        if (wp.degradingDamage)
        {
            wp.degradingDamageStart = EditorGUILayout.IntField("Degraded damage start:", wp.degradingDamageStart);
            wp.degradedDamage = EditorGUILayout.IntField("Degraded damage:", wp.degradedDamage);
        }


        EditorGUILayout.Space();
        wp.isHeadshotCapable = GUILayout.Toggle(wp.isHeadshotCapable, "Is Headshot Capable");
        if (wp.isHeadshotCapable)
            wp.headshotMultiplier = EditorGUILayout.FloatField("Headshot Multiplier:", wp.headshotMultiplier);



        wp.hasBloom = GUILayout.Toggle(wp.hasBloom, "Has bloom");
        if (wp.hasBloom)
        {
            wp.maxBloom = EditorGUILayout.FloatField("Max Bloom:", wp.maxBloom);
            wp.bloom = EditorGUILayout.FloatField("Bloom:", wp.bloom);
            wp.bloomIncrement = EditorGUILayout.FloatField("Bloom Increment:", wp.bloomIncrement);
            //wp.bloomAcceleration = EditorGUILayout.FloatField("Bloom Increase:", wp.bloomAcceleration);
            wp.bloomDecceleration = EditorGUILayout.FloatField("Bloom Decceleration:", wp.bloomDecceleration);
        }

        wp.isDualWieldable = GUILayout.Toggle(wp.isDualWieldable, "Is Dual WieldAble");
        if (wp.isDualWieldable)
        {
            wp.leftWeapon = EditorGUILayout.ObjectField("Left Weapon", wp.leftWeapon, typeof(WeaponProperties), true) as WeaponProperties;
            wp.rightWeapon = EditorGUILayout.ObjectField("Right Weapon", wp.rightWeapon, typeof(WeaponProperties), true) as WeaponProperties;
        }

        wp.ultraBind = GUILayout.Toggle(wp.ultraBind, "Ultra Bind");

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Aiming", EditorStyles.boldLabel);
        wp.crosshair = EditorGUILayout.ObjectField("Crosshair", wp.crosshair, typeof(Crosshair), true) as Crosshair;
        wp.fakeRRR = GUILayout.Toggle(wp.fakeRRR, "Fake RRR");
        wp.defaultRedReticuleRange = EditorGUILayout.FloatField("Default RRR:", wp.defaultRedReticuleRange);
        wp.aimingMechanic = (WeaponProperties.AimingMechanic)EditorGUILayout.EnumPopup("Aiming mechanic", wp.aimingMechanic);

        if (wp.aimingMechanic != WeaponProperties.AimingMechanic.None)
        {
            wp.scopeMagnification = (WeaponProperties.ScopeMagnification)EditorGUILayout.EnumPopup("Scope Magnification", wp.scopeMagnification);
            //wp.scopeFov = EditorGUILayout.FloatField("Scope FOV:", wp.scopeFov);
            wp.scopeRRR = EditorGUILayout.FloatField("Scope RRR:", wp.scopeRRR);
            //wp.scopeSway = EditorGUILayout.IntField("Scope sway:", wp.scopeSway);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Recoil", EditorStyles.boldLabel);
        wp.yRecoil = EditorGUILayout.FloatField("Vertical Recoil:", wp.yRecoil);
        wp.xRecoil = EditorGUILayout.FloatField("Horizontal Recoil:", wp.xRecoil);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Model", EditorStyles.boldLabel);
        wp.weaponRessource = EditorGUILayout.ObjectField("Ressource", wp.weaponRessource, typeof(GameObject), true) as GameObject;
        wp.idleHandlingAnimationType = (WeaponProperties.IdleHandlingAnimationType)EditorGUILayout.EnumPopup("Idle Handling Type", wp.idleHandlingAnimationType);
        wp.equippedModel = EditorGUILayout.ObjectField("Equipped model", wp.equippedModel, typeof(GameObject), true) as GameObject;
        wp.holsteredModel = EditorGUILayout.ObjectField("Holstered model", wp.holsteredModel, typeof(GameObject), true) as GameObject;



        wp.fpsMuzzleFlash = EditorGUILayout.ObjectField("Fps Muzzleflash", wp.fpsMuzzleFlash, typeof(GameObject), true) as GameObject;
        wp.tpsMuzzleFlash = EditorGUILayout.ObjectField("Tps Muzzleflash", wp.tpsMuzzleFlash, typeof(GameObject), true) as GameObject;



        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Audio", EditorStyles.boldLabel);
        wp.Fire = EditorGUILayout.ObjectField("Fire", wp.Fire, typeof(AudioClip), true) as AudioClip;
        wp.ReloadShort = EditorGUILayout.ObjectField("Reload short", wp.ReloadShort, typeof(AudioClip), true) as AudioClip;
        wp.draw = EditorGUILayout.ObjectField("Draw", wp.draw, typeof(AudioClip), true) as AudioClip;


        EditorUtility.SetDirty(wp);
    }
}
#endif



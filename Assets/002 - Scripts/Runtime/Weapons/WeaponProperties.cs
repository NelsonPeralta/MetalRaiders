using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;


public class WeaponProperties : MonoBehaviour
{
    public delegate void WeaponPropertiesEvent(WeaponProperties weaponProperties);
    public WeaponPropertiesEvent OnCurrentAmmoChanged, OnSpareAmmoChanged;

    //Enums
    public enum WeaponType { AR, DMR, Pistol, SMG, Shotgun, Sniper, LMG, Launcher, None }
    public enum FiringMode { Auto, Burst, Single }
    public enum AmmoType { Heavy, Light, Power }
    public enum AmmoReloadType { Magazine, Shell, Single }
    public enum AmmoProjectileType { Bullet, Grenade, Rocket }
    public enum AimingMechanic { None, Zoom, Scope }
    public enum IdleHandlingAnimationType { Rifle, Pistol }

    public Player player { get { return pController.GetComponent<Player>(); } }

    public Crosshair crosshair;

    float _aimAssistRadius;

    [Header("Weapon Info")]
    public Sprite weaponIcon;
    public string codeName; // Used for scripting purposes
    public string cleanName; // Used for UI purposes
    public WeaponType weaponType;
    public FiringMode firingMode;
    public int damage = 50;
    public int bulletSize;
    public int numberOfPellets = 1;
    public int bulletSpeed = 250;
    public float range;
    public bool isShotgun;
    public float redReticuleHint;
    float _previousRedReticuleHint;

    [Header("Ammo")]
    public AmmoType ammoType;
    public AmmoProjectileType ammoProjectileType;
    public AmmoReloadType ammoReloadType;
    [SerializeField] int _currentAmmo;
    [SerializeField] int _spareAmmo;
    [SerializeField] int _maxAmmo;
    public int ammoCapacity;
    public float bulletSpray;

    [Header("Range")]
    public bool fakeRRR;
    public float defaultRedReticuleRange;
    public float currentRedReticuleRange;

    [Header("Aiming")]
    public AimingMechanic aimingMechanic;
    public float scopeFov;
    public float scopeRRR;
    public bool isHeadshotCapable;
    public float headshotMultiplier;
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
    public GameObject equippedModelA;
    public GameObject unequippedModelA;

    [Header("Dual Wielding")]
    public GameObject rightHandGO;
    public GameObject leftHandGO;
    public bool isDualWieldable;
    public WeaponProperties leftWeapon;
    public WeaponProperties rightWeapon;

    public GameObject equippedModelB;
    public GameObject holsteredModel;

    public GameObject weaponRessource;
    public GameObject muzzleFlash;


    public int currentAmmo
    {
        get { return _currentAmmo; }
        set
        {
            _currentAmmo = value;

            if ((!player.playerInventory.leftWeapon) || (player.playerInventory.leftWeapon && player.playerInventory.leftWeapon != this))
            {
                OnCurrentAmmoChanged?.Invoke(this);
                if (this == player.playerInventory.activeWeapon)
                    pController.GetComponent<PlayerUI>().activeAmmoText.text = currentAmmo.ToString();
            }
            else if (player.playerInventory.leftWeapon && player.playerInventory.leftWeapon == this)
            {
                player.GetComponent<PlayerUI>().leftActiveAmmoText.text = currentAmmo.ToString();
                player.GetComponent<PlayerUI>().leftSpareAmmoText.text = spareAmmo.ToString();
            }

            if (player.isMine && ((_currentAmmo == 0 || _currentAmmo == ammoCapacity)
                || ammoReloadType == AmmoReloadType.Shell))
            {
                UpdateAmmo(index, _currentAmmo, sender: true);
            }
        }
    }

    public int spareAmmo
    {
        get { return _spareAmmo; }
        set
        {
            int preVal = _spareAmmo; int newVal = value;

            if (GameManager.instance.gameType == GameManager.GameType.GunGame && newVal < preVal) return;


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

    public int maxAmmo
    {
        get { return _maxAmmo; }
        set { _maxAmmo = value; }
    }

    public bool isOutOfAmmo
    {
        get { return currentAmmo <= 0; }
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

    int _index, _preLayer;

    private void Start()
    {
        if (fireRate <= 0)
            fireRate = 600;
        //delayBetweenBullets = 1f / fireRate;
        //Debug.Log($"Delay Between Bullets: {delayBetweenBullets}");

        if (headshotMultiplier <= 0)
            headshotMultiplier = 1;

        currentRedReticuleRange = defaultRedReticuleRange;

        SetCorrectLayer();

        _previousRedReticuleHint = redReticuleHint;

        pController.OnControllerTypeChangedToController += OnControllerTypeChanged;
        pController.OnControllerTypeChangedToMouseAndKeyboard += OnControllerTypeChanged;
    }

    public void Recoil()
    {
        if (camScript)
            if (xRecoil > 0 || yRecoil > 0)
            {
                float ranHorRecoil = Random.Range(-xRecoil, xRecoil);
                if (pController.isCrouching)
                {
                    camScript.xRotation -= yRecoil / 2f;
                    camScript.yRotation -= ranHorRecoil / 2;
                }
                else if (!pController.movement.isGrounded || !pController.isCrouching)
                {
                    camScript.xRotation -= yRecoil;
                    camScript.RotateCameraBy(ranHorRecoil);
                }
            }
    }

    public void SpawnMuzzleflash()
    {
        StartCoroutine(SpawnMuzzleflash_Coroutine());
    }

    IEnumerator SpawnMuzzleflash_Coroutine()
    {
        muzzleFlash.SetActive(false);
        yield return new WaitForEndOfFrame();
        muzzleFlash.SetActive(true);
    }

    public Quaternion GetRandomSprayRotation()
    {
        float currentBulletSpray = bulletSpray;

        if (pController.isCrouching)
            currentBulletSpray *= 0.75f;

        float ranX = Random.Range(-currentBulletSpray, currentBulletSpray);
        float ranY = Random.Range(-currentBulletSpray, currentBulletSpray);

        Quaternion ranSprayRotation = new Quaternion();
        ranSprayRotation.eulerAngles = new Vector3(ranX, ranY, 0);

        return ranSprayRotation;
    }

    public int GetNumberOfBulletsToShoot()
    {
        return numberOfPellets;
    }

    public void SetCorrectLayer()
    {
        if (pController.PV.IsMine)
        {
            if (pController.rid == 0)
                GameManager.SetLayerRecursively(gameObject, 24);
            else if (pController.rid == 1)
                GameManager.SetLayerRecursively(gameObject, 26);
            else if (pController.rid == 2)
                GameManager.SetLayerRecursively(gameObject, 28);
            else if (pController.rid == 3)
                GameManager.SetLayerRecursively(gameObject, 30);
        }
        else
            GameManager.SetLayerRecursively(gameObject, 3);
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
            equippedModelB.SetActive(true);
        }
        catch
        {

        }
    }

    private void OnDisable()
    {
        try
        {
            crosshair.gameObject.SetActive(false);
            equippedModelB.SetActive(false);
        }
        catch
        {

        }
    }

    void OnControllerTypeChanged(PlayerController playerController)
    {
        //Debug.Log("OnControllerTypeChanged");
        if (playerController.activeControllerType == Rewired.ControllerType.Joystick)
        {
            redReticuleHint = _previousRedReticuleHint * 0.8f;
        }
        else
        {
            redReticuleHint = _previousRedReticuleHint * 0.8f;
        }
    }

    public static Dictionary<string, int> spriteIdDic = new Dictionary<string, int>()
    {
        {"scar", 0 }, {"ak47", 3 }, {"m4", 4 }, {"rpg", 5 }, {"m1100", 6 },
        {"m1911", 8 }, {"mp5", 13 }, {"m249c", 15 }, {"r700", 17 }, {"barrett50cal", 18 },
        {"patriot", 19 }, {"colt", 20 }, {"m16", 21 }
    };

    public void UpdateAmmo(int wIndex, int ammo, bool isSpare = false, bool sender = false)
    {
        if (player.isMine)
        {
            if (sender)
            {
                player.PV.RPC("UpdateAmmo", Photon.Pun.RpcTarget.All, wIndex, ammo, isSpare, sender);
            }
        }
        else
        {
            Debug.Log($"UpdateAmmo Is Not Mine");
            if (!isSpare)
                _currentAmmo = ammo;
            else
                _spareAmmo = ammo;
        }
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
        wp.codeName = EditorGUILayout.TextField("Code Name:", wp.codeName);
        wp.cleanName = EditorGUILayout.TextField("Clean Name:", wp.cleanName);
        wp.fireRate = EditorGUILayout.IntField("Fire Rate:", wp.fireRate);
        wp.weaponType = (WeaponProperties.WeaponType)EditorGUILayout.EnumPopup("Weapon Type", wp.weaponType);
        wp.firingMode = (WeaponProperties.FiringMode)EditorGUILayout.EnumPopup("Firing mode", wp.firingMode);
        wp.isShotgun = GUILayout.Toggle(wp.isShotgun, "Is shotgun");
        if (wp.isShotgun)
        {
            wp.numberOfPellets = EditorGUILayout.IntField("Pellets:", wp.numberOfPellets);
        }
        wp.redReticuleHint = EditorGUILayout.FloatField("Red reticule hint:", wp.redReticuleHint);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Ammo", EditorStyles.boldLabel);
        wp.weaponIcon = (Sprite)EditorGUILayout.ObjectField("Weapon Icon", wp.weaponIcon, typeof(Sprite), false);
        wp.ammoType = (WeaponProperties.AmmoType)EditorGUILayout.EnumPopup("Ammo type", wp.ammoType);
        wp.ammoReloadType = (WeaponProperties.AmmoReloadType)EditorGUILayout.EnumPopup("Ammo reload type", wp.ammoReloadType);
        wp.ammoProjectileType = (WeaponProperties.AmmoProjectileType)EditorGUILayout.EnumPopup("Ammo projectile type", wp.ammoProjectileType);
        wp.currentAmmo = EditorGUILayout.IntField("Ammo:", wp.currentAmmo);
        wp.spareAmmo = EditorGUILayout.IntField("Spare Ammo:", wp.spareAmmo);
        wp.maxAmmo = EditorGUILayout.IntField("Max Ammo:", wp.maxAmmo);
        wp.ammoCapacity = EditorGUILayout.IntField("Ammo Capacity:", wp.ammoCapacity);
        wp.damage = EditorGUILayout.IntField("Bullet damage:", wp.damage);
        if (wp.bulletSize <= 0) wp.bulletSize = 1;
        wp.bulletSize = EditorGUILayout.IntField("Bullet size:", wp.bulletSize);
        wp.bulletSpeed = EditorGUILayout.IntField("Bullet speed:", wp.bulletSpeed);
        wp.range = EditorGUILayout.FloatField("Bullet range:", wp.range);
        wp.bulletSpray = EditorGUILayout.FloatField("Bullet spray:", wp.bulletSpray);

        wp.isHeadshotCapable = GUILayout.Toggle(wp.isHeadshotCapable, "Is Headshot Capable");
        if (wp.isHeadshotCapable)
            wp.headshotMultiplier = EditorGUILayout.FloatField("Headshot Multiplier:", wp.headshotMultiplier);

        wp.isDualWieldable = GUILayout.Toggle(wp.isDualWieldable, "Is Dual WieldAble");
        if (wp.isDualWieldable)
        {
            wp.leftWeapon = EditorGUILayout.ObjectField("Left Weapon", wp.leftWeapon, typeof(WeaponProperties), true) as WeaponProperties;
            wp.rightWeapon = EditorGUILayout.ObjectField("Right Weapon", wp.rightWeapon, typeof(WeaponProperties), true) as WeaponProperties;
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Aiming", EditorStyles.boldLabel);
        wp.crosshair = EditorGUILayout.ObjectField("Crosshair", wp.crosshair, typeof(Crosshair), true) as Crosshair;
        wp.fakeRRR = GUILayout.Toggle(wp.fakeRRR, "Fake RRR");
        wp.defaultRedReticuleRange = EditorGUILayout.FloatField("Default RRR:", wp.defaultRedReticuleRange);
        wp.aimingMechanic = (WeaponProperties.AimingMechanic)EditorGUILayout.EnumPopup("Aiming mechanic", wp.aimingMechanic);

        if (wp.aimingMechanic != WeaponProperties.AimingMechanic.None)
        {
            wp.scopeFov = EditorGUILayout.FloatField("Scope FOV:", wp.scopeFov);
            wp.scopeRRR = EditorGUILayout.FloatField("Scope RRR:", wp.scopeRRR);
            wp.scopeSway = EditorGUILayout.IntField("Scope sway:", wp.scopeSway);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Recoil", EditorStyles.boldLabel);
        wp.yRecoil = EditorGUILayout.FloatField("Vertical Recoil:", wp.yRecoil);
        wp.xRecoil = EditorGUILayout.FloatField("Horizontal Recoil:", wp.xRecoil);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Model", EditorStyles.boldLabel);
        wp.weaponRessource = EditorGUILayout.ObjectField("Ressource", wp.weaponRessource, typeof(GameObject), true) as GameObject;
        wp.idleHandlingAnimationType = (WeaponProperties.IdleHandlingAnimationType)EditorGUILayout.EnumPopup("Idle Handling Type", wp.idleHandlingAnimationType);
        wp.equippedModelA = EditorGUILayout.ObjectField("Equipped model A", wp.equippedModelA, typeof(GameObject), true) as GameObject;
        wp.unequippedModelA = EditorGUILayout.ObjectField("Unequipped model A", wp.unequippedModelA, typeof(GameObject), true) as GameObject;
        wp.equippedModelB = EditorGUILayout.ObjectField("Equipped model B", wp.equippedModelB, typeof(GameObject), true) as GameObject;
        wp.holsteredModel = EditorGUILayout.ObjectField("Holstered model", wp.holsteredModel, typeof(GameObject), true) as GameObject;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Audio", EditorStyles.boldLabel);
        wp.Fire = EditorGUILayout.ObjectField("Fire", wp.Fire, typeof(AudioClip), true) as AudioClip;
        wp.ReloadShort = EditorGUILayout.ObjectField("Reload short", wp.ReloadShort, typeof(AudioClip), true) as AudioClip;
        wp.draw = EditorGUILayout.ObjectField("Draw", wp.draw, typeof(AudioClip), true) as AudioClip;

        EditorUtility.SetDirty(wp);
    }
}
#endif
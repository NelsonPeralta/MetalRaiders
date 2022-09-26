using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class WeaponProperties : MonoBehaviour
{
    public delegate void WeaponPropertiesEvent(WeaponProperties weaponProperties);
    public WeaponPropertiesEvent OnCurrentAmmoChanged;

    //Enums
    public enum ReticuleType { AR, DMR, Pistol, SMG, Shotgun, Sniper, None }
    public enum FiringMode { Auto, Burst, Single }
    public enum AmmoType { Heavy, Light, Power }
    public enum AmmoReloadType { Magazine, Shell, Single }
    public enum AmmoProjectileType { Bullet, Grenade, Rocket }
    public enum AimingMechanic { None, Zoom, Scope }
    public enum IdleHandlingAnimationType { Rifle, Pistol }

    [Header("Weapon Info")]
    public string codeName; // Used for scripting purposes
    public string cleanName; // Used for UI purposes
    public ReticuleType reticuleType;
    public FiringMode firingMode;
    public int damage = 50;
    public int bulletSize;
    public int numberOfPellets = 1;
    public int bulletSpeed = 250;
    public float range;
    public bool isShotgun;
    public float redReticuleHint;

    [Header("Ammo")]
    public AmmoType ammoType;
    public AmmoProjectileType ammoProjectileType;
    public AmmoReloadType ammoReloadType;
    [SerializeField] int _currentAmmo;
    [SerializeField] int _spareAmmo;
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
    public AudioSource mainAudioSource;
    public PlayerController pController;

    [Header("Animation")]
    public IdleHandlingAnimationType idleHandlingAnimationType;
    public GameObject equippedModelA;
    public GameObject unequippedModelA;

    [Header("Dual Wielding")]
    public GameObject rightHandGO;
    public GameObject leftHandGO;
    public bool isDualWieldable;
    public bool isRightWeapon;
    public bool isLeftWeapon;


    public GameObject equippedModelB;

    public GameObject weaponRessource;

    public int currentAmmo
    {
        get { return _currentAmmo; }
        set { _currentAmmo = value; OnCurrentAmmoChanged?.Invoke(this); }
    }

    // Properties
    public bool isOutOfAmmo
    {
        get { return currentAmmo <= 0; }
    }
    private void Start()
    {
        if (fireRate <= 0)
            fireRate = 600;
        //delayBetweenBullets = 1f / fireRate;
        //Debug.Log($"Delay Between Bullets: {delayBetweenBullets}");

        if (headshotMultiplier <= 0)
            headshotMultiplier = 1;

        currentRedReticuleRange = defaultRedReticuleRange;
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

    private void OnEnable()
    {
        try
        {
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
            equippedModelB.SetActive(false);
        }
        catch
        {

        }
    }

    public static Dictionary<string, int> spriteIdDic = new Dictionary<string, int>()
    {
        {"scar", 0 }, {"ak47", 3 }, {"m4", 4 }, {"rpg", 5 }, {"m1100", 6 },
        {"m1911", 8 }, {"mp5", 13 }, {"m249c", 15 }, {"r700", 17 }, {"barrett50cal", 18 },
        {"patriot", 19 }, {"colt", 20 }, {"m16", 21 }
    };
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
        wp.reticuleType = (WeaponProperties.ReticuleType)EditorGUILayout.EnumPopup("Reticule Type", wp.reticuleType);
        wp.firingMode = (WeaponProperties.FiringMode)EditorGUILayout.EnumPopup("Firing mode", wp.firingMode);
        wp.isShotgun = GUILayout.Toggle(wp.isShotgun, "Is shotgun");
        if (wp.isShotgun)
        {
            wp.numberOfPellets = EditorGUILayout.IntField("Pellets:", wp.numberOfPellets);
        }
        wp.redReticuleHint = EditorGUILayout.FloatField("Red reticule hint:", wp.redReticuleHint);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Ammo", EditorStyles.boldLabel);
        wp.ammoType = (WeaponProperties.AmmoType)EditorGUILayout.EnumPopup("Ammo type", wp.ammoType);
        wp.ammoReloadType = (WeaponProperties.AmmoReloadType)EditorGUILayout.EnumPopup("Ammo reload type", wp.ammoReloadType);
        wp.ammoProjectileType = (WeaponProperties.AmmoProjectileType)EditorGUILayout.EnumPopup("Ammo projectile type", wp.ammoProjectileType);
        wp.currentAmmo = EditorGUILayout.IntField("Ammo:", wp.currentAmmo);
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


        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Aiming", EditorStyles.boldLabel);
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

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Audio", EditorStyles.boldLabel);
        wp.Fire = EditorGUILayout.ObjectField("Fire", wp.Fire, typeof(AudioClip), true) as AudioClip;
        wp.ReloadShort = EditorGUILayout.ObjectField("Reload short", wp.ReloadShort, typeof(AudioClip), true) as AudioClip;
        wp.draw = EditorGUILayout.ObjectField("Draw", wp.draw, typeof(AudioClip), true) as AudioClip;

        EditorUtility.SetDirty(wp);
    }
}
#endif
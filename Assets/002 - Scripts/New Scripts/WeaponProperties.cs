using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class WeaponProperties : MonoBehaviour
{
    //Enums
    public enum ReticuleType { AssaultRifle, DMR, Pistol, SMG, Shotgun, Sniper, None }
    public enum FiringMode { Auto, Burst, Single }
    public enum AmmoType { Heavy, Light, Power }
    public enum AmmoReloadType { Magazine, Shell, Single }
    public enum AmmoProjectileType { Bullet, Grenade, Rocket }
    public enum IdleHandlingAnimationType { Rifle, Pistol }

    [Header("Weapon Info")]
    public string weaponIdentity; // Used for scripting purposes
    public string weaponName; // Used for UI purposes
    public ReticuleType reticuleType;
    public FiringMode firingMode;
    public int damage = 50;
    public int numberOfPellets = 1;
    public int bulletSpeed = 250;
    public float range;
    public bool isShotgun;

    [Header("Ammo")]
    public AmmoType ammoType;
    public AmmoProjectileType ammoProjectileType;
    public AmmoReloadType ammoReloadType;
    public int currentAmmo;
    public int ammoCapacity;
    public float bulletSpray;

    [Header("Range")]
    public float defaultRedReticuleRange;
    public float currentRedReticuleRange;

    [Header("Aiming")]
    public bool canScopeIn;
    public float scopeFov;
    public float scopeRRR;
    public bool isHeadshotCapable;
    public float headshotMultiplier;
    [Tooltip("In Degrees")]
    public int scopeSway; // Weapon sway is the weapon moving all on its own while you just aim down sight.

    [Header("Recoil Behaviour")]
    public float yRecoil;
    public float xRecoil;
    public CameraScript camScript;

    [Header("Sounds")]
    public AudioClip draw;
    public AudioClip Fire;
    public AudioClip Reload_1;
    public AudioClip Reload_2;
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
    public GameObject thirdPersonModelEquipped;
    public GameObject thirdPersonModelUnequipped;

    [Header("Dual Wielding")]
    public GameObject rightHandGO;
    public GameObject leftHandGO;
    public bool isDualWieldable;
    public bool isRightWeapon;
    public bool isLeftWeapon;

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
            currentBulletSpray /= 2;

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
        wp.camScript = EditorGUILayout.ObjectField(wp.camScript, typeof(CameraScript), false) as CameraScript;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Info", EditorStyles.boldLabel);
        wp.weaponIdentity = EditorGUILayout.TextField("Weapon Identity:", wp.weaponIdentity);
        wp.weaponName = EditorGUILayout.TextField("Weapon Name:", wp.weaponName);
        wp.fireRate = EditorGUILayout.IntField("Fire Rate:", wp.fireRate);
        wp.reticuleType = (WeaponProperties.ReticuleType)EditorGUILayout.EnumPopup("Reticule Type", wp.reticuleType);
        wp.firingMode = (WeaponProperties.FiringMode)EditorGUILayout.EnumPopup("Firing mode", wp.firingMode);
        wp.isShotgun = GUILayout.Toggle(wp.isShotgun, "Is shotgun");
        if (wp.isShotgun)
        {
            wp.numberOfPellets = EditorGUILayout.IntField("Pellets:", wp.numberOfPellets);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Ammo", EditorStyles.boldLabel);
        wp.ammoType = (WeaponProperties.AmmoType)EditorGUILayout.EnumPopup("Ammo type", wp.ammoType);
        wp.ammoReloadType = (WeaponProperties.AmmoReloadType)EditorGUILayout.EnumPopup("Ammo reload type", wp.ammoReloadType);
        wp.ammoProjectileType = (WeaponProperties.AmmoProjectileType)EditorGUILayout.EnumPopup("Ammo projectile type", wp.ammoProjectileType);
        wp.currentAmmo = EditorGUILayout.IntField("Ammo:", wp.currentAmmo);
        wp.ammoCapacity = EditorGUILayout.IntField("Ammo Capacity:", wp.ammoCapacity);
        wp.damage = EditorGUILayout.IntField("Bullet damage:", wp.damage);
        wp.bulletSpeed = EditorGUILayout.IntField("Bullet speed:", wp.bulletSpeed);
        wp.range = EditorGUILayout.FloatField("Bullet range:", wp.range);
        wp.bulletSpray = EditorGUILayout.FloatField("Bullet spray:", wp.bulletSpray);
        wp.isHeadshotCapable = GUILayout.Toggle(wp.isHeadshotCapable, "Is Headshot Capable");
        if (wp.isHeadshotCapable)
            wp.headshotMultiplier = EditorGUILayout.FloatField("Headshot Multiplier:", wp.headshotMultiplier);


        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Aiming", EditorStyles.boldLabel);
        wp.defaultRedReticuleRange = EditorGUILayout.FloatField("Default RRR:", wp.defaultRedReticuleRange);
        wp.canScopeIn = GUILayout.Toggle(wp.canScopeIn, "Can scope in");

        if (wp.canScopeIn)
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
        wp.idleHandlingAnimationType = (WeaponProperties.IdleHandlingAnimationType)EditorGUILayout.EnumPopup("Idle Handling Type", wp.idleHandlingAnimationType);
        wp.thirdPersonModelEquipped = EditorGUILayout.ObjectField("Equipped model", wp.thirdPersonModelEquipped, typeof(GameObject), true) as GameObject;
        wp.thirdPersonModelUnequipped = EditorGUILayout.ObjectField("Unequipped model", wp.thirdPersonModelUnequipped, typeof(GameObject), true) as GameObject;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Audio", EditorStyles.boldLabel);
        wp.Fire = EditorGUILayout.ObjectField("Fire", wp.Fire, typeof(AudioClip), true) as AudioClip;

        EditorUtility.SetDirty(wp);
    }
}
#endif
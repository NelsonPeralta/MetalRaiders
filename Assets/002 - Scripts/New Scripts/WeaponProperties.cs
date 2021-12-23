using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class WeaponProperties : MonoBehaviour
{
    //Enums
    public enum WeaponType { AssaultRifle, DMR, Pistol, SMG, Shotgun, Sniper }
    public enum ReticuleType { AssaultRifle, DMR, Pistol, SMG, Shotgun, Sniper, None }
    public enum FiringMode { Auto, Burst, Single }
    public enum AmmoType { Power, Heavy, Light }
    public enum AmmoReloadType { Magazine, Shell, Single }
    public enum AmmoProjectileType { Bullet, Grenade, Rocket }
    public enum IdleHandlingAnimationType { Rifle, Pistol }

    [Header("Weapon Info")]
    public string weaponName;
    public string weaponUiName;
    public WeaponType weaponType;
    public ReticuleType reticuleType;
    public FiringMode firingMode;
    public int damage = 50;
    public int numberOfPellets = 1;
    public int bulletSpeed = 250;
    public float range;

    [Header("Ammo")]
    public AmmoType ammoType;
    public AmmoProjectileType ammoProjectileType;
    public AmmoReloadType ammoReloadType;
    public int currentAmmo;
    public int maxAmmoInWeapon;
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
    public float yRecoil = 1;
    public float xRecoil = 1f;
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
    public float defaultReloadTime;

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
            fireRate = 10;
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
        wp.weaponName = EditorGUILayout.TextField("Weapon Name:", wp.weaponName);
        wp.weaponUiName = EditorGUILayout.TextField("Weapon UI Name:", wp.weaponUiName);
        wp.weaponType = (WeaponProperties.WeaponType)EditorGUILayout.EnumPopup("Weapon Type", wp.weaponType);
        wp.reticuleType = (WeaponProperties.ReticuleType)EditorGUILayout.EnumPopup("Reticule Type", wp.reticuleType);
        if (wp.weaponType == WeaponProperties.WeaponType.Shotgun)
        {
            wp.numberOfPellets = EditorGUILayout.IntField("Pellets:", wp.numberOfPellets);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Ammo", EditorStyles.boldLabel);
        wp.damage = EditorGUILayout.IntField("Bullet damage:", wp.damage);
        wp.bulletSpeed = EditorGUILayout.IntField("Bullet speed:", wp.bulletSpeed);
        wp.range = EditorGUILayout.FloatField("Bullet range:", wp.range);
        wp.bulletSpray = EditorGUILayout.FloatField("Bullet spray:", wp.bulletSpray);
        wp.isHeadshotCapable = GUILayout.Toggle(wp.isHeadshotCapable, "Is Headshot Capable");
        wp.defaultReloadTime = EditorGUILayout.FloatField("Reload Time:", wp.defaultReloadTime);
        if (wp.isHeadshotCapable)
            wp.headshotMultiplier = EditorGUILayout.FloatField("I field:", wp.headshotMultiplier);


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
        wp.thirdPersonModelEquipped = EditorGUILayout.ObjectField(wp.thirdPersonModelEquipped, typeof(GameObject), false) as GameObject;
        wp.thirdPersonModelUnequipped = EditorGUILayout.ObjectField(wp.thirdPersonModelEquipped, typeof(GameObject), false) as GameObject;
    }
}
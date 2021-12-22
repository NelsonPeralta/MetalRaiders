using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class WeaponProperties : MonoBehaviour
{
    //Enums
    public enum WeaponType { AssaultRifle, DMR, Pistol, SMG, Shotgun, Sniper }
    public enum WeaponReticule{AssaultRifle, DMR, Pistol, SMG, Shotgun, Sniper, None}
    public enum IdleHandlingAnimationType { Rifle, Pistol} // TODO: Remove variable "pistolIdle" and fix dependencies

    [Header("Weapon Info")]
    public string weaponName;
    public string weaponUiName;
    public WeaponType weaponType;
    public WeaponReticule weaponReticule;
    public int damage = 50;
    public int numberOfBulletsToShoot = 1;
    public int bulletSpeed = 250;
    public float range;
    public bool pistolIdle;

    [Header("Inventory")]
    public int currentAmmo;
    public int maxAmmoInWeapon;
    public bool outOfAmmo;

    [Header("Range")]
    public float DefaultRedReticuleRange;
    public float RedReticuleRange;
    public float RRDiameter;

    [Header("Aiming")]
    public bool canAim;
    public float aimFOV;
    public float aimRRR;
    public bool isHeadshotCapable;
    public float headshotMultiplier;
    [Tooltip("In Degrees")]
    public int weaponSway; // Weapon sway is the weapon moving all on its own while you just aim down sight.

    [Header("Bullet Behavior")]
    public bool isNormalBullet;
    public bool canBleedthroughHeadshot;
    public bool canBleedthroughAnything;

    [Header("Recoil Behaviour")]
    public float bulletSpray;
    public float verticalRecoil = 1;
    public float horizontalRecoil = 1f;
    public int defaultBulletsToIgnoreRecoil; // Only for Fully Auto
    int bulletsToIgnoreRecoil;
    public CameraScript camScript;

    [Header("Sounds")]
    public AudioClip draw;
    public AudioClip Fire;
    public AudioClip Reload_1;
    public AudioClip Reload_2;
    public AudioClip holster;

    [Header("Firing Mode")]
    public int fireRate; // To be used later to replace old variables
    public float delayBetweenBullets;

    [Header("Fully Automatic Setings")]
    public bool isFullyAutomatic;
    public float timeBetweenFABullets = .01f;

    [Header("Burst Mode Settings")]
    public bool isBurstWeapon;
    public float timeBetweenBurstBullets = .01f, timeBetweenBurstCompletion = .01f;

    [Header("Single Fire Settings")]
    public bool isSingleFire;
    public float timeBetweenSingleBullets = .01f;

    [Header("Reload Properties")]
    public ReloadScript reloadScript;
    public float defaultReloadSpeed;
    public bool usesMags;
    public bool usesShells;
    public bool usesSingleAmmo;
    public bool genericReload;

    [Header("Ammo Type")]
    public AmmoType ammoType;
    public bool smallAmmo;
    public bool heavyAmmo;
    public bool powerAmmo;
    [Space(10)]
    public bool usesGrenades;
    public bool usesRockets;

    [Header("Components")]
    public AudioSource mainAudioSource;
    public PlayerController pController;


    [Header("On Player Meshes")]
    public GameObject thirdPersonModelEquipped;
    public GameObject thirdPersonModelUnequipped;

    [Header("Dual Wielding")]
    public GameObject rightHandGO;
    public GameObject leftHandGO;
    public bool isDualWieldable;
    public bool isRightWeapon;
    public bool isLeftWeapon;

    private void Start()
    {
        bulletsToIgnoreRecoil = defaultBulletsToIgnoreRecoil;
        if (fireRate <= 0)
            fireRate = 10;
        delayBetweenBullets = 1f / fireRate;
        Debug.Log($"Delay Between Bullets: {delayBetweenBullets}");

        if (headshotMultiplier <= 0)
            headshotMultiplier = 1;
        //Debug.Log($"ENUM DEBUG TEST: {weaponType}");

        if (isNormalBullet)
        {
            isNormalBullet = true;
            isHeadshotCapable = false;
            canBleedthroughHeadshot = false;
            canBleedthroughAnything = false;
        }
        else if (isHeadshotCapable)
        {
            isNormalBullet = false;
            isHeadshotCapable = true;
            canBleedthroughHeadshot = false;
            canBleedthroughAnything = false;
        }
        else if (canBleedthroughHeadshot)
        {
            isNormalBullet = false;
            isHeadshotCapable = false;
            canBleedthroughHeadshot = true;
            canBleedthroughAnything = false;
        }
        else if (canBleedthroughAnything)
        {
            isNormalBullet = false;
            isHeadshotCapable = false;
            canBleedthroughHeadshot = false;
            canBleedthroughAnything = true;
        }


        DefaultRedReticuleRange = RedReticuleRange;
    }

    public void Recoil()
    {
        if (camScript)
            if (horizontalRecoil > 0 || verticalRecoil > 0)
            {
                if (bulletsToIgnoreRecoil > 0)
                {
                    bulletsToIgnoreRecoil--;
                    return;
                }
                float ranHorRecoil = Random.Range(-horizontalRecoil, horizontalRecoil);
                if (pController.isCrouching)
                {
                    camScript.xRotation -= verticalRecoil / 2f;
                    camScript.yRotation -= ranHorRecoil / 2;
                }
                else if (!pController.movement.isGrounded || !pController.isCrouching)
                {
                    camScript.xRotation -= verticalRecoil;
                    camScript.RotateCameraBy(ranHorRecoil);
                }
            }
    }

    public string getAmmoType()
    {
        if (smallAmmo)
            return "Small";
        else if (heavyAmmo)
            return "Heavy";
        else if (powerAmmo)
            return "Power";
        return "";
    }

    public void ResetBulletToIgnoreRecoil()
    {
        bulletsToIgnoreRecoil = defaultBulletsToIgnoreRecoil;
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
        return numberOfBulletsToShoot;
    }
}

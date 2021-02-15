using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum weaponType { Assault_Rifle, DMR, Pistol, SMG, Shotgun, Sniper };

public class WeaponProperties : MonoBehaviour
{
    [Header("Weapon Info")]
    public string weaponName;
    public bool hasReticule;
    public string reticule;
    public weaponType weaponType;
    public int storedWeaponNumber;
    public int damage = 50;
    public int bulletSpeed = 250;
    public float range;
    public bool canSelectFire;
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

    [Header("Bullet Behavior")]
    public bool isNormalBullet;
    public bool isHeadshotCapable;
    public bool canBleedthroughHeadshot;
    public bool canBleedthroughAnything;

    [Header("Recoil")]
    public bool hasRecoil;
    public float recoilAmount;
    public CameraScript camScript;

    [Header("Sounds")]
    public AudioClip draw;
    public AudioClip Fire;
    public AudioClip Reload_1;
    public AudioClip Reload_2;
    public AudioClip holster;

    [Header("Fully Automatic Setings")]
    public bool isFullyAutomatic; public float timeBetweenFABullets = .01f;

    [Header("Burst Mode Settings")]
    public bool isBurstWeapon; public float timeBetweenBurstBullets = .01f, timeBetweenBurstCompletion = .01f;

    [Header("Single Fire Settings")]
    public bool isSingleFire; public float timeBetweenSingleBullets = .01f;

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
        Debug.Log($"ENUM DEBUG TEST: {weaponType}");

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
        if (camScript != null)
        {
            if (hasRecoil)
            {
                if(!pController.movement.isGrounded || !pController.isCrouching)
                    camScript.xRotation -= recoilAmount;
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
}

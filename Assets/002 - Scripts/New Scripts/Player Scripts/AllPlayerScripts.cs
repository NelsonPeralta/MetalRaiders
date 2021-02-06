using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllPlayerScripts : MonoBehaviour
{
    [Header("General")]
    public PlayerProperties playerProperties;
    [Header("Inventory")]
    public PlayerInventory playerInventory;
    public WeaponPickUp weaponPickUp;
    public GameObjectPool playerBulletPool;    
    [Header("UI Components")]
    public CrosshairScript crosshairScript;
    public RaycastScript raycastScript;
    public PlayerUIComponents playerUIComponents;
    public Aiming aimingScript;
    [Header("Sounds")]
    public PlayerSFXs playerSFXs;
    public Announcer announcer;
    [Header("Multiplayer Scripts")]
    public PlayerMPProperties playerMPProperties;
    [Header("Pools")]
    public GameObjectPool playerGenericHitPool;
    public GameObjectPool playerFirstMuzzleFlash;
    public GameObjectPool playerThirdMuzzleFlash;
}

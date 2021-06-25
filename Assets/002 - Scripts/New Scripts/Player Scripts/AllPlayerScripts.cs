using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllPlayerScripts : MonoBehaviour
{
    [Header("General")]
    public PlayerProperties playerProperties;
    public PlayerController playerController;
    [Header("Inventory")]
    public PlayerInventory playerInventory;
    public WeaponPickUp weaponPickUp;
    [Header("UI Components")]
    public CrosshairScript crosshairScript;
    public RaycastScript raycastScript;
    public PlayerUIComponents playerUIComponents;
    public Aiming aimingScript;
    public WaveCounter waveCounter;
    [Header("Sounds")]
    public PlayerSFXs playerSFXs;
    public Announcer announcer;
    [Header("Multiplayer Scripts")]
    public PlayerMPProperties playerMPProperties;
    [Header("Pools")]
    public GameObjectPool playerFirstMuzzleFlash;
    public GameObjectPool playerThirdMuzzleFlash;
}

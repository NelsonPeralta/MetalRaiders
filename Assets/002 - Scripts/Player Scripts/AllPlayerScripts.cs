using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllPlayerScripts : MonoBehaviour
{
    [Header("General")]
    public Player playerProperties;
    public PlayerController playerController;
    [Header("Inventory")]
    public PlayerInventory playerInventory;
    public PlayerWeaponSwapping weaponPickUp;
    [Header("UI Components")]
    public PlayerCamera cameraScript;
    public CrosshairManager crosshairScript;
    public AimAssist aimAssist;
    public Aiming aimingScript;
    public WaveCounter waveCounter;
    public ScoreboardManager scoreboardManager;
    public KillFeedManager killFeedManager;
    public DamageIndicatorManager damageIndicatorManager;
    [Header("Sounds")]
    public PlayerSFXs playerSFXs;
    public Announcer announcer;
    [Header("Multiplayer Scripts")]
    public PlayerMultiplayerMatchStats playerMultiplayerStats;
    [Header("Pools")]
    public GameObjectPool playerFirstMuzzleFlash;
    public GameObjectPool playerThirdMuzzleFlash;
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public class Bullet : MonoBehaviourPunCallbacks
{
    public PhotonView PV;
    [Header("Other Scripts")]
    // BulletProperties bProperties;
    public AllPlayerScripts allPlayerScripts;
    public MyPlayerManager pManager;
    public PlayerProperties playerWhoShot;
    public PlayerInventory pInventory;
    public WeaponProperties wProperties;
    public ZombieScript zScript;
    public AimAssist raycastScript;
    public CrosshairScript crosshairScript;
    public GameObjectPool gameObjectPool;

    public GameObject collision;
    public GameObject bulletTarget;

    public RaycastHit[] hits;
    //public LayerMask layerMask;

    [Header("Bullet Info")]
    public int damage;
    public float bulletSpeed;
    public float range;
    public float distanceTravelled;
    public int playerRewiredID;
    public bool redTeam = false;
    public bool blueTeam = false;
    public bool yellowTeam = false;
    public bool greenTeam = false;
    float defaultTimeToDespawn = .5f, timeToDespawn;


    [Header("Bullet Behavior")]
    public bool isNormalBullet;
    public bool isHeadshotCapable;
    public bool canBleedthroughHeadshot;
    public bool canBleedthroughAnything;

    public bool useUpdateVoid;
    public bool damageDealt;

    Vector3 playerPosWhenBulletShot;
    Vector3 prePos;
    Vector3 originalPos;

    [Header("Impact Effects")]
    public GameObject genericHit;
    public GameObject organicBlood;
    public GameObject magicBlood;
    public GameObject shieldHit;

    int frameCounter;
    List<ObjectHit> objectsHit = new List<ObjectHit>();
    void Awake()
    {
        gameObjectPool = GameObjectPool.gameObjectPoolInstance;
    }

    override public void OnEnable()
    {
        objectsHit.Clear();
        if (playerWhoShot)
            playerPosWhenBulletShot = playerWhoShot.transform.position;
        frameCounter = 0;
        distanceTravelled = 0;
        damageDealt = false;
        GetBulletInfo();

        if (crosshairScript && wProperties)
            if (!crosshairScript.RRisActive)
            {
                //if (wProperties.weaponType == "Shotgun")
                //{
                //    //gameObject.SetActive(false);
                //}

                useUpdateVoid = true;
            }
            else
            {
                useUpdateVoid = false;
            }

        originalPos = transform.position;
        timeToDespawn = CalculateTimeToDespawn();
        //Debug.Log($"BULLET CALCULATE DESPAWN TIME: {CalculateTimeToDespawn()}");
    }

    float CalculateTimeToDespawn()
    {
        return (range / bulletSpeed);
    }

    void Despawn()
    {
        if (timeToDespawn > 0)
            timeToDespawn -= Time.deltaTime;
        else
            gameObject.SetActive(false);
    }

    private void Update()
    {
        Travel();
        Despawn();
    }

    void Travel()
    {
        string hitMessage = "";
        prePos = transform.position; // Previous Position
        transform.Translate(Vector3.forward * Time.deltaTime * bulletSpeed); // Moves the bullet at 'bulletSpeed' units per second
        hits = Physics.RaycastAll(new Ray(prePos, (transform.position - prePos).normalized), (transform.position - prePos).magnitude);//, layerMask);
        frameCounter++;
        hitMessage += $"FRAME: {frameCounter}.";
        for (int i = 0; i < hits.Length; i++) // wWith a normal for loop, if the player is too close to a wall, it checks what object it collided with from farthest to closest. (int i = 0; i < hits.Length; i++)
        {
            //hitMessage += $"\nHIT INDEX: {i}. Hit NAME: {hits[i].collider.name} HIT DISTANCE FROM PLAYER: {Vector3.Distance(playerWhoShot.transform.position, hits[i].point)}";
            Debug.Log(hitMessage);
            if (!damageDealt && hits[i].collider.gameObject.layer != 22)
            {
                //GameObject hit = hits[i].collider.gameObject;
                //if (hit.GetComponent<AIHitbox>() && !hit.GetComponent<AIHitbox>().aiAbstractClass.isDead())
                //{
                //    //hitMessage = "Hit AI";
                //    AIHitbox hitbox = hits[i].collider.gameObject.GetComponent<AIHitbox>();
                //    int _damage = damage;
                //    if (hitbox.isHead && wProperties.isHeadshotCapable)
                //    {
                //        playerWhoShot.allPlayerScripts.playerUIComponents.ShowHeadshotIndicator();
                //        _damage = (int)(wProperties.headshotMultiplier * damage);
                //    }

                //    if (playerWhoShot.PV.IsMine)
                //        hitbox.aiAbstractClass.Damage(_damage, playerWhoShot.PV.ViewID);

                //    GameObject bloodHit = gameObjectPool.SpawnPooledBloodHit();
                //    bloodHit.transform.position = hits[i].point;
                //    bloodHit.SetActive(true);

                //    damageDealt = true;
                //}
                //else if (hit.GetComponent<PlayerHitbox>() && !hit.GetComponent<PlayerHitbox>().player.isDead && !hit.GetComponent<PlayerHitbox>().player.isRespawning)
                //{
                //    //hitMessage = "Hit Player at: + " + hit.name + damageDealt;

                //    PlayerHitbox hitbox = hits[i].collider.gameObject.GetComponent<PlayerHitbox>();
                //    PlayerProperties playerProperties = hitbox.player.GetComponent<PlayerProperties>();
                //    bool wasHeadshot = false;
                //    if (hitbox.isHead && wProperties.isHeadshotCapable)
                //    {
                //        damage = (int)(damage * wProperties.headshotMultiplier);
                //        wasHeadshot = true;
                //        playerWhoShot.allPlayerScripts.playerUIComponents.ShowHeadshotIndicator();
                //    }

                //    if (playerWhoShot.PV.IsMine)
                //        playerProperties.Damage(damage, wasHeadshot, playerWhoShot.GetComponent<PhotonView>().ViewID);

                //    GameObject bloodHit = gameObjectPool.SpawnPooledBloodHit();
                //    bloodHit.transform.position = hits[i].point;
                //    bloodHit.SetActive(true);

                //    damageDealt = true;
                //}
                //else if (!hit.GetComponent<PlayerHitbox>() && !hit.GetComponent<CapsuleCollider>() && !hit.GetComponent<AIHitbox>() && !hit.GetComponent<CharacterController>())
                //{
                //    hitMessage += $"\n\n---HIT---: {hit.name}";
                //    GameObject genericHit = allPlayerScripts.playerController.objectPool.SpawnPooledGenericHit();
                //    int iplus = Mathf.Clamp(i + 1, 0, hits.Length - 1); // Both inclusive
                //    genericHit.transform.position = hits[iplus].point;
                //    genericHit.SetActive(true);

                //    damageDealt = true;
                //}







                GameObject hit = hits[i].collider.gameObject;
                if (hit.GetComponent<AIHitbox>() && !hit.GetComponent<AIHitbox>().aiAbstractClass.isDead())
                {
                    ObjectHit newHit = new ObjectHit(hit, hits[i].point, Vector3.Distance(playerPosWhenBulletShot, hits[i].point));
                    objectsHit.Add(newHit);

                }
                else if (hit.GetComponent<PlayerHitbox>() && !hit.GetComponent<PlayerHitbox>().player.isDead && !hit.GetComponent<PlayerHitbox>().player.isRespawning)
                {
                    ObjectHit newHit = new ObjectHit(hit, hits[i].point, Vector3.Distance(playerPosWhenBulletShot, hits[i].point));
                    objectsHit.Add(newHit);
                }
                else if (!hit.GetComponent<PlayerHitbox>() && !hit.GetComponent<CapsuleCollider>() && !hit.GetComponent<AIHitbox>() && !hit.GetComponent<CharacterController>())
                {
                    ObjectHit newHit = new ObjectHit(hit, hits[i].point, Vector3.Distance(playerPosWhenBulletShot, hits[i].point));
                    Debug.Log(newHit.hitPoint);
                    objectsHit.Add(newHit);
                }
            }

        }
        CheckForFinalHit();
    }

    void CheckForFinalHit()
    {
        Debug.Log($"Hits: {objectsHit.Count}");
        if (objectsHit.Count > 0)
        {
            GameObject finalHitObject = objectsHit[0].gameObject;
            Vector3 finalHitPoint = objectsHit[0].hitPoint;
            float finalHitDistance = objectsHit[0].distanceFromPlayer;
            for (int i = 0; i < objectsHit.Count; i++)
            {
                Debug.Log($"Object Hit: {objectsHit[i].gameObject.name}. Distance: {objectsHit[i].distanceFromPlayer}. Hir point: {objectsHit[i].hitPoint}");
                if (objectsHit[i].distanceFromPlayer < finalHitDistance)
                {
                    Debug.Log(objectsHit[i].hitPoint);
                    finalHitDistance = objectsHit[i].distanceFromPlayer;
                    finalHitPoint = objectsHit[i].hitPoint;
                    finalHitObject = objectsHit[i].gameObject;
                }
            }

            Debug.Log($"Final Hit: {finalHitObject.name} with a distance of: {finalHitDistance} at point: {finalHitPoint}");
            if (finalHitObject.GetComponent<AIHitbox>() && !finalHitObject.GetComponent<AIHitbox>().aiAbstractClass.isDead())
            {
                AIHitbox hitbox = finalHitObject.GetComponent<AIHitbox>();
                int _damage = damage;
                if (hitbox.isHead && wProperties.isHeadshotCapable)
                {
                    playerWhoShot.allPlayerScripts.playerUIComponents.ShowHeadshotIndicator();
                    _damage = (int)(wProperties.headshotMultiplier * damage);
                }

                if (playerWhoShot.PV.IsMine)
                    hitbox.aiAbstractClass.Damage(_damage, playerWhoShot.PV.ViewID);

                GameObject bloodHit = gameObjectPool.SpawnPooledBloodHit();
                bloodHit.transform.position = finalHitPoint;
                bloodHit.SetActive(true);

                damageDealt = true;
            }
            else if (finalHitObject.GetComponent<PlayerHitbox>() && !finalHitObject.GetComponent<PlayerHitbox>().player.isDead && !finalHitObject.GetComponent<PlayerHitbox>().player.isRespawning)
            {
                //hitMessage = "Hit Player at: + " + hit.name + damageDealt;
                Debug.Log("HEEERSERE");

                PlayerHitbox hitbox = finalHitObject.GetComponent<PlayerHitbox>();
                PlayerProperties playerProperties = hitbox.player.GetComponent<PlayerProperties>();
                bool wasHeadshot = false;
                if (hitbox.isHead && wProperties.isHeadshotCapable)
                {
                    damage = (int)(damage * wProperties.headshotMultiplier);
                    wasHeadshot = true;
                    playerWhoShot.allPlayerScripts.playerUIComponents.ShowHeadshotIndicator();
                }

                if (playerWhoShot.PV.IsMine)
                {
                    Debug.Log("asdasdasdas");
                    playerProperties.Damage(damage, wasHeadshot, playerWhoShot.GetComponent<PhotonView>().ViewID);
                }

                GameObject bloodHit = gameObjectPool.SpawnPooledBloodHit();
                bloodHit.transform.position = finalHitPoint;
                bloodHit.SetActive(true);

                damageDealt = true;
            }
            else if (!finalHitObject.GetComponent<PlayerHitbox>() && !finalHitObject.GetComponent<CapsuleCollider>() && !finalHitObject.GetComponent<AIHitbox>() && !finalHitObject.GetComponent<CharacterController>())
            {
                //hitMessage += $"\n\n---HIT---: {hit.name}";
                GameObject genericHit = allPlayerScripts.playerController.objectPool.SpawnPooledGenericHit();
                //int iplus = Mathf.Clamp(i + 1, 0, hits.Length - 1); // Both inclusive
                genericHit.transform.position = finalHitPoint;
                genericHit.SetActive(true);

                damageDealt = true;
            }
            gameObject.SetActive(false);
        }

    }
    public class ObjectHit
    {
        public GameObject gameObject;
        public Vector3 hitPoint;
        public float distanceFromPlayer;
        public ObjectHit(GameObject _gameobject, Vector3 _hitPoint, float dist)
        {
            gameObject = _gameobject;
            hitPoint = _hitPoint;
            distanceFromPlayer = dist;
        }
    }

    [PunRPC]
    void DisableThisBullet()
    {
        gameObject.SetActive(false);
    }

    [PunRPC]
    void DamagePlayer(PlayerHitbox pHitbox)
    {

        //Debug.Log(pHitbox.gameObject.layer);
        //PlayerProperties hitPlayerProperties = pHitbox.player.GetComponent<PlayerProperties>();

        //if (!damageDealt)
        //{
        //    if (isNormalBullet) /////////////////////////////////////////////////////////////////////////////////////Normal Bullet
        //    {
        //        if (hitPlayerProperties.hasShield) //If Player has Shields
        //        {
        //            if (hitPlayerProperties.Shield > 0)
        //            {
        //                hitPlayerProperties.SetShield(damage);
        //            }
        //            else
        //            {
        //                hitPlayerProperties.SetHealth(damage, false, playerRewiredID);
        //            }
        //        }
        //        else // If Player does not have Armor
        //        {
        //            hitPlayerProperties.SetHealth(damage, false, playerRewiredID);
        //        }
        //    }
        //    if (isHeadshotCapable) //////////////////////////////////////////////////////////////////////////////// Is Headshot Capable
        //    {
        //        if (hitPlayerProperties.hasShield)
        //        {
        //            if (hitPlayerProperties.Shield > 0)
        //            {
        //                hitPlayerProperties.SetShield(damage);
        //            }
        //            else if (hitPlayerProperties.Shield <= 0 && pHitbox.isHead)
        //            {
        //                hitPlayerProperties.SetHealth(damage, true, playerRewiredID);
        //            }
        //            else if (hitPlayerProperties.Shield <= 0 && !pHitbox.isHead)
        //            {
        //                hitPlayerProperties.SetHealth(damage, false, playerRewiredID);
        //            }
        //        }
        //        else // If Player does not have Armor
        //        {
        //            hitPlayerProperties.SetHealth(damage, false, playerRewiredID);
        //        }
        //    }
        //    if (canBleedthroughHeadshot) /////////////////////////////////////////////////////////////////////////// Can Bleedthrough Headshot
        //    {
        //        if (hitPlayerProperties.hasShield)
        //        {
        //            if (hitPlayerProperties.Shield > 0 && !pHitbox.isHead)
        //            {
        //                hitPlayerProperties.SetShield(damage);
        //            }
        //            else if (hitPlayerProperties.Shield > 0 && pHitbox.isHead)
        //            {
        //                hitPlayerProperties.BleedthroughDamage(damage, true, playerRewiredID);
        //            }
        //            else if (hitPlayerProperties.Shield <= 0 && !pHitbox.isHead)
        //            {
        //                hitPlayerProperties.SetHealth(damage, false, playerRewiredID);
        //            }
        //            else if (hitPlayerProperties.Shield <= 0 && pHitbox.isHead)
        //            {
        //                hitPlayerProperties.SetHealth(damage, true, playerRewiredID);
        //            }
        //        }
        //        else if (!hitPlayerProperties.hasShield) // If Player does not have Armor
        //        {
        //            if (pHitbox.isHead)
        //            {
        //                hitPlayerProperties.SetHealth(damage, true, playerRewiredID);
        //            }
        //            else
        //            {
        //                hitPlayerProperties.SetHealth(damage, false, playerRewiredID);
        //            }
        //        }
        //    }
        //    if (canBleedthroughAnything) /////////////////////////////////////////////////////////////////////////////// Can Bleedthrough Anything
        //    {

        //    }

        //    damageDealt = true; ;
        //    gameObject.SetActive(false);
        //}

    }

    [PunRPC]
    void SpawnGenericHit(Vector3 point)
    {
        //if (!PV.IsMine)
        //    return;
        //Debug.Log("Spawned Generic Hit from Bullet Script. Damage dealt: " + damageDealt);
        //GameObject genericHit = gameObjectPool.SpawnPooledGenericHit();
        //genericHit.transform.position = point;
        //genericHit.SetActive(true);
        //gameObject.SetActive(false);
    }

    void AIDamage(AIHitbox aiHB, RaycastHit hit)
    {
        //Debug.Log($"Damage dealt: {damageDealt}");
        //Debug.Log(aiHB.gameObject.name);

        if (!damageDealt)
        {
            //Debug.Log("Hit 0 " + damageDealt);

            if (isNormalBullet) /////////////////////////////////////////////////////////////////////////////////////Normal Bullet
            {
                aiHB.DamageAI(true, damage, playerWhoShot.gameObject);
                //Debug.Log("Hit 1");
            }
            if (isHeadshotCapable) //////////////////////////////////////////////////////////////////////////////// Is Headshot Capable
            {
                if (aiHB.isHead)
                {
                    aiHB.DamageAI(true, damage * 1.5f, playerWhoShot.gameObject);
                    playerWhoShot.allPlayerScripts.playerUIComponents.ShowHeadshotIndicator();
                    //Debug.Log("Hit 2");
                }
                else
                {
                    aiHB.DamageAI(true, damage, playerWhoShot.gameObject);
                    //Debug.Log("Hit 3");
                }
            }
            if (canBleedthroughHeadshot) /////////////////////////////////////////////////////////////////////////// Can Bleedthrough Headshot
            {
                if (aiHB.isHead)
                {
                    aiHB.DamageAI(true, damage * 1.5f, playerWhoShot.gameObject);
                    //Debug.Log("Hit 4");
                }
                else
                {
                    aiHB.DamageAI(true, damage, playerWhoShot.gameObject);
                    //Debug.Log("Hit 5");
                }
            }
            if (canBleedthroughAnything) /////////////////////////////////////////////////////////////////////////////// Can Bleedthrough Anything
            {
                aiHB.DamageAI(true, damage, playerWhoShot.gameObject);
                //Debug.Log("Hit 6");
            }

            GameObject magicBloodEffect = Instantiate(magicBlood);
            magicBloodEffect.transform.position = hit.point;
            Destroy(magicBloodEffect, 1);
            damageDealt = true;
            gameObject.SetActive(false);
        }
    }


    void GetBulletInfo()
    {
        if (playerWhoShot)
            if (!playerWhoShot.GetComponent<PlayerController>().isDualWielding)
                wProperties = pInventory.activeWeapon.gameObject.GetComponent<WeaponProperties>();
        if (wProperties)
        {
            damage = wProperties.damage;
            bulletSpeed = wProperties.bulletSpeed;
            //Debug.Log(wProperties.gameObject.name);
            //Debug.Log(wProperties.damage);
            //Debug.Log(wProperties.bulletSpeed);

            if (wProperties.isNormalBullet)
            {
                isNormalBullet = true;
                isHeadshotCapable = false;
                canBleedthroughHeadshot = false;
                canBleedthroughAnything = false;
            }
            else if (wProperties.isHeadshotCapable)
            {
                isNormalBullet = false;
                isHeadshotCapable = true;
                canBleedthroughHeadshot = false;
                canBleedthroughAnything = false;
            }
            else if (wProperties.canBleedthroughHeadshot)
            {
                isNormalBullet = false;
                isHeadshotCapable = false;
                canBleedthroughHeadshot = true;
                canBleedthroughAnything = false;
            }
            else if (wProperties.canBleedthroughAnything)
            {
                isNormalBullet = false;
                isHeadshotCapable = false;
                canBleedthroughHeadshot = false;
                canBleedthroughAnything = true;
            }
        }
    }
}

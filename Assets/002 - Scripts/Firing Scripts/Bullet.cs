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
    public Player playerWhoShot;
    public PlayerInventory pInventory;
    public WeaponProperties weaponProperties;
    public Zombie zScript;
    public CrosshairManager crosshairScript;
    public GameObjectPool gameObjectPool;

    public GameObject collision;
    public GameObject bulletTarget;

    public RaycastHit[] hits;
    //public LayerMask layerMask;

    [Header("Bullet Info")]
    public int damage;
    public int size;
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

        if (crosshairScript && weaponProperties)
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

    List<int> bulletLayers = new List<int> { 0, 7, 12, 14 };
    void Travel()
    {
        frameCounter++;
        prePos = transform.position; // Previous Position
        transform.Translate(Vector3.forward * Time.deltaTime * bulletSpeed); // Moves the bullet at 'bulletSpeed' units per second

        //Collider[] colliders = Physics.OverlapSphere(transform.position, size);
        //List<GameObject> objectsHit = new List<GameObject>();
        //for (int i = 0; i < colliders.Length; i++) // foreach(Collider hit in colliders)
        //{
        //    Debug.Log($"Bullet OverlapSphere: {colliders[i].name}. Frame: {frameCounter}. Distance from bullet: {Vector3.Distance(colliders[i].transform.position, transform.position)}");
        //}

        hits = Physics.RaycastAll(new Ray(prePos, (transform.position - prePos).normalized), (transform.position - prePos).magnitude);//, layerMask);
        for (int i = 0; i < hits.Length; i++) // wWith a normal for loop, if the player is too close to a wall, it checks what object it collided with from farthest to closest. (int i = 0; i < hits.Length; i++)
        {
            //hitMessage += $"\nHIT INDEX: {i}. Hit NAME: {hits[i].collider.name} HIT DISTANCE FROM PLAYER: {Vector3.Distance(playerWhoShot.transform.position, hits[i].point)}";
            if (!damageDealt && bulletLayers.Contains(hits[i].collider.gameObject.layer))
            {
                GameObject hit = hits[i].collider.gameObject;

                if (hits[i].collider.GetComponent<IDamageable>() != null || hits[i].collider)
                {
                    ObjectHit newHit = new ObjectHit(hit, hits[i].point, Vector3.Distance(playerPosWhenBulletShot, hits[i].point));
                    objectsHit.Add(newHit);
                }

                // Old
                #region
                //if (hit.GetComponent<AIHitbox>() && !hit.GetComponent<AIHitbox>().aiAbstractClass.isDead)
                //{
                //    ObjectHit newHit = new ObjectHit(hit, hits[i].point, Vector3.Distance(playerPosWhenBulletShot, hits[i].point));
                //    objectsHit.Add(newHit);

                //}
                //else if (hit.GetComponent<PlayerHitbox>() && !hit.GetComponent<PlayerHitbox>().player.isDead && !hit.GetComponent<PlayerHitbox>().player.isRespawning)
                //{
                //    ObjectHit newHit = new ObjectHit(hit, hits[i].point, Vector3.Distance(playerPosWhenBulletShot, hits[i].point));
                //    objectsHit.Add(newHit);
                //}
                //else if (!hit.GetComponent<PlayerHitbox>() && !hit.GetComponent<CapsuleCollider>() && !hit.GetComponent<AIHitbox>() && !hit.GetComponent<CharacterController>())
                //{
                //    ObjectHit newHit = new ObjectHit(hit, hits[i].point, Vector3.Distance(playerPosWhenBulletShot, hits[i].point));
                //    objectsHit.Add(newHit);
                //}
                #endregion
            }

        }
        CheckForFinalHit();
    }

    void CheckForFinalHit()
    {
        if (objectsHit.Count > 0)
        {
            GameObject finalHitObject = objectsHit[0].gameObject;
            IDamageable finalHitDamageable = objectsHit[0].gameObject.GetComponent<IDamageable>();
            Vector3 finalHitPoint = objectsHit[0].hitPoint;
            float finalHitDistance = objectsHit[0].distanceFromPlayer;
            for (int i = 0; i < objectsHit.Count; i++)
            {
                if (objectsHit[i].distanceFromPlayer < finalHitDistance)
                {
                    Debug.Log(objectsHit[i].hitPoint);
                    finalHitDistance = objectsHit[i].distanceFromPlayer;
                    finalHitDamageable = objectsHit[i].gameObject.GetComponent<IDamageable>();
                    finalHitPoint = objectsHit[i].hitPoint;
                    finalHitObject = objectsHit[i].gameObject;
                }
            }

            try
            {
                if (finalHitObject.GetComponent<PlayerHitbox>() && !finalHitObject.GetComponent<PlayerHitbox>().player.isDead && !finalHitObject.GetComponent<PlayerHitbox>().player.isRespawning)
                {
                    PlayerHitbox hitbox = finalHitObject.GetComponent<PlayerHitbox>();
                    Player player = hitbox.player.GetComponent<Player>();
                    bool wasHeadshot = false;
                    bool wasNutshot = false;
                    if (weaponProperties.isHeadshotCapable && (hitbox.isHead || hitbox.isNuts))
                    {
                        int maxShieldPoints = player.maxHitPoints - player.maxHealthPoints;

                        if (maxShieldPoints > 0 && (player.hitPoints <= player.maxHealthPoints))
                            damage = player.maxHealthPoints;

                        if (weaponProperties.reticuleType == WeaponProperties.ReticuleType.Sniper)
                            damage = (int)(damage * weaponProperties.headshotMultiplier);

                        wasHeadshot = hitbox.isHead;
                        wasNutshot = hitbox.isNuts;

                        if (wasHeadshot && player.hitPoints < damage)
                            playerWhoShot.GetComponent<PlayerMultiplayerMatchStats>().headshots++;

                    }

                    if (playerWhoShot.PV.IsMine)
                    {
                        if (weaponProperties.codeName != null)
                            finalHitDamageable.Damage(damage, wasHeadshot, playerWhoShot.GetComponent<PhotonView>().ViewID, finalHitPoint, weaponProperties.codeName, isGroin: wasNutshot);
                        else
                            finalHitDamageable.Damage(damage, wasHeadshot, playerWhoShot.GetComponent<PhotonView>().ViewID, finalHitPoint, isGroin: wasNutshot);
                    }

                    damageDealt = true;
                }
                else if (!finalHitObject.GetComponent<PlayerHitbox>() && !finalHitObject.GetComponent<CapsuleCollider>() && !finalHitObject.GetComponent<AIHitbox>() && !finalHitObject.GetComponent<CharacterController>())
                {
                    try
                    {
                        finalHitDamageable.Damage(damage);
                    }
                    catch { }
                    GameObject genericHit = FindObjectOfType<GameObjectPool>().SpawnPooledGenericHit();
                    genericHit.transform.position = finalHitPoint;
                    genericHit.SetActive(true);

                    damageDealt = true;
                }
                gameObject.SetActive(false);
            }
            catch { }

            // Old
            #region
            //if (finalHitObject.GetComponent<AIHitbox>() && !finalHitObject.GetComponent<AIHitbox>().aiAbstractClass.isDead)
            //{
            //    AIHitbox hitbox = finalHitObject.GetComponent<AIHitbox>();
            //    int finalDamage = damage;
            //    bool isHeadshot = hitbox.isHead && weaponProperties.isHeadshotCapable;
            //    if (isHeadshot)
            //    {
            //        //Debug.Log($"Bullet final hit: {finalHitObject.name} HEADSHOT");
            //        finalDamage = (int)(weaponProperties.headshotMultiplier * damage);

            //        if (hitbox.aiAbstractClass.health <= finalDamage)
            //        {
            //            playerWhoShot.GetComponent<PlayerSwarmMatchStats>().headshots++;
            //        }
            //    }



            //    if (playerWhoShot.PV.IsMine)
            //    {
            //        //Debug.Log($"AI is dead: {hitbox.aiAbstractClass.isDead}");
            //        hitbox.aiAbstractClass.Damage(finalDamage, playerWhoShot.PV.ViewID, isHeadshot: isHeadshot, damageSource: weaponProperties.codeName);
            //    }

            //    GameObject bloodHit = gameObjectPool.SpawnPooledBloodHit();
            //    bloodHit.transform.position = finalHitPoint;
            //    bloodHit.SetActive(true);

            //    damageDealt = true;
            //}
            //else if (finalHitObject.GetComponent<PlayerHitbox>() && !finalHitObject.GetComponent<PlayerHitbox>().player.isDead && !finalHitObject.GetComponent<PlayerHitbox>().player.isRespawning)
            //{
            //    //hitMessage = "Hit Player at: + " + hit.name + damageDealt;

            //    PlayerHitbox hitbox = finalHitObject.GetComponent<PlayerHitbox>();
            //    Player player = hitbox.player.GetComponent<Player>();
            //    bool wasHeadshot = false;
            //    bool wasNutshot = false;
            //    if (weaponProperties.isHeadshotCapable && (hitbox.isHead || hitbox.isNuts))
            //    {
            //        int maxShieldPoints = player.maxHitPoints - player.maxHealthPoints;

            //        if (maxShieldPoints > 0 && (player.hitPoints <= player.maxHealthPoints))
            //            damage = player.maxHealthPoints;

            //        if (weaponProperties.reticuleType == WeaponProperties.ReticuleType.Sniper)
            //            damage = (int)(damage * weaponProperties.headshotMultiplier);

            //        wasHeadshot = hitbox.isHead;
            //        wasNutshot = hitbox.isNuts;

            //        if (wasHeadshot && player.hitPoints < damage)
            //            playerWhoShot.GetComponent<PlayerMultiplayerMatchStats>().headshots++;

            //    }

            //    if (playerWhoShot.PV.IsMine)
            //    {
            //        if (weaponProperties.codeName != null)
            //        {
            //            player.Damage(damage, wasHeadshot, playerWhoShot.GetComponent<PhotonView>().ViewID, finalHitPoint, weaponProperties.codeName, isGroin: wasNutshot);
            //        }
            //        else
            //            player.Damage(damage, wasHeadshot, playerWhoShot.GetComponent<PhotonView>().ViewID, finalHitPoint, isGroin: wasNutshot);
            //    }

            //    damageDealt = true;
            //}
            //else if (!finalHitObject.GetComponent<PlayerHitbox>() && !finalHitObject.GetComponent<CapsuleCollider>() && !finalHitObject.GetComponent<AIHitbox>() && !finalHitObject.GetComponent<CharacterController>())
            //{
            //    //hitMessage += $"\n\n---HIT---: {hit.name}";
            //    GameObject genericHit = FindObjectOfType<GameObjectPool>().SpawnPooledGenericHit();
            //    //int iplus = Mathf.Clamp(i + 1, 0, hits.Length - 1); // Both inclusive
            //    genericHit.transform.position = finalHitPoint;
            //    genericHit.SetActive(true);

            //    damageDealt = true;
            //}
            #endregion
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

    class BulletData
    {
        public int sourcePID;

        public bool wasHeadshot;
        public bool wasGroin;

        public Vector3 finalHitPos;
        public string sourceWeaponCodename;
    }
    void GetBulletInfo()
    {
        if (playerWhoShot)
            if (!playerWhoShot.GetComponent<PlayerController>().isDualWielding)
                weaponProperties = pInventory.activeWeapon.gameObject.GetComponent<WeaponProperties>();
        if (weaponProperties)
        {
            damage = weaponProperties.damage;
            size = weaponProperties.bulletSize;
            bulletSpeed = weaponProperties.bulletSpeed;
            //Debug.Log(wProperties.gameObject.name);
            //Debug.Log(wProperties.damage);
            //Debug.Log(wProperties.bulletSpeed);

            isNormalBullet = true;
            isHeadshotCapable = false;
            canBleedthroughHeadshot = false;
            canBleedthroughAnything = false;
            if (weaponProperties.isHeadshotCapable)
            {
                isNormalBullet = false;
                isHeadshotCapable = true;
                canBleedthroughHeadshot = false;
                canBleedthroughAnything = false;
            }
        }
    }
}

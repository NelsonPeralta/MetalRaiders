using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public class Bullet : MonoBehaviourPunCallbacks
{
    public Player sourcePlayer { get { return _sourcePlayer; } set { _sourcePlayer = value; _spawnDir = sourcePlayer.transform.position; } }
    public Vector3 spawnDir
    {
        get { return _spawnDir; }
    }

    Player _sourcePlayer;
    Vector3 _spawnDir;
    Vector3 _nextPos;

    public PhotonView PV;
    [Header("Other Scripts")]
    // BulletProperties bProperties;
    public AllPlayerScripts allPlayerScripts;
    public PlayerInventory pInventory;
    public WeaponProperties weaponProperties;
    public Zombie zScript;
    public CrosshairManager crosshairScript;
    public GameObjectPool gameObjectPool;

    public GameObject collision;
    public GameObject bulletTarget;

    public RaycastHit[] hits;
    [SerializeField] LayerMask _layerMask;

    [Header("Bullet Info")]
    public int damage;
    public int size;
    public float speed;
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
    public GameObject bluePlasma;

    int frameCounter;
    List<ObjectHit> objectsHit = new List<ObjectHit>();

    void Awake()
    {
        gameObjectPool = FindObjectOfType<GameObjectPool>();
    }

    override public void OnEnable()
    {
        prePos = transform.position;
        _nextPos = Vector3.zero;

        objectsHit.Clear();
        if (sourcePlayer)
            playerPosWhenBulletShot = sourcePlayer.transform.position;
        frameCounter = 0;
        distanceTravelled = 0;
        damageDealt = false;

        originalPos = transform.position;
        timeToDespawn = CalculateTimeToDespawn();
    }

    float CalculateTimeToDespawn()
    {
        return (range / speed);
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
        Despawn();
        ShootRay();
        Travel();
    }
    private void LateUpdate()
    {
        //Travel();
    }


    float _distanceTravalled;
    //List<int> bulletLayers = new List<int> { 0, 7, 12, 14 };
    void ShootRay()
    {
        prePos = transform.position;
        _nextPos = transform.position + transform.TransformDirection(Vector3.forward) * speed * Time.deltaTime;
        //transform.Translate(Vector3.forward * Time.deltaTime * bulletSpeed); // Moves the bullet at 'bulletSpeed' units per second

        float _dTravalled = Vector3.Distance(prePos, _nextPos);
        //if (_dTravalled > weaponProperties.range)
        //    gameObject.SetActive(false);

        RaycastHit[] m_Results = new RaycastHit[5];
        Ray r = new Ray(prePos, (_nextPos - prePos).normalized);
        int dLayer = 0;
        int hLayer = 7;

        int finalmask = (1 << dLayer) | (1 << hLayer);

        RaycastHit fhit;
        if (Physics.Raycast(r.origin, r.direction, out fhit, maxDistance: _dTravalled, finalmask))
        {
            Debug.Log($"HIT: {fhit.collider.gameObject.name}. LAYER: {fhit.collider.gameObject.layer}");


            GameObject hit = fhit.collider.gameObject;

            if (fhit.collider.GetComponent<IDamageable>() != null || fhit.collider)
            {
                float _distanceFromSpawnToHit = Vector3.Distance(originalPos, fhit.point);

                if (_distanceFromSpawnToHit <= weaponProperties.range)
                {
                    ObjectHit newHit = new ObjectHit(hit, fhit, fhit.point, Vector3.Distance(playerPosWhenBulletShot, fhit.point));
                    objectsHit.Add(newHit);
                }
            }

            CheckForFinalHit();

            gameObject.SetActive(false);

        }
        return;
        for (int i = 0; i < hits.Length; i++) // wWith a normal for loop, if the player is too close to a wall, it checks what object it collided with from farthest to closest. (int i = 0; i < hits.Length; i++)
        {
            //hitMessage += $"\nHIT INDEX: {i}. Hit NAME: {hits[i].collider.name} HIT DISTANCE FROM PLAYER: {Vector3.Distance(playerWhoShot.transform.position, hits[i].point)}";
            if (!damageDealt)
            {
                GameObject hit = hits[i].collider.gameObject;

                if (hits[i].collider.GetComponent<IDamageable>() != null || hits[i].collider)
                {
                    ObjectHit newHit = new ObjectHit(hit, fhit, hits[i].point, Vector3.Distance(playerPosWhenBulletShot, hits[i].point));
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

    void Travel()
    {
        transform.Translate(Vector3.forward * Time.deltaTime * speed);
        //prePos = transform.position;
    }

    void CheckForFinalHit()
    {
        if (objectsHit.Count > 0)
        {
            RaycastHit hitInfo = objectsHit[0].raycastHit;
            GameObject finalHitObject = objectsHit[0].gameObject;
            IDamageable finalHitDamageable = objectsHit[0].gameObject.GetComponent<IDamageable>();
            Vector3 finalHitPoint = objectsHit[0].hitPoint;
            float finalHitDistance = objectsHit[0].distanceFromPlayer;
            for (int i = 0; i < objectsHit.Count; i++)
            {
                if (objectsHit[i].distanceFromPlayer < finalHitDistance)
                {
                    finalHitDistance = objectsHit[i].distanceFromPlayer;
                    finalHitDamageable = objectsHit[i].gameObject.GetComponent<IDamageable>();
                    finalHitPoint = objectsHit[i].hitPoint;
                    finalHitObject = objectsHit[i].gameObject;
                    hitInfo = objectsHit[i].raycastHit;
                }
            }

            _spawnDir = finalHitPoint - _spawnDir;
            Debug.Log($"BULLET CheckForFinalHit {finalHitObject.name}");

            if (weaponProperties.degradingDamage && finalHitDistance >= weaponProperties.degradingDamageStart)
                damage = weaponProperties.degradedDamage;

            try
            {
                finalHitObject.GetComponent<PropHitbox>().hitPoints.Damage(damage, finalHitObject.GetComponent<PropHitbox>().isHead && weaponProperties.isHeadshotCapable, playerRewiredID, finalHitPoint, spawnDir, weaponProperties.cleanName, finalHitObject.GetComponent<PropHitbox>().isGroin && weaponProperties.isHeadshotCapable);
                damageDealt = true;
            }
            catch { }

            try
            {
                if (finalHitObject.GetComponent<ActorHitbox>())
                {
                    try
                    {
                        bool ih = false;
                        if (finalHitObject.GetComponent<ActorHitbox>().isHead)
                        {
                            ih = true;

                            if (weaponProperties.isHeadshotCapable)
                                if (finalHitObject.GetComponent<ActorHitbox>().actor.oneShotHeadshot)
                                {
                                    ih = true;
                                    damage = finalHitObject.GetComponent<ActorHitbox>().actor.hitPoints;
                                }
                                else
                                    damage = (int)(damage * weaponProperties.headshotMultiplier);
                        }
                        finalHitDamageable.Damage(damage, ih, sourcePlayer.pid);

                    }
                    catch
                    {
                        finalHitDamageable.Damage(damage);
                    }
                    genericHit = gameObjectPool.SpawnPooledBloodHit();
                    genericHit.transform.position = finalHitPoint;
                    genericHit.SetActive(true);

                    damageDealt = true;

                    return;
                }

                if (GameManager.instance.teamMode.ToString().Contains("Classic"))
                {

                    if (finalHitObject.GetComponent<PlayerHitbox>())
                    {
                        Debug.Log(sourcePlayer.team.ToString());
                        Debug.Log(finalHitObject.GetComponent<PlayerHitbox>().player.team.ToString());

                        if (finalHitObject.GetComponent<PlayerHitbox>().player.team != sourcePlayer.team)
                        {
                            if (finalHitObject.GetComponent<PlayerHitbox>() && !finalHitObject.GetComponent<PlayerHitbox>().player.isDead && !finalHitObject.GetComponent<PlayerHitbox>().player.isRespawning)
                            {
                                PlayerHitbox hitbox = finalHitObject.GetComponent<PlayerHitbox>();
                                Player player = hitbox.player.GetComponent<Player>();
                                bool wasHeadshot = false;
                                bool wasNutshot = false;
                                if (weaponProperties.isHeadshotCapable && (hitbox.isHead || hitbox.isGroin))
                                {
                                    int maxShieldPoints = player.maxHitPoints - player.maxHealthPoints;

                                    if (maxShieldPoints > 0 && (player.hitPoints <= player.maxHealthPoints))
                                        damage = player.maxHealthPoints;

                                    if (weaponProperties.weaponType == WeaponProperties.WeaponType.Sniper)
                                        damage = (int)(damage * weaponProperties.headshotMultiplier);

                                    wasHeadshot = hitbox.isHead;
                                    wasNutshot = hitbox.isGroin;

                                }

                                //if (sourcePlayer.PV.IsMine)
                                {
                                    if (weaponProperties.codeName != null)
                                        finalHitDamageable.Damage(damage, wasHeadshot, sourcePlayer.GetComponent<PhotonView>().ViewID, finalHitPoint, impactDir: spawnDir, damageSource: weaponProperties.cleanName, isGroin: wasNutshot);
                                    else
                                        finalHitDamageable.Damage(damage, wasHeadshot, sourcePlayer.GetComponent<PhotonView>().ViewID, finalHitPoint, isGroin: wasNutshot);
                                }

                                damageDealt = true;
                            }
                        }
                        else
                        {
                            GameObject genericHit = gameObjectPool.SpawnPooledGenericHit();
                            genericHit.transform.position = finalHitPoint;
                            genericHit.SetActive(true);

                            damageDealt = true;

                            GameObject imp = gameObjectPool.SpawnBulletMetalImpactObject();
                            imp.transform.position = finalHitPoint;
                            imp.transform.rotation = Quaternion.LookRotation(hitInfo.normal);
                            imp.SetActive(true);
                        }
                    }
                    else
                    {
                        try
                        {
                            Debug.Log("asdf1234");
                            if (!finalHitObject.GetComponent<PlayerHitbox>())
                                try
                                {
                                    if (finalHitObject.GetComponent<ActorHitbox>() && finalHitObject.GetComponent<ActorHitbox>().isHead)
                                        damage = (int)(damage * weaponProperties.headshotMultiplier);
                                    finalHitDamageable.Damage(damage, false, sourcePlayer.pid);

                                }
                                catch
                                {
                                    finalHitDamageable.Damage(damage);
                                }
                        }
                        catch { }

                        GameObject genericHit;
                        if (finalHitObject.GetComponent<ActorHitbox>())
                            genericHit = gameObjectPool.SpawnPooledBloodHit();
                        else
                        {
                            genericHit = gameObjectPool.SpawnPooledGenericHit();

                            GameObject imp = gameObjectPool.SpawnBulletMetalImpactObject();
                            imp.transform.position = finalHitPoint;
                            imp.transform.rotation = Quaternion.LookRotation(hitInfo.normal);
                            imp.SetActive(true);
                        }
                        genericHit.transform.position = finalHitPoint;
                        genericHit.SetActive(true);

                        damageDealt = true;
                    }

                }
                else
                {
                    if (finalHitObject.GetComponent<PlayerHitbox>() && !finalHitObject.GetComponent<PlayerHitbox>().player.isDead && !finalHitObject.GetComponent<PlayerHitbox>().player.isRespawning)
                    {
                        PlayerHitbox hitbox = finalHitObject.GetComponent<PlayerHitbox>();
                        Player player = hitbox.player.GetComponent<Player>();
                        bool wasHeadshot = false;
                        bool wasNutshot = false;

                        if (weaponProperties.isHeadshotCapable && (hitbox.isHead || hitbox.isGroin))
                        {
                            Debug.Log("asdf5");

                            int maxShieldPoints = player.maxHitPoints - player.maxHealthPoints;

                            if (maxShieldPoints > 0 && (player.hitPoints <= player.maxHealthPoints))
                                damage = player.maxHealthPoints;

                            if (weaponProperties.weaponType == WeaponProperties.WeaponType.Sniper)
                                damage = (int)(damage * weaponProperties.headshotMultiplier);

                            if (GameManager.instance.gameType == GameManager.GameType.Swat)
                                damage = 999;

                            wasHeadshot = hitbox.isHead;
                            wasNutshot = hitbox.isGroin;
                        }

                        //if (sourcePlayer.PV.IsMine)
                        {
                            Debug.Log("asdf6");

                            if (weaponProperties.codeName != null)
                                finalHitDamageable.Damage(damage, wasHeadshot, sourcePlayer.GetComponent<PhotonView>().ViewID, finalHitPoint, impactDir: spawnDir, weaponProperties.cleanName, isGroin: wasNutshot);
                            else
                                finalHitDamageable.Damage(damage, wasHeadshot, sourcePlayer.GetComponent<PhotonView>().ViewID, finalHitPoint, isGroin: wasNutshot);
                        }

                        damageDealt = true;
                    }
                    else if (!finalHitObject.GetComponent<PlayerHitbox>() && !finalHitObject.GetComponent<CapsuleCollider>() && !finalHitObject.GetComponent<ActorHitbox>() && !finalHitObject.GetComponent<CharacterController>())
                    {
                        Debug.Log("asdf10");

                        try
                        {
                            if (!finalHitObject.GetComponent<PlayerHitbox>())
                                try
                                {
                                    finalHitDamageable.Damage(damage, false, sourcePlayer.pid);

                                }
                                catch
                                {
                                    finalHitDamageable.Damage(damage);
                                }
                        }
                        catch { }
                        GameObject genericHit = gameObjectPool.SpawnPooledGenericHit();
                        genericHit.transform.position = finalHitPoint;
                        genericHit.SetActive(true);

                        GameObject imp = gameObjectPool.SpawnBulletMetalImpactObject();
                        imp.transform.position = finalHitPoint;
                        imp.transform.rotation = Quaternion.LookRotation(hitInfo.normal);
                        imp.SetActive(true);

                        damageDealt = true;
                    }
                }

                #region
                // New logic
                //if (finalHitObject.GetComponent<PlayerHitbox>() && !finalHitObject.GetComponent<PlayerHitbox>().player.isDead && !finalHitObject.GetComponent<PlayerHitbox>().player.isRespawning)
                //{
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
                //            finalHitDamageable.Damage(damage, wasHeadshot, playerWhoShot.GetComponent<PhotonView>().ViewID, finalHitPoint, weaponProperties.codeName, isGroin: wasNutshot);
                //        else
                //            finalHitDamageable.Damage(damage, wasHeadshot, playerWhoShot.GetComponent<PhotonView>().ViewID, finalHitPoint, isGroin: wasNutshot);
                //    }

                //    damageDealt = true;
                //}
                //else if (!finalHitObject.GetComponent<PlayerHitbox>() && !finalHitObject.GetComponent<CapsuleCollider>() && !finalHitObject.GetComponent<AIHitbox>() && !finalHitObject.GetComponent<CharacterController>())
                //{
                //    try
                //    {
                //        finalHitDamageable.Damage(damage);
                //    }
                //    catch { }
                //    GameObject genericHit = FindObjectOfType<GameObjectPool>().SpawnPooledGenericHit();
                //    genericHit.transform.position = finalHitPoint;
                //    genericHit.SetActive(true);

                //    damageDealt = true;
                //}
                #endregion
                gameObject.SetActive(false);
            }
            catch (System.Exception e) { Debug.LogWarning(e); }

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
        public RaycastHit raycastHit;
        public GameObject gameObject;
        public Vector3 hitPoint;
        public float distanceFromPlayer;
        public ObjectHit(GameObject _gameobject, RaycastHit hitInfo, Vector3 _hitPoint, float dist)
        {
            raycastHit = hitInfo;
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
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;
using System.Linq;

public class Bullet : MonoBehaviourPunCallbacks
{
    public Player sourcePlayer { get { return _sourcePlayer; } set { _sourcePlayer = value; _spawnDir = sourcePlayer.transform.position; } }
    public Vector3 spawnDir
    {
        get { return _spawnDir; }
    }
    public bool trailOnly { get { return _trailOnly; } set { _trailOnly = value; } }

    Player _sourcePlayer;
    Vector3 _spawnDir;


    public PhotonView PV;
    public Biped trackingTarget;
    [Header("Other Scripts")]
    // BulletProperties bProperties;
    public AllPlayerScripts allPlayerScripts;
    public WeaponProperties weaponProperties;
    public Zombie zScript;
    public CrosshairManager crosshairScript;

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
    public bool overcharged;
    float defaultTimeToDespawn = .5f, timeToDespawn;


    [Header("Bullet Behavior")]
    public bool isNormalBullet;
    public bool isHeadshotCapable;
    public bool canBleedthroughHeadshot;
    public bool canBleedthroughAnything;

    public bool useUpdateVoid;
    public bool damageDealt;

    Vector3 playerPosWhenBulletShot;
    Vector3 _prePos, _nextPos;
    Vector3 originalPos;
    float _dTravalled;

    [Header("Impact Effects")]
    public GameObject genericHit;
    public GameObject organicBlood;
    public GameObject magicBlood;
    public GameObject shieldHit;
    public GameObject bluePlasma, greenPlasma, redPlasma, shard;

    int frameCounter;
    List<ObjectHit> objectsHit = new List<ObjectHit>();


    float _ignoreOriginPlayerTime;
    bool _addToHits, _trailOnly;

    void Awake()
    {
    }

    override public void OnEnable()
    {
        _ignoreOriginPlayerTime = 0.2f;

        _prePos = transform.position;
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
        if (_ignoreOriginPlayerTime > 0) _ignoreOriginPlayerTime -= Time.deltaTime;


        if (!_trailOnly) Despawn();
        ShootRay();
        Travel();
    }
    private void LateUpdate()
    {
        //Travel();
    }


    float _distanceTravalled;
    //List<int> bulletLayers = new List<int> { 0, 7, 12, 14 };



    List<RaycastHit> _hitList = new List<RaycastHit>();
    RaycastHit _tempRh;
    void ShootRay()
    {
        _prePos = transform.position;
        _nextPos = transform.position + transform.TransformDirection(Vector3.forward) * speed * Time.deltaTime;
        //transform.Translate(Vector3.forward * Time.deltaTime * bulletSpeed); // Moves the bullet at 'bulletSpeed' units per second

        _dTravalled = Vector3.Distance(_prePos, _nextPos);






        RaycastHit[] hits;
        _hitList = Physics.RaycastAll(_prePos, (_nextPos - _prePos).normalized, maxDistance: _dTravalled, layerMask: GameManager.instance.bulletLayerMask).ToList();
        _hitList.RemoveAll(hit => hit.collider.GetComponent<ManCannon>());

        if (_hitList.Count > 0)
        {
            if (_ignoreOriginPlayerTime > 0)
                for (int i = _hitList.Count; i-- > 0;)
                {
                    _tempRh = _hitList[i];
                    print($"bullet hit: {_hitList[i].collider.name}");
                    if (_tempRh.collider.transform.root == weaponProperties.player.transform) _hitList.RemoveAt(i);
                }




            if (_hitList.Count > 0)
            {
                _hitList = _hitList.OrderBy(x => Vector2.Distance(_prePos, x.point)).ToList();




                _tempRh = _hitList[0];
                float _distanceFromSpawnToHit = Vector3.Distance(originalPos, _tempRh.point);

                if (_distanceFromSpawnToHit <= weaponProperties.range)
                {
                    ObjectHit newHit = new ObjectHit(_tempRh.collider.gameObject, _tempRh, _tempRh.point, Vector3.Distance(playerPosWhenBulletShot, _tempRh.point));
                    objectsHit.Add(newHit);
                }

                if (objectsHit.Count > 0)
                {
                    if (damage > 0) CheckForFinalHit();
                    print($"bullet time test. Despawned at: {Time.time}");


                    if (trailOnly && GetComponent<FakeBulletTrailDisable>()) GetComponent<FakeBulletTrailDisable>().TriggerDisable(); else gameObject.SetActive(false);
                }
            }
        }



        return;
        RaycastHit[] m_Results = new RaycastHit[5];
        Ray r = new Ray(_prePos, (_nextPos - _prePos).normalized);


        RaycastHit fhit;
        if (Physics.Raycast(r.origin, r.direction, out fhit, maxDistance: _dTravalled, layerMask: GameManager.instance.bulletLayerMask))
        {
            _addToHits = true;
            Debug.Log($"Bullet hit: {fhit.collider.gameObject.name}. LAYER: {fhit.collider.gameObject.layer}. Root: {fhit.transform.root.name}. Check :{_ignoreOriginPlayerTime}");



            if (fhit.collider.GetComponent<IDamageable>() != null || fhit.collider)
            {
                if (fhit.transform.root.GetComponent<Player>())
                    if (fhit.transform.root.GetComponent<Player>() == weaponProperties.transform.root.GetComponent<Player>() && _ignoreOriginPlayerTime > 0)
                        _addToHits = false;



                GameObject hit = fhit.collider.gameObject;
                float _distanceFromSpawnToHit = Vector3.Distance(originalPos, fhit.point);

                if (_distanceFromSpawnToHit <= weaponProperties.range && _addToHits)
                {
                    ObjectHit newHit = new ObjectHit(hit, fhit, fhit.point, Vector3.Distance(playerPosWhenBulletShot, fhit.point));
                    objectsHit.Add(newHit);
                }
            }

            if (objectsHit.Count > 0)
            {
                CheckForFinalHit();
                gameObject.SetActive(false);
            }
        }
    }




    Vector3 _bulletToTrackingTargetDirection = Vector3.zero;
    void Travel()
    {
        //print($"Bullet has tracking target {weaponProperties.targetTracking} {trackingTarget}");

        transform.Translate(Vector3.forward * Time.deltaTime * speed);
        if (weaponProperties && weaponProperties.targetTracking && trackingTarget)
        {
            if (!trackingTarget.gameObject.activeInHierarchy || trackingTarget.GetComponent<HitPoints>().isDead) trackingTarget = null;


            if (trackingTarget)
            {
                _bulletToTrackingTargetDirection = trackingTarget.targetTrackingCorrectTarget.position - transform.position;
                if (Vector3.Angle(_bulletToTrackingTargetDirection, transform.forward) > 70) trackingTarget = null;


                if (trackingTarget)
                {
                    Vector3 dir = trackingTarget.targetTrackingCorrectTarget.position - transform.position;
                    Quaternion rot = Quaternion.LookRotation(dir);
                    transform.rotation = Quaternion.Lerp(transform.rotation, rot, weaponProperties.trackingSpeed * Time.deltaTime);
                }
            }
        }
        else
        {
            //Debug.LogWarning("You are missing a reference");
        }

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
                        finalHitDamageable.Damage(damage, ih, sourcePlayer.photonId, weaponIndx: weaponProperties.index);

                    }
                    catch
                    {
                        finalHitDamageable.Damage(damage);
                    }
                    genericHit = GameObjectPool.instance.SpawnPooledBloodHit();
                    genericHit.transform.position = finalHitPoint;
                    genericHit.SetActive(true);

                    damageDealt = true;

                    return;
                }

                //if (GameManager.instance.teamMode == GameManager.TeamMode.Classic)
                //{
                //    if (finalHitObject.GetComponent<PlayerHitbox>())
                //    {
                //        Debug.Log(sourcePlayer.team.ToString());
                //        Debug.Log(finalHitObject.GetComponent<PlayerHitbox>().player.team.ToString());

                //        if (finalHitObject.GetComponent<PlayerHitbox>().player.team != sourcePlayer.team)
                //        {
                //            if (finalHitObject.GetComponent<PlayerHitbox>() && !finalHitObject.GetComponent<PlayerHitbox>().player.isDead && !finalHitObject.GetComponent<PlayerHitbox>().player.isRespawning)
                //            {
                //                PlayerHitbox hitbox = finalHitObject.GetComponent<PlayerHitbox>();
                //                Player player = hitbox.player.GetComponent<Player>();
                //                bool wasHeadshot = false;
                //                bool wasNutshot = false;
                //                if (weaponProperties.isHeadshotCapable && (hitbox.isHead || hitbox.isGroin))
                //                {
                //                    int maxShieldPoints = player.maxHitPoints - player.maxHealthPoints;

                //                    if (maxShieldPoints > 0 && (player.hitPoints <= player.maxHealthPoints))
                //                        damage = player.maxHealthPoints;

                //                    if (weaponProperties.weaponType == WeaponProperties.WeaponType.Sniper)
                //                        damage = (int)(damage * weaponProperties.headshotMultiplier);

                //                    wasHeadshot = hitbox.isHead;
                //                    wasNutshot = hitbox.isGroin;

                //                }

                //                //if (sourcePlayer.PV.IsMine)
                //                {
                //                    if (weaponProperties.codeName != null)
                //                        finalHitDamageable.Damage(damage, wasHeadshot, sourcePlayer.GetComponent<PhotonView>().ViewID, finalHitPoint, impactDir: spawnDir, damageSource: weaponProperties.cleanName, isGroin: wasNutshot);
                //                    else
                //                        finalHitDamageable.Damage(damage, wasHeadshot, sourcePlayer.GetComponent<PhotonView>().ViewID, finalHitPoint, isGroin: wasNutshot);
                //                }

                //                damageDealt = true;
                //            }
                //        }
                //        else
                //        {
                //            GameObject genericHit = gameObjectPool.SpawnPooledGenericHit();
                //            genericHit.transform.position = finalHitPoint;
                //            genericHit.SetActive(true);

                //            damageDealt = true;

                //            GameObject imp = gameObjectPool.SpawnBulletMetalImpactObject();
                //            imp.transform.position = finalHitPoint;
                //            imp.transform.rotation = Quaternion.LookRotation(hitInfo.normal);
                //            imp.SetActive(true);
                //        }
                //    }
                //    else
                //    {
                //        try
                //        {
                //            Debug.Log("asdf1234");
                //            if (!finalHitObject.GetComponent<PlayerHitbox>())
                //                try
                //                {
                //                    if (finalHitObject.GetComponent<ActorHitbox>() && finalHitObject.GetComponent<ActorHitbox>().isHead)
                //                        damage = (int)(damage * weaponProperties.headshotMultiplier);
                //                    finalHitDamageable.Damage(damage, false, sourcePlayer.photonId);

                //                }
                //                catch
                //                {
                //                    finalHitDamageable.Damage(damage);
                //                }
                //        }
                //        catch { }

                //        GameObject genericHit;
                //        if (finalHitObject.GetComponent<ActorHitbox>())
                //            genericHit = gameObjectPool.SpawnPooledBloodHit();
                //        else
                //        {
                //            genericHit = gameObjectPool.SpawnPooledGenericHit();

                //            GameObject imp = gameObjectPool.SpawnBulletMetalImpactObject();
                //            imp.transform.position = finalHitPoint;
                //            imp.transform.rotation = Quaternion.LookRotation(hitInfo.normal);
                //            imp.SetActive(true);
                //        }
                //        genericHit.transform.position = finalHitPoint;
                //        genericHit.SetActive(true);

                //        damageDealt = true;
                //    }

                //}
                //else
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


                        if (player.playerController.cameraIsFloating) damage = 0;

                        //if (sourcePlayer.PV.IsMine)
                        {
                            Debug.Log("asdf6");


                            if (player.hasArmor)
                            {
                                print($"Bullet Overcharged damage : {player.shieldPoints}");
                                if (player.shieldPoints > 0 && overcharged) damage = (int)player.shieldPoints;
                            }



                            if (weaponProperties.codeName != null)
                                finalHitDamageable.Damage(damage, wasHeadshot, sourcePlayer.GetComponent<PhotonView>().ViewID, finalHitPoint, impactDir: spawnDir, weaponProperties.cleanName, isGroin: wasNutshot, weaponIndx: weaponProperties.index, kfo: weaponProperties.killFeedOutput);
                            else
                                finalHitDamageable.Damage(damage, wasHeadshot, sourcePlayer.GetComponent<PhotonView>().ViewID, finalHitPoint, isGroin: wasNutshot, weaponIndx: weaponProperties.index, kfo: weaponProperties.killFeedOutput);
                        }

                        damageDealt = true;
                    }
                    else if (!finalHitObject.GetComponent<PlayerHitbox>() && !finalHitObject.GetComponent<CapsuleCollider>() && !finalHitObject.GetComponent<ActorHitbox>() && !finalHitObject.GetComponent<CharacterController>())
                    {
                        //Debug.Log($"Bullet ELSEIF {finalHitObject.name}");

                        try
                        {
                            if (!finalHitObject.GetComponent<PlayerHitbox>())
                            {
                                if (finalHitDamageable == null)
                                {

                                    //Debug.Log($"No IDamageeable script on {finalHitObject.name}");
                                }
                                else
                                {
                                    try
                                    {
                                        finalHitDamageable.Damage(damage, false, sourcePlayer.photonId);
                                    }
                                    catch (Exception e)
                                    {
                                        Debug.Log(finalHitObject.name);
                                        Debug.LogException(e);
                                        //finalHitDamageable.Damage(damage);
                                    }
                                }
                            }
                        }
                        catch { }
                        GameObjectPool.instance.SpawnPooledGenericHit(finalHitPoint, hitInfo.normal);

                        if (finalHitObject.GetComponent<IDamageable>() == null && !finalHitObject.GetComponent<DontSpawnBulletHoleDecalHere>()) // avoids staying in empty space after glass is destroyed
                        {
                            //print($"SpawnBulletHole {hitInfo.transform.name}");
                            GameObjectPool.instance.SpawnBulletHole(finalHitPoint, hitInfo.normal);
                        }

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

    private void OnDisable()
    {
        _trailOnly = false;
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

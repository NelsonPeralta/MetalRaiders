using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Bullet;
using static Rewired.ComponentControls.Effects.RotateAroundAxis;

public class Fireball : MonoBehaviour
{
    public GameObject sourceBiped;

    [Header("Settings")]
    public float damage;
    public float speed;
    public float radius;
    public float power;
    public int deSpawnTime;


    [Header("Prefabs")]
    public Transform explosionPrefab;

    List<Player> playersHit = new List<Player>();
    GameObject[] AIsHit = new GameObject[20];
    List<ObjectHit> objectsHit = new List<ObjectHit>();


    float _despawnTime;
    Vector3 _prePos, _nextPos, originalPos, _playerPosWhenBulletShot, _spawnDir;


    private void Awake()
    {
        _despawnTime = 10;
        _playerPosWhenBulletShot = transform.position;
    }

    private void Start()
    {
    }

    private void Update()
    {
        Despawn();
        ShootRay();
        Travel();
    }


    //void Explosion()
    //{
    //    var explosion = Instantiate(explosionPrefab, transform.position, transform.rotation);
    //    Destroy(explosion.gameObject, 5);

    //    //Explosion force
    //    Vector3 explosionPos = transform.position;
    //    //Use overlapshere to check for nearby colliders
    //    Collider[] colliders = Physics.OverlapSphere(explosionPos, radius);
    //    foreach (Collider hit in colliders)
    //    {
    //        Rigidbody rb = hit.GetComponent<Rigidbody>();
    //        //Log.Print(() =>$"{name} explosion on {hit.transform.name}");
    //        float hitDistance = Vector3.Distance(hit.transform.position, transform.position);
    //        float calculatedDamage = damage * (1 - (hitDistance / radius));

    //        //Add force to nearby rigidbodies
    //        //if (rb != null && rb.gameObject.layer != gameObject.layer)
    //        //    rb.AddExplosionForce(power, explosionPos, radius, 3.0F);

    //        if (hit.GetComponent<PlayerHitbox>() && !playersHit.Contains(hit.GetComponent<PlayerHitbox>().player))
    //        {
    //            GameObject player = hit.GetComponent<PlayerHitbox>().player.gameObject;
    //            float playerDistance = Vector3.Distance(hit.transform.position, transform.position);
    //            int playerHitID = player.GetComponent<Player>().controllerId;


    //            if (playerDistance < radius)
    //            {
    //                //Log.Print(() =>"Damage= " + damage + " playerDistance= " + playerDistance + " radius= " + radius);
    //                player.GetComponent<Player>().Damage((int)calculatedDamage, false, sourceBiped.GetComponent<PhotonView>().ViewID);
    //                playersHit.Add(player.GetComponent<Player>());
    //            }
    //        }
    //        if (hit.GetComponent<AIHitbox>() != null)
    //        {
    //            //Log.Print(() =>"Hit AI");
    //            GameObject ai;
    //            ai = hit.GetComponent<AIHitbox>().aiGO;
    //            float aiDistance = Vector3.Distance(hit.transform.position, transform.position);

    //            bool aiAlreadyHit = false;

    //            for (int i = 0; i < AIsHit.Length; i++)
    //            {
    //                if (AIsHit[i] != null)
    //                {
    //                    if (ai == AIsHit[i])
    //                    {
    //                        aiAlreadyHit = true;
    //                    }
    //                }
    //            }

    //            bool assignedAIInArray = false;

    //            if (!aiAlreadyHit)
    //            {
    //                for (int i = 0; i < AIsHit.Length; i++)
    //                {
    //                    if (AIsHit[i] == null && !assignedAIInArray)
    //                    {
    //                        AIsHit[i] = ai;
    //                        assignedAIInArray = true;
    //                    }
    //                }
    //            }

    //            if (!aiAlreadyHit)
    //            {
    //                if (hit.GetComponent<AIHitbox>().aiHealth > 0)
    //                {
    //                    hit.GetComponent<AIHitbox>().DamageAI(false, calculatedDamage, sourceBiped);
    //                }

    //            }
    //        }
    //    }

    //    //Destroy the grenade object on explosion
    //    Destroy(gameObject);
    //}



    void ShootRay()
    {
        objectsHit.Clear();
        _prePos = transform.position;
        _nextPos = transform.position + transform.TransformDirection(Vector3.forward) * speed * Time.deltaTime;

        float _dTravalled = Vector3.Distance(_prePos, _nextPos);

        RaycastHit[] m_Results = new RaycastHit[5];
        Ray r = new Ray(_prePos, (_nextPos - _prePos).normalized);
        int dLayer = 0;
        int hLayer = 7;

        int finalmask = (1 << dLayer) | (1 << hLayer);

        RaycastHit fhit;
        if (Physics.Raycast(r.origin, r.direction, out fhit, maxDistance: _dTravalled, finalmask))
        {
            Log.Print(() =>$"HIT: {fhit.collider.gameObject.name}. LAYER: {fhit.collider.gameObject.layer}");


            GameObject hit = fhit.collider.gameObject;

            if (fhit.collider.GetComponent<IDamageable>() != null || fhit.collider)
            {
                {
                    ObjectHit newHit = new ObjectHit(hit, fhit, fhit.point, Vector3.Distance(_playerPosWhenBulletShot, fhit.point));
                    objectsHit.Add(newHit);
                }
            }

            CheckForFinalHit();

            Despawn(true);
        }
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
            Log.Print(() =>$"BULLET CheckForFinalHit {finalHitObject.name}");


            //finalHitObject.GetComponent<PropHitbox>().hitPoints.Damage((int)damage, false, sourceBiped.GetComponent<PhotonView>().ViewID);
            if (finalHitObject.GetComponent<PlayerHitbox>())
            {
                if (!GameManager.instance.devMode)
                    finalHitDamageable.Damage((int)damage, false, sourceBiped.GetComponent<PhotonView>().ViewID);
            }

            Despawn(true);
        }

    }


    void Travel()
    {
        transform.Translate(Vector3.forward * Time.deltaTime * speed);
    }

    void Despawn(bool force = false)
    {
        if (force)
        {
            transform.parent = MapCamera.instance.disabledJunk;
            gameObject.SetActive(false);
        }
        else if (_despawnTime > 0)
        {
            _despawnTime -= Time.deltaTime;
            if (_despawnTime <= 0)
            {
                transform.parent = MapCamera.instance.disabledJunk;
                gameObject.SetActive(false);
            }
        }
    }
}

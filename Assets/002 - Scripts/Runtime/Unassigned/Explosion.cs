using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    public enum Type { Grenade, RPG, Barrel, UltraBind }
    public enum Color { Yellow, Blue, Purple, Water }

    public delegate void ExplosionEvent(Explosion explosion);
    public ExplosionEvent OnObjectAdded;

    public Player player { get { return _player; } set { Log.Print(() => $"SETTING EXPLOSION PLAYER {value.name}"); _player = value; } }
    public bool stuck { get { return _stuck; } set { _stuck = value; } }

    public string damageSource { get { return _damageSource; } set { _damageSource = value; } }

    public Color color
    {
        set
        {
            _yellowModel.SetActive(value == Color.Yellow); _blueModel.SetActive(value == Color.Blue); _purpleModel.SetActive(value == Color.Purple); _waterModel.SetActive(value == Color.Water);
        }
    }

    public Type type
    {
        get { return _type; }
        set
        {
            _type = value;

            //if (value == Type.Grenade) damageSource = "Grenade";
            //else if (value == Type.Barrel) damageSource = "Barrel";
            //else if (value == Type.UltraBind) damageSource = "Ultra Bind";
            //else if (value == Type.RPG) damageSource = "RPG";
        }
    }

    [SerializeField] Player _player;
    [SerializeField] string _damageSource;
    public WeaponProperties.KillFeedOutput killFeedOutput;

    [Header("Settings")]
    public float damage; // Determined in Weapon Properties Script
    public float radius;
    public float explosionPower;

    [SerializeField] LayerMask _obsLayerMask;

    List<GameObject> objectsHit = new List<GameObject>();
    bool _stuck;
    [SerializeField] GameObject _yellowModel, _blueModel, _purpleModel, _waterModel;
    Type _type;
    float _defaultSpatialBlend, _defaultVolume;



    private void Awake()
    {
        _defaultSpatialBlend = GetComponent<AudioSource>().spatialBlend;
        _defaultVolume = GetComponent<AudioSource>().volume;
    }
    private void Start()
    {
        //Explode();
    }

    private void OnEnable()
    {
        if (GameManager.instance.connection == GameManager.NetworkType.Local || GameManager.instance.nbLocalPlayersPreset > 1)
        {
            float _distanceFromRootPlayer = Vector3.Distance(GameManager.GetRootPlayer().transform.position, transform.position);
            float _closestDistanceToThisExplosion = _distanceFromRootPlayer;

            foreach (Player p in GameManager.GetLocalPlayers().Where(item => item != GameManager.GetRootPlayer()))
            {
                if (Vector3.Distance(p.transform.position, transform.position) < _closestDistanceToThisExplosion)
                    _closestDistanceToThisExplosion = Vector3.Distance(p.transform.position, transform.position);
            }

            float _ratio = _closestDistanceToThisExplosion / _distanceFromRootPlayer;

            GetComponent<AudioSource>().spatialBlend = _ratio * _defaultSpatialBlend;
        }
    }



    private readonly Collider[] _overlapSphereBuffer = new Collider[512]; // Preallocated reusable buffer

    public void Explode()
    {
        objectsHit.Clear();
        HashSet<Collider> processedColliders = new HashSet<Collider>();

        // Use OverlapSphereNonAlloc to avoid allocating a new array
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, radius, _overlapSphereBuffer);
        //Debug.Log($"Explosion {hitCount}");

        for (int i = 0; i < hitCount; i++)
        {
            Collider col = _overlapSphereBuffer[i];

            // Skip duplicates
            if (!processedColliders.Add(col))
                continue;

            float hitDistance = Vector3.Distance(transform.position, col.transform.position);
            if (hitDistance > radius)
                continue;

            float disRatio = 1 - (hitDistance / radius);
            float calculatedPower = explosionPower * disRatio;
            int calculatedDamage = (int)(damage * disRatio);

            Log.Print(() => $"EXPLOSION {col.name}. ROOT: {col.transform.root}");

            // Skip obstructions if not collectible
            if (!col.name.Contains("llectible"))
            {
                Vector3 directionToTarget = (col.transform.position - transform.position).normalized;
                if (Physics.Raycast(transform.position, directionToTarget, out RaycastHit hit, hitDistance, _obsLayerMask))
                {
                    if (hit.collider != col)
                        continue;
                }
            }

            // OddballSkull special case
            if (col.GetComponent<OddballSkull>() && PhotonNetwork.IsMasterClient)
            {
                objectsHit.Add(col.gameObject);
                NetworkGameManager.instance.AddForceToOddball(calculatedPower, transform.position, radius, 3.0f);
            }

            Rigidbody rb = col.GetComponent<Rigidbody>();

            // Apply forces to rigidbodies
            if (rb != null && !rb.isKinematic)
            {
                Log.Print(() => $"Explosion force added to: {rb.name} {rb.transform.root.name}");

                if (rb.GetComponent<LootableWeapon>())
                {
                    rb.AddExplosionForce(calculatedPower * 2, transform.position, radius, 3.0f);
                    Log.Print(() => "Explosion force added: 1");
                }
                else if (rb.GetComponent<RagdollLimb>())
                {
                    var ragdollRoot = rb.GetComponentInParent<PlayerRagdoll>().gameObject;
                    if (!objectsHit.Contains(ragdollRoot))
                    {
                        calculatedPower = Mathf.Clamp(calculatedPower, calculatedDamage * 40, 8000);
                        rb.GetComponentInParent<PlayerRagdoll>().hips.GetComponent<Rigidbody>()
                            .AddExplosionForce(calculatedPower, transform.position, radius, 3.0f);
                        objectsHit.Add(ragdollRoot);
                        Log.Print(() => "Explosion force added: 2");
                    }
                    else Log.Print(() => "Explosion force added: 2 - skipped");
                }
                else if (!rb.transform.root.GetComponent<PlayerRagdoll>() && rb.gameObject != gameObject)
                {
                    rb.AddExplosionForce(calculatedPower, transform.position, radius, 3.0f);
                    Log.Print(() => "Explosion force added: 3");
                }
                else
                {
                    rb.AddExplosionForce(calculatedPower * 3.3f, transform.position, radius, 4.0f);
                    Log.Print(() => "Explosion force added: 4");
                }
            }

            // Damage players
            if (col.GetComponent<PlayerHitbox>() && !col.GetComponent<PlayerHitbox>().player.isDead && !col.GetComponent<PlayerHitbox>().player.isRespawning)
            {
                GameObject playerHit = col.GetComponent<PlayerHitbox>().player.gameObject;
                if (!objectsHit.Contains(playerHit))
                {
                    objectsHit.Add(playerHit);
                    OnObjectAdded?.Invoke(this);

                    if (killFeedOutput == WeaponProperties.KillFeedOutput.Grenade_Launcher && calculatedDamage > 120) calculatedDamage = 120;

                    try
                    {
                        Log.Print(() => $"EXPLOSION {name} DAMAGING PLAYER at {transform.position}");
                        //Debug.Log($"Explosion damage player {calculatedDamage}");
                        if (player.isMine)
                            col.GetComponent<PlayerHitbox>().Damage(calculatedDamage, false, player.photonId,
                                damageSource: this._damageSource, impactDir: col.transform.position - transform.position, kfo: killFeedOutput);
                    }
                    catch
                    {
                        if (col.GetComponent<PlayerHitbox>().player.isMine)
                            col.GetComponent<PlayerHitbox>().Damage(calculatedDamage);
                    }
                }
            }
            // Damage other IDamageable objects
            else if (col.GetComponent<IDamageable>() != null)
            {
                GameObject hitObject = col.gameObject;

                if (col.GetComponent<ActorHitbox>() && objectsHit.Contains(col.GetComponent<ActorHitbox>().actor.gameObject))
                    continue;
                else if (col.GetComponent<ActorHitbox>())
                    objectsHit.Add(col.GetComponent<ActorHitbox>().actor.gameObject);

                if (!objectsHit.Contains(hitObject))
                {
                    objectsHit.Add(hitObject);
                    OnObjectAdded?.Invoke(this);

                    try
                    {
                        hitObject.GetComponent<IDamageable>().Damage(calculatedDamage, false, player.photonId,
                            impactDir: col.transform.position - transform.position);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning(e + " " + hitObject.name);
                    }
                }
            }
        }
    }





    //public void Explode()
    //{
    //    objectsHit.Clear();
    //    //Use overlapshere to check for nearby colliders
    //    Collider[] colliders = Physics.OverlapSphere(transform.position, radius).Distinct().ToArray();

    //    for (int i = 0; i < colliders.Length; i++) // foreach(Collider hit in colliders)
    //    {
    //        Collider col = colliders[i];
    //        float hitDistance = Vector3.Distance(transform.position, col.transform.position);

    //        if (hitDistance > radius)
    //            continue;

    //        float disRatio = 1 - (hitDistance / radius);
    //        float calculatedPower = explosionPower * disRatio;
    //        int calculatedDamage = (int)(damage * disRatio);





    //        Log.Print(() =>$"EXPLOSION {col.name}. ROOT: {col.transform.root}");

    //        // Check if the is an obstruction between explosion and the hit
    //        if (!col.name.Contains("llectible"))
    //        {
    //            Transform or = transform;

    //            Vector3 directionToTarget = (col.transform.position - or.position).normalized;
    //            RaycastHit hit;
    //            if (Physics.Raycast(or.position, directionToTarget, out hit, hitDistance, _obsLayerMask))
    //            {
    //                //Log.Print(() =>$"EXPLOSION. Hit {col.name} but colliding with {hit.collider.name}");
    //                if (hit.collider != col) // Checks if the object obstructing is not the hit itself if it happens to be of layer Default
    //                    continue;
    //            }
    //        }

    //        if (col.GetComponent<OddballSkull>() && PhotonNetwork.IsMasterClient)
    //        {
    //            objectsHit.Add(col.gameObject);
    //            NetworkGameManager.instance.AddForceToOddball(calculatedPower, transform.position, radius, 3.0F);
    //        }


    //        Rigidbody rb = col.GetComponent<Rigidbody>();




    //        //Add force to nearby rigidbodies
    //        if (rb != null && !rb.isKinematic) // does not actually detect rb in player root, grenade jumping is handled in player script
    //        {
    //            Log.Print(() => $"Explosion force added to: {rb.name} {rb.transform.root.name}");
    //            Log.Print(() => $"Explosion force added to: {calculatedPower} {rb.GetComponent<LootableWeapon>() != null} {rb.GetComponent<RagdollLimb>() != null}");
    //            if (rb.GetComponent<LootableWeapon>())
    //            {
    //                rb.AddExplosionForce(calculatedPower * 2, transform.position, radius, 3.0F);
    //                Log.Print(() => "Explosion force added: 1");
    //            }
    //            else if (rb.GetComponent<RagdollLimb>())
    //            {
    //                if (!objectsHit.Contains(rb.GetComponentInParent<PlayerRagdoll>().gameObject))
    //                {
    //                    calculatedPower = Mathf.Clamp(calculatedPower, calculatedDamage * 40, 8000);
    //                    rb.GetComponentInParent<PlayerRagdoll>().hips.GetComponent<Rigidbody>().AddExplosionForce(calculatedPower, transform.position, radius, 3.0F);
    //                    objectsHit.Add(rb.GetComponentInParent<PlayerRagdoll>().gameObject);
    //                    Log.Print(() => "Explosion force added: 2");
    //                }
    //                else
    //                {
    //                    Log.Print(() => "Explosion force added: 2 - skipped");
    //                }
    //            }
    //            else if (!rb.transform.root.GetComponent<PlayerRagdoll>() && rb.gameObject != gameObject)
    //            {
    //                rb.AddExplosionForce(calculatedPower, transform.position, radius, 3.0F);
    //                Log.Print(() => "Explosion force added: 3");
    //            }
    //            else
    //            {
    //                rb.AddExplosionForce(calculatedPower * 3.3f, transform.position, radius, 4.0F);
    //                Log.Print(() => "Explosion force added: 4");
    //            }
    //        }


    //        if (col.GetComponent<PlayerHitbox>() && !col.GetComponent<PlayerHitbox>().player.isDead && !col.GetComponent<PlayerHitbox>().player.isRespawning)
    //        {
    //            GameObject playerHit = col.GetComponent<PlayerHitbox>().player.gameObject;
    //            if (!objectsHit.Contains(playerHit))
    //            {
    //                objectsHit.Add(playerHit);
    //                OnObjectAdded?.Invoke(this);

    //                try
    //                {
    //                    Log.Print(() =>$"EXPLOSION {name} DAMAGING PLAYER at {transform.position}");
    //                    //if (stuck)
    //                    //    _damageSource = "Stuck";
    //                    if (player.isMine)
    //                        col.GetComponent<PlayerHitbox>().Damage((int)calculatedDamage, false, player.photonId, damageSource: this._damageSource, impactDir: (col.transform.position - transform.position), kfo: killFeedOutput);
    //                }
    //                catch { if (col.GetComponent<PlayerHitbox>().player.isMine) col.GetComponent<PlayerHitbox>().Damage((int)calculatedDamage); }
    //            }
    //        }
    //        else if (col.GetComponent<IDamageable>() != null)
    //        {
    //            if (col.GetComponent<ActorHitbox>())
    //                if (objectsHit.Contains(col.GetComponent<ActorHitbox>().actor.gameObject))
    //                    return;
    //                else
    //                    objectsHit.Add(col.GetComponent<ActorHitbox>().actor.gameObject);
    //            else;

    //            GameObject hitObject = col.gameObject;

    //            if (!objectsHit.Contains(hitObject))
    //            {
    //                Log.Print(() =>hitObject.name);
    //                //Log.Print(() =>calculatedDamage);

    //                objectsHit.Add(hitObject);
    //                OnObjectAdded?.Invoke(this);
    //                try
    //                {
    //                    Log.Print(() =>calculatedDamage);
    //                    Log.Print(() =>player.photonId);
    //                    Log.Print(() =>col.transform.position - transform.position);
    //                    hitObject.GetComponent<IDamageable>().Damage(calculatedDamage, false, player.photonId, impactDir: (col.transform.position - transform.position));
    //                }
    //                catch (System.Exception e) { Debug.LogWarning(e + " " + hitObject.name); }
    //            }
    //        }


    //    }
    //    //Destroy(gameObject, 10);
    //}

    public void DisableIn3Seconds()
    {
        StartCoroutine(DIsableIn3Seconds_Coroutine());
    }

    IEnumerator DIsableIn3Seconds_Coroutine()
    {
        yield return new WaitForSeconds(3);

        gameObject.SetActive(false);
    }
}

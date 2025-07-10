using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    public enum Type { Grenade, RPG, Barrel, UltraBind }
    public enum Color { Yellow, Blue, Purple }

    public delegate void ExplosionEvent(Explosion explosion);
    public ExplosionEvent OnObjectAdded;

    public Player player { get { return _player; } set { Debug.Log($"SETTING EXPLOSION PLAYER {value.name}"); _player = value; } }
    public bool stuck { get { return _stuck; } set { _stuck = value; } }

    public string damageSource { get { return _damageSource; } set { _damageSource = value; } }

    public Color color
    {
        set
        {
            _yellowModel.SetActive(value == Color.Yellow); _blueModel.SetActive(value == Color.Blue); _purpleModel.SetActive(value == Color.Purple);
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
    [SerializeField] GameObject _yellowModel, _blueModel, _purpleModel;
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
        if (GameManager.instance.connection == GameManager.Connection.Local || GameManager.instance.nbLocalPlayersPreset > 1)
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

    public void Explode()
    {
        objectsHit.Clear();
        //Use overlapshere to check for nearby colliders
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius).Distinct().ToArray();

        for (int i = 0; i < colliders.Length; i++) // foreach(Collider hit in colliders)
        {
            Collider col = colliders[i];
            float hitDistance = Vector3.Distance(transform.position, col.transform.position);

            if (hitDistance > radius)
                continue;

            float disRatio = 1 - (hitDistance / radius);
            float calculatedPower = explosionPower * disRatio;
            int calculatedDamage = (int)(damage * disRatio);





            Debug.Log($"EXPLOSION {col.name}. ROOT: {col.transform.root}");

            // Check if the is an obstruction between explosion and the hit
            if (!col.name.Contains("llectible"))
            {
                Transform or = transform;

                Vector3 directionToTarget = (col.transform.position - or.position).normalized;
                RaycastHit hit;
                if (Physics.Raycast(or.position, directionToTarget, out hit, hitDistance, _obsLayerMask))
                {
                    //Debug.Log($"EXPLOSION. Hit {col.name} but colliding with {hit.collider.name}");
                    if (hit.collider != col) // Checks if the object obstructing is not the hit itself if it happens to be of layer Default
                        continue;
                }
            }

            if (col.GetComponent<OddballSkull>() && PhotonNetwork.IsMasterClient)
            {
                objectsHit.Add(col.gameObject);
                NetworkGameManager.instance.AddForceToOddball(calculatedPower, transform.position, radius, 3.0F);
            }


            Rigidbody rb = col.GetComponent<Rigidbody>();




            //Add force to nearby rigidbodies
            if (rb != null && !rb.isKinematic) // does not actually detect rb in player root, grenade jumping is handled in player script
            {
                print($"Explosion force added to: {rb.name}");
                rb.AddExplosionForce(calculatedPower, transform.position, radius, 3.0F);
            }


            if (col.GetComponent<PlayerHitbox>() && !col.GetComponent<PlayerHitbox>().player.isDead && !col.GetComponent<PlayerHitbox>().player.isRespawning)
            {
                GameObject playerHit = col.GetComponent<PlayerHitbox>().player.gameObject;
                if (!objectsHit.Contains(playerHit))
                {
                    objectsHit.Add(playerHit);
                    OnObjectAdded?.Invoke(this);

                    try
                    {
                        Debug.Log($"EXPLOSION {name} DAMAGING PLAYER at {transform.position}");
                        if (stuck)
                            _damageSource = "Stuck";
                        if (player.isMine)
                            col.GetComponent<PlayerHitbox>().Damage((int)calculatedDamage, false, player.photonId, damageSource: this._damageSource, impactDir: (col.transform.position - transform.position), kfo: killFeedOutput);
                    }
                    catch { if (col.GetComponent<PlayerHitbox>().player.isMine) col.GetComponent<PlayerHitbox>().Damage((int)calculatedDamage); }
                }
            }
            else if (col.GetComponent<IDamageable>() != null)
            {
                if (col.GetComponent<ActorHitbox>())
                    if (objectsHit.Contains(col.GetComponent<ActorHitbox>().actor.gameObject))
                        return;
                    else
                        objectsHit.Add(col.GetComponent<ActorHitbox>().actor.gameObject);
                else;

                GameObject hitObject = col.gameObject;

                if (!objectsHit.Contains(hitObject))
                {
                    Debug.Log(hitObject.name);
                    //Debug.Log(calculatedDamage);

                    objectsHit.Add(hitObject);
                    OnObjectAdded?.Invoke(this);
                    try
                    {
                        Debug.Log(calculatedDamage);
                        Debug.Log(player.photonId);
                        Debug.Log(col.transform.position - transform.position);
                        hitObject.GetComponent<IDamageable>().Damage(calculatedDamage, false, player.photonId, impactDir: (col.transform.position - transform.position));
                    }
                    catch (System.Exception e) { Debug.LogWarning(e + " " + hitObject.name); }
                }
            }


        }
        //Destroy(gameObject, 10);
    }

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

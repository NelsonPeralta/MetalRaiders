using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra.Storage;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.SceneManagement;
using static Player;

public class ExplosiveProjectile : MonoBehaviour
{
    public Player player { get { return _player; } set { _player = value; } }
    //public int throwForce { get { return _throwForce; } set { _throwForce = value; } }
    public bool useConstantForce { get { return _useConstantForce; } set { _useConstantForce = value; } }
    public GameObject model { get { return _model; } }
    public GameObject visualIndicator { get { return _visualIndicator; } }
    public GameObject visualIndicatorDuplicate { get { return _visualIndicatorDuplicate; } }
    public bool stuck { get { return _stuck; } set { _stuck = value; } }

    [SerializeField] Player _player;
    [SerializeField] int _damage, _radius, /*_throwForce,*/ _explosionPower, _stuckPlayerPhotonId;
    [SerializeField] string _sourceCleanName;
    [SerializeField] WeaponProperties.KillFeedOutput _killFeedOutput;
    [SerializeField] bool _useConstantForce;
    [SerializeField] float _defaultExplosionDelayOnImpact;
    [SerializeField] Transform explosionPrefab;
    [SerializeField] AudioClip _collisionSound;
    [SerializeField] bool _sticky;
    [SerializeField] bool _stuck;
    [SerializeField] LayerMask _stickyLayerMask;
    [SerializeField] GameObject _model;
    [SerializeField] GameObject _visualIndicator;
    [SerializeField] GameObject _visualIndicatorDuplicate;
    [SerializeField] GameObject _stuckVfx;
    [SerializeField] GameObject _fakeModel;
    [SerializeField] float _ttl, _defaultTtl;
    [SerializeField] Explosion.Color _color;
    [SerializeField] Explosion.Type _type;
    [SerializeField] AudioSource _stuckSfxAudioSource;

    bool _collided, _exploded;
    float _explosionDelayOnImpact, _resetIgnoredColliders, _defaultSpatialBlend;




    private void Awake()
    {
        _lastPos = Vector3.zero;
        if (GetComponent<AudioSource>())
            _defaultSpatialBlend = GetComponent<AudioSource>().spatialBlend;
    }


    private void OnEnable()
    {
        _stuckPlayerPhotonId = -999;
        _stuck = false; // old but needed to avoid fake positives
        if (_defaultTtl <= 0) { _defaultTtl = 10; }


        _visualIndicator.SetActive(true);
        try { _stuckVfx.SetActive(false); } catch { }
        if (_sticky)
        {
            foreach (Player p in GameManager.instance.GetAllPhotonPlayers())
            {
                if (p)
                    Physics.IgnoreCollision(p.playerCapsule.GetComponent<Collider>(), GetComponent<Collider>());
            }
        }

        _ttl = _defaultTtl;

        _exploded = false;
        _collided = false; _explosionDelayOnImpact = _defaultExplosionDelayOnImpact;
        GetComponent<Collider>().enabled = true;
        GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        gameObject.layer = 8;
    }

    private void OnDisable()
    {
        _lastPos = Vector3.zero;
        if (_sticky && !GetComponent<Rigidbody>())
        {
            gameObject.AddComponent<Rigidbody>();
            GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;
            GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }
    }


    Vector3 _lastPos;



    // Update is called once per frame
    void Update()
    {

        if (GameManager.LEVELS_WITH_WATER.Contains(SceneManager.GetActiveScene().buildIndex))
        {
            if (_lastPos != Vector3.zero && _lastPos != transform.position)
            {
                RaycastHit[] hits;

                hits = Physics.RaycastAll(_lastPos, (transform.position - _lastPos), (transform.position - _lastPos).magnitude);

                for (int i = 0; i < hits.Length; i++)
                {
                    //Debug.Log($"ExplosiveProjectile splash check: {hits[i].collider.name}");
                    if (hits[i].collider.gameObject.layer == 4)
                    {
                        GameObjectPool.instance.SpawnSmallWaterEffect(hits[i].point);
                    }
                }
            }
            _lastPos = transform.position;
        }



        if (_ttl > 0)
        {
            _ttl -= Time.deltaTime;

            if (_ttl <= 0)
            {
                Log.Print($"{name} disabled due to ttl");
                gameObject.SetActive(false);
                transform.SetParent(GrenadePool.instance.transform);
            }
        }


        //if (useConstantForce && !_collided)
        //    GetComponent<Rigidbody>().AddForce
        //        (gameObject.transform.forward * throwForce);

        if (_collided)
        {
            _explosionDelayOnImpact -= Time.deltaTime;
            if (_explosionDelayOnImpact < 0)
                Explosion();
        }


        if (_resetIgnoredColliders > 0)
        {
            _resetIgnoredColliders -= Time.deltaTime;

            if (_resetIgnoredColliders <= 0)
            {
                if (_collidersToIgnore.Count > 0)
                    foreach (Collider collider in _collidersToIgnore)
                        Physics.IgnoreCollision(GetComponent<Collider>(), collider);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (GetComponent<Collider>().isTrigger) return;

        if (collision.gameObject.layer != 9)
        {
            if (_killFeedOutput == WeaponProperties.KillFeedOutput.Frag_Grenade && GetComponent<Rigidbody>().mass != 2) GetComponent<Rigidbody>().mass = 2;
            Debug.Log($"Collided with: {collision.gameObject.name} {collision.gameObject.GetComponent<Collider>()} {collision.gameObject.GetComponent<Player>()}");

            try
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

                GetComponent<AudioSource>().clip = _collisionSound; GetComponent<AudioSource>().Play();
            }
            catch { }





            if (_sticky && !_collided)
            {
                Debug.Log($"STUCK: {collision.gameObject.name} {collision.gameObject.GetComponent<PlayerHitbox>()} {collision.gameObject.GetComponent<ActorHitbox>()}");


                if (collision.gameObject.transform.root.GetComponent<Player>())
                {

                    Log.Print("Stuck 1");

                    GetComponent<Rigidbody>().linearVelocity = Vector3.zero; GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
                    if (player.isMine)
                        NetworkGameManager.StickGrenadeOnPlayer(GrenadePool.instance.stickyGrenadePool.IndexOf(gameObject), collision.gameObject.transform.root.GetComponent<Player>().PV.ViewID, collision.contacts[0].point);
                    _stuckVfx.SetActive(true);

                }
                else if (collision.gameObject.GetComponent<Rigidbody>())
                {
                    Log.Print("Stuck 2");
                    transform.parent = collision.transform;


                    GetComponent<Rigidbody>().linearVelocity = Vector3.zero; GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
                    GetComponent<Rigidbody>().useGravity = false;
                    GetComponent<Rigidbody>().isKinematic = true;
                    try { _stuckVfx.SetActive(true); } catch { }
                }
                else
                {
                    //PrintOnlyInEditor.Log($"STICK TO WALL {collision.gameObject.name} " +
                    //    $"{transform.position - collision.transform.position}   {collision.transform.InverseTransformDirection(transform.position - collision.transform.position)}");

                    //Vector3 vg = transform.position - collision.transform.position;
                    //Vector3 v = collision.transform.InverseTransformDirection(transform.position - collision.transform.position);
                    //PrintOnlyInEditor.Log(v);

                    //if (Mathf.Sign(v.x) != Mathf.Sign(vg.x) && Mathf.Abs(v.x) == Mathf.Abs(vg.x))
                    //    v.x *= -1;

                    //if (Mathf.Sign(v.y) != Mathf.Sign(vg.y) && Mathf.Abs(v.y) == Mathf.Abs(vg.y))
                    //    v.y *= -1;

                    //if (Mathf.Sign(v.z) != Mathf.Sign(vg.z) && Mathf.Abs(v.z) == Mathf.Abs(vg.z))
                    //    v.z *= -1;

                    ////v.y *= Mathf.Sign(vg.y);
                    ////v.z *= Mathf.Sign(vg.z);
                    //PrintOnlyInEditor.Log(v);

                    //ConstraintSource cs = new ConstraintSource(); cs.sourceTransform = collision.transform; cs.weight = 1;
                    //GetComponent<ParentConstraint>().AddSource(cs);
                    //GetComponent<ParentConstraint>().SetTranslationOffset(0, v);
                    //GetComponent<ParentConstraint>().SetRotationOffset(0, transform.rotation.eulerAngles);
                    //GetComponent<ParentConstraint>().locked = true;
                    //GetComponent<ParentConstraint>().constraintActive = true;


                    //_localDirToFakeParentOnColl = collision.transform.InverseTransformDirection(transform.position - collision.transform.position).normalized;
                    //_distOnColl = Vector3.Distance(transform.position, collision.transform.position);
                    //_fakeParent = collision.transform;


                    Log.Print("Stuck 3");


                    //transform.parent = collision.transform;
                    GetComponent<Rigidbody>().linearVelocity = Vector3.zero; GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
                    GetComponent<Rigidbody>().useGravity = false;
                    GetComponent<Rigidbody>().isKinematic = true;
                    try { _stuckVfx.SetActive(true); } catch { }
                }
            }



            _collided = true;
        }
    }





    //NOPE
    //private void OnTriggerEnter(Collider other)
    //{
    //    if (GetComponent<Collider>().isTrigger)
    //    {
    //        Debug.Log($"OnTriggerEnter with: {other.gameObject.name} {other.gameObject.layer} at point {other.ClosestPoint(transform.position)}");

    //        if (GameManager.LayerIsPartOfLayerMask(other.gameObject.layer, _stickyLayerMask))
    //        {
    //            _collided = true;
    //            try { GetComponent<AudioSource>().clip = _collisionSound; GetComponent<AudioSource>().Play(); } catch { }






    //            if (_sticky && !stuck)
    //            {
    //                if (other.GetComponent<PlayerHitbox>())
    //                {
    //                    NetworkGameManager.StickGrenadeOnPlayer(GrenadePool.instance.stickyGrenadePool.IndexOf(gameObject), other.GetComponent<PlayerHitbox>().player.hitboxes.IndexOf(other.GetComponent<PlayerHitbox>()), other.GetComponent<PlayerHitbox>().player.playerId, other.ClosestPoint(transform.position));
    //                }
    //                else
    //                {
    //                    transform.position = other.ClosestPoint(transform.position);
    //                    gameObject.transform.SetParent(other.gameObject.transform, true);

    //                    GetComponent<Rigidbody>().useGravity = false;
    //                    GetComponent<Rigidbody>().isKinematic = true;

    //                    if (other.gameObject.GetComponent<PlayerHitbox>())
    //                    {
    //                        Debug.Log("STUCK!");
    //                        _stuck = true;
    //                    }
    //                }
    //            }
    //        }
    //    }
    //}

    void Explosion()
    {
        if (!_exploded)
        {
            Debug.Log($"Is under water {IsUnderwater()}");
            Debug.Log($"Explosion {_stuckPlayerPhotonId} {PhotonNetwork.IsMasterClient == true} {_player.photonId} {_exploded == true}");
        }

        if (PhotonNetwork.IsMasterClient && !_exploded)
        {
            if (_stuckPlayerPhotonId > 0)
            {
                Debug.Log($"Explosion 1");

                //GameManager.GetPlayerWithPhotonView(_stuckPlayerPhotonId).Damage(damage: 999, headshot: false, source_pid: _player.photonId, impactPos: transform.position,
                //  impactDir: GameManager.GetPlayerWithPhotonView(_stuckPlayerPhotonId).targetTrackingCorrectTarget.position - transform.position, kfo: WeaponProperties.KillFeedOutput.Stuck);



                GameManager.GetPlayerWithPhotonView(_stuckPlayerPhotonId).PV.RPC("Damage_RPC", RpcTarget.All, 0, 999, _player.photonId, (int)DeathNature.Stuck, transform.position, GameManager.GetPlayerWithPhotonView(_stuckPlayerPhotonId).targetTrackingCorrectTarget.position - transform.position, -1, (int)WeaponProperties.KillFeedOutput.Stuck);


                StartCoroutine(ExplosionCoroutine());
            }
            else
            {
                Debug.Log($"Explosion 2");
                Debug.Log("Calling DisableAndExplodeProjectile");
                NetworkGameManager.instance.DisableAndExplodeProjectile((int)_killFeedOutput, GrenadePool.instance.GetIndexOfExplosive(_killFeedOutput, gameObject), transform.position, underWater: IsUnderwater());
            }
        }
        _exploded = true;
    }

    IEnumerator ExplosionCoroutine()
    {
        yield return new WaitForEndOfFrame();
        if (!_exploded)
            _exploded = true;
        Debug.Log("Calling DisableAndExplodeProjectile");
        NetworkGameManager.instance.DisableAndExplodeProjectile((int)_killFeedOutput, GrenadePool.instance.GetIndexOfExplosive(_killFeedOutput, gameObject), transform.position, underWater: IsUnderwater());
    }



    public void TriggerExplosion(Vector3 pos, bool underWater)
    {
        if (underWater)
            GrenadePool.SpawnExplosion(player, damage: _damage, radius: _radius, expPower: _explosionPower, damageCleanNameSource: _sourceCleanName, pos, col: global::Explosion.Color.Water, _type, GrenadePool.instance.underWaterClip, _killFeedOutput, stuck);
        else if (_color == global::Explosion.Color.Yellow)
            GrenadePool.SpawnExplosion(player, damage: _damage, radius: _radius, expPower: _explosionPower, damageCleanNameSource: _sourceCleanName, pos, _color, _type, GrenadePool.instance.fragClip, _killFeedOutput, stuck);
        else if (_color == global::Explosion.Color.Blue)
            GrenadePool.SpawnExplosion(player, damage: _damage, radius: _radius, expPower: _explosionPower, damageCleanNameSource: _sourceCleanName, pos, _color, _type, GrenadePool.instance.plasmaClip, _killFeedOutput, stuck);



        _resetIgnoredColliders = -1;
        gameObject.SetActive(false);
        transform.SetParent(GrenadePool.instance.transform);


        if (_collidersToIgnore.Count > 0)
            foreach (Collider collider in _collidersToIgnore)
                Physics.IgnoreCollision(GetComponent<Collider>(), collider, false);
    }





    public void TriggerStuckBehaviour(int playerPhotonId, Vector3 gPos)
    {
        Debug.Log($"TriggerStuckBehaviour. of photon id {playerPhotonId}. gPos {gPos}");
        GetComponent<Rigidbody>().linearVelocity = Vector3.zero; GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

        _explosionDelayOnImpact = _defaultExplosionDelayOnImpact;
        stuck = true;
        _collided = true;


        _stuckPlayerPhotonId = playerPhotonId;
        gameObject.transform.position = gPos;
        gameObject.transform.SetParent(GameManager.GetPlayerWithPhotonView(playerPhotonId).transform, true);

        //GetComponent<Rigidbody>().useGravity = false;
        //GetComponent<Rigidbody>().isKinematic = true;
        //GetComponent<Rigidbody>().detectCollisions = false;
        Destroy(GetComponent<Rigidbody>());

        GetComponent<Collider>().enabled = false;

        stuck = true;


        if (GameManager.instance.connection == GameManager.NetworkType.Local || GameManager.instance.nbLocalPlayersPreset > 1) _stuckSfxAudioSource.spatialBlend = 0; else _stuckSfxAudioSource.spatialBlend = 1;
        _stuckSfxAudioSource.Play();


        GameManager.GetPlayerWithPhotonView(playerPhotonId).PlayStuckClip();
    }




    List<Collider> _collidersToIgnore = new List<Collider>();
    public void IgnoreTheseCollidersFor1Second(List<Collider> collidersToIgnore)
    {
        _collidersToIgnore = collidersToIgnore;
        foreach (Collider collider in collidersToIgnore)
        {
            Physics.IgnoreCollision(GetComponent<Collider>(), collider);
        }

        _resetIgnoredColliders = 1;
    }


    private bool IsUnderwater()
    {
        if (!GameManager.LEVELS_WITH_WATER.Contains(SceneManager.GetActiveScene().buildIndex)) return false;

        RaycastHit[] hits;

        hits = Physics.RaycastAll(transform.position + (Vector3.up * 10), Vector3.down, 15);

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].collider.gameObject.layer == 4)
            {
                print($"ExplosiveProjectile IsUnderwater {hits[i].transform.position.y} vs {transform.position.y}");
                if ((hits[i].transform.position.y - transform.position.y > 0))
                    return true;
            }
        }

        return false;
    }
}

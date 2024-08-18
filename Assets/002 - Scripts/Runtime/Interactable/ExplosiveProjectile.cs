using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class ExplosiveProjectile : MonoBehaviour
{
    public Player player { get { return _player; } set { _player = value; } }
    public int throwForce { get { return _throwForce; } set { _throwForce = value; } }
    public bool useConstantForce { get { return _useConstantForce; } set { _useConstantForce = value; } }
    public GameObject model { get { return _model; } }
    public GameObject visualIndicator { get { return _visualIndicator; } }
    public GameObject visualIndicatorDuplicate { get { return _visualIndicatorDuplicate; } }
    public bool stuck { get { return _stuck; } set { _stuck = value; } }

    [SerializeField] Player _player;
    [SerializeField] int _damage, _radius, _throwForce, _explosionPower;
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
    float _explosionDelayOnImpact;





    private void OnEnable()
    {
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


            GetComponent<ParentConstraint>().constraintActive = false;
            GetComponent<ParentConstraint>().locked = false;

            for (int j = GetComponent<ParentConstraint>().sourceCount; j-- > 0;)
                GetComponent<ParentConstraint>().RemoveSource(j);
        }

        _ttl = _defaultTtl;

        _exploded = false;
        _collided = false; _explosionDelayOnImpact = _defaultExplosionDelayOnImpact;
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        gameObject.layer = 8;
    }





    // Update is called once per frame
    void Update()
    {
        if (_ttl > 0)
        {
            _ttl -= Time.deltaTime;

            if (_ttl <= 0)
            {
                print($"{name} disabled due to ttl");
                gameObject.SetActive(false);
            }
        }


        if (useConstantForce)
            GetComponent<Rigidbody>().AddForce
                (gameObject.transform.forward * throwForce);

        if (_collided)
        {
            _explosionDelayOnImpact -= Time.deltaTime;
            if (_explosionDelayOnImpact < 0)
                Explosion();
        }


    }

    private void OnCollisionEnter(Collision collision)
    {
        if (GetComponent<Collider>().isTrigger) return;

        if (collision.gameObject.layer != 9)
        {
            Debug.Log($"Collided with: {collision.gameObject.name} {collision.gameObject.GetComponent<Collider>()} {collision.gameObject.GetComponent<Player>()}");

            try { GetComponent<AudioSource>().clip = _collisionSound; GetComponent<AudioSource>().Play(); } catch { }

            if (_sticky && !_collided)
            {
                Debug.Log($"STUCK: {collision.gameObject.name} {collision.gameObject.GetComponent<PlayerHitbox>()} {collision.gameObject.GetComponent<ActorHitbox>()}");


                if (collision.gameObject.transform.root.GetComponent<Player>())
                {

                    print("Stuck 1");

                    GetComponent<ParentConstraint>().locked = true;
                    GetComponent<ParentConstraint>().constraintActive = false;


                    GetComponent<Rigidbody>().velocity = Vector3.zero; GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
                    if (player.isMine)
                        NetworkGameManager.StickGrenadeOnPlayer(GrenadePool.instance.stickyGrenadePool.IndexOf(gameObject), collision.gameObject.transform.root.GetComponent<Player>().playerId, collision.contacts[0].point);
                    _stuckVfx.SetActive(true);

                }
                else if (collision.gameObject.GetComponent<Rigidbody>())
                {
                    print("Stuck 2");
                    transform.parent = collision.transform;


                    GetComponent<Rigidbody>().velocity = Vector3.zero; GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
                    GetComponent<Rigidbody>().useGravity = false;
                    GetComponent<Rigidbody>().isKinematic = true;
                    try { _stuckVfx.SetActive(true); } catch { }
                }
                else
                {
                    //print($"STICK TO WALL {collision.gameObject.name} " +
                    //    $"{transform.position - collision.transform.position}   {collision.transform.InverseTransformDirection(transform.position - collision.transform.position)}");

                    //Vector3 vg = transform.position - collision.transform.position;
                    //Vector3 v = collision.transform.InverseTransformDirection(transform.position - collision.transform.position);
                    //print(v);

                    //if (Mathf.Sign(v.x) != Mathf.Sign(vg.x) && Mathf.Abs(v.x) == Mathf.Abs(vg.x))
                    //    v.x *= -1;

                    //if (Mathf.Sign(v.y) != Mathf.Sign(vg.y) && Mathf.Abs(v.y) == Mathf.Abs(vg.y))
                    //    v.y *= -1;

                    //if (Mathf.Sign(v.z) != Mathf.Sign(vg.z) && Mathf.Abs(v.z) == Mathf.Abs(vg.z))
                    //    v.z *= -1;

                    ////v.y *= Mathf.Sign(vg.y);
                    ////v.z *= Mathf.Sign(vg.z);
                    //print(v);

                    //ConstraintSource cs = new ConstraintSource(); cs.sourceTransform = collision.transform; cs.weight = 1;
                    //GetComponent<ParentConstraint>().AddSource(cs);
                    //GetComponent<ParentConstraint>().SetTranslationOffset(0, v);
                    //GetComponent<ParentConstraint>().SetRotationOffset(0, transform.rotation.eulerAngles);
                    //GetComponent<ParentConstraint>().locked = true;
                    //GetComponent<ParentConstraint>().constraintActive = true;


                    //_localDirToFakeParentOnColl = collision.transform.InverseTransformDirection(transform.position - collision.transform.position).normalized;
                    //_distOnColl = Vector3.Distance(transform.position, collision.transform.position);
                    //_fakeParent = collision.transform;


                    print("Stuck 3");


                    //transform.parent = collision.transform;
                    GetComponent<Rigidbody>().velocity = Vector3.zero; GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
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
        if (player.isMine && !_exploded)
        {
            _exploded = true;
            NetworkGameManager.instance.DisableAndExplodeProjectile((int)_killFeedOutput, GrenadePool.instance.GetIndexOfExplosive(_killFeedOutput, gameObject), transform.position);
        }










        //if (_color == global::Explosion.Color.Yellow)
        //    GrenadePool.SpawnExplosion(player, damage: _damage, radius: _radius, expPower: _explosionPower, damageCleanNameSource: _sourceCleanName, transform.position, _color, _type, GrenadePool.instance.fragClip, _killFeedOutput, stuck);
        //else if (_color == global::Explosion.Color.Blue)
        //    GrenadePool.SpawnExplosion(player, damage: _damage, radius: _radius, expPower: _explosionPower, damageCleanNameSource: _sourceCleanName, transform.position, _color, _type, GrenadePool.instance.plasmaClip, _killFeedOutput, stuck);



        //gameObject.SetActive(false);
    }

    public void TriggerExplosion(Vector3 pos)
    {
        if (_color == global::Explosion.Color.Yellow)
            GrenadePool.SpawnExplosion(player, damage: _damage, radius: _radius, expPower: _explosionPower, damageCleanNameSource: _sourceCleanName, pos, _color, _type, GrenadePool.instance.fragClip, _killFeedOutput, stuck);
        else if (_color == global::Explosion.Color.Blue)
            GrenadePool.SpawnExplosion(player, damage: _damage, radius: _radius, expPower: _explosionPower, damageCleanNameSource: _sourceCleanName, pos, _color, _type, GrenadePool.instance.plasmaClip, _killFeedOutput, stuck);



        gameObject.SetActive(false);
    }





    public void TriggerStuckBehaviour(int playerId, Vector3 gPos)
    {
        Debug.Log($"TriggerStuckBehaviour. of player {playerId}. gPos {gPos}");
        GetComponent<Rigidbody>().velocity = Vector3.zero; GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

        _explosionDelayOnImpact = _defaultExplosionDelayOnImpact;
        stuck = true;
        _collided = true;


        gameObject.transform.position = gPos;
        gameObject.transform.SetParent(GameManager.GetPlayerWithId(playerId).transform, true);

        GetComponent<Rigidbody>().useGravity = false;
        GetComponent<Rigidbody>().isKinematic = true;

        stuck = true;


        if (GameManager.instance.connection == GameManager.Connection.Local) _stuckSfxAudioSource.spatialBlend = 0; else _stuckSfxAudioSource.spatialBlend = 1;
        _stuckSfxAudioSource.Play();


        GameManager.GetPlayerWithId(playerId).PlayStuckClip();
    }
}

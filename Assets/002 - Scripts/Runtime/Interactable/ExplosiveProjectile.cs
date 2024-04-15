using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveProjectile : MonoBehaviour
{
    public Player player { get { return _player; } set { _player = value; } }
    public int force { get { return _force; } set { _force = value; } }
    public bool useConstantForce { get { return _useConstantForce; } set { _useConstantForce = value; } }
    public GameObject model { get { return _model; } }
    public GameObject visualIndicator { get { return _visualIndicator; } }
    public GameObject visualIndicatorDuplicate { get { return _visualIndicatorDuplicate; } }
    public bool stuck { get { return _stuck; } set { _stuck = value; } }

    [SerializeField] Player _player;
    [SerializeField] int _force;
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

    bool _collided;
    float _explosionDelayOnImpact;


    private void Awake()
    {
        if (_defaultTtl <= 0) { _defaultTtl = 10; }
    }

    private void OnEnable()
    {
        if (_defaultTtl <= 0) { _defaultTtl = 10; }


        _visualIndicator.SetActive(true);
        try { _stuckVfx.SetActive(false); } catch { }
        if (_sticky)
            foreach (Player p in GameManager.instance.pid_player_Dict.Values)
            {
                Physics.IgnoreCollision(p.playerCapsule.GetComponent<Collider>(), GetComponent<Collider>());
            }

        _ttl = _defaultTtl;

        _collided = false; _explosionDelayOnImpact = _defaultExplosionDelayOnImpact;
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        gameObject.layer = 8;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!player.PV.IsMine) _explosionDelayOnImpact *= 0.9f;
        try
        {
            foreach (PlayerHitbox ph in player.GetComponent<PlayerHitboxes>().hitboxes)
                Physics.IgnoreCollision(GetComponent<Collider>(), ph.GetComponent<Collider>());
        }
        catch { }

        if (!useConstantForce)
            GetComponent<Rigidbody>().AddForce(gameObject.transform.forward * force);
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
                (gameObject.transform.forward * force);

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
                if (collision.gameObject.transform.root.GetComponent<Player>())
                {
                    GetComponent<Rigidbody>().velocity = Vector3.zero; GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
                    if (player.isMine)
                        NetworkGameManager.StickGrenadeOnPlayer(GrenadePool.instance.stickyGrenadePool.IndexOf(gameObject), collision.gameObject.transform.root.GetComponent<Player>().playerId, collision.contacts[0].point);
                    _stuckVfx.SetActive(true);

                }
                else
                {
                    gameObject.transform.parent = collision.gameObject.transform;
                    GetComponent<Rigidbody>().useGravity = false;
                    GetComponent<Rigidbody>().isKinematic = true;
                    try { _stuckVfx.SetActive(true); } catch { }
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
        Explosion e = Instantiate(explosionPrefab, transform.position, transform.rotation).GetComponent<Explosion>();
        if (_sticky) { transform.parent = GrenadePool.instance.transform; }
        e.player = player;
        e.stuck = _stuck;
        e.DisableIn5Seconds();
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
    }
}

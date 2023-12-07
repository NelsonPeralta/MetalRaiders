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
    public Collider collid { get { return _collider; } }

    [SerializeField] Player _player;
    [SerializeField] int _damage;
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
    [SerializeField] GameObject _explosionFx;
    [SerializeField] Collider _collider;

    bool _collided;
    float _explosionDelayOnImpact;




    private void OnEnable()
    {
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
                Physics.IgnoreCollision(collid, ph.GetComponent<Collider>());
        }
        catch { }

        if (!useConstantForce)
            GetComponent<Rigidbody>().AddForce(gameObject.transform.forward * force);
    }

    // Update is called once per frame
    void Update()
    {
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
        if (collid.isTrigger) return;

        if (collision.gameObject.layer != 9)
        {
            Debug.Log($"Collided with: {collision.gameObject.name} {collision.gameObject.GetComponent<Collider>()}");
            _collided = true;
            try { GetComponent<AudioSource>().clip = _collisionSound; GetComponent<AudioSource>().Play(); } catch { }

            if (_sticky)
            {
                _sticky = false;
                {
                    if (_stickyLayerMask == (_stickyLayerMask | (1 << collision.gameObject.layer)))
                    {
                        Debug.Log($"Collided with: {collision.gameObject.name} {collision.gameObject.layer}");
                        gameObject.transform.parent = collision.gameObject.transform;

                        GetComponent<Rigidbody>().useGravity = false;
                        GetComponent<Rigidbody>().isKinematic = true;

                        if (collision.gameObject.GetComponent<PlayerHitbox>())
                        {
                            Debug.Log("STUCK!");
                            _stuck = true;
                        }
                    }
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collid.isTrigger)
        {
            Debug.Log($"Collided with: {other.gameObject.name} {other.gameObject.layer}");

            if (GameManager.LayerIsPartOfLayerMask(other.gameObject.layer, _stickyLayerMask))
            {
                _collided = true;
                try { GetComponent<AudioSource>().clip = _collisionSound; GetComponent<AudioSource>().Play(); } catch { }

                if (_sticky)
                {
                    if (other.GetComponent<Hitbox>())
                        Debug.Log($" blabla {other.GetComponent<Hitbox>().hitboxesScript.hitboxes.Contains(other.GetComponent<Hitbox>())} {other.GetComponent<Hitbox>().hitboxesScript.hitboxes.IndexOf(other.GetComponent<Hitbox>())}");

                    _sticky = false;
                    {
                        gameObject.transform.SetParent(other.gameObject.transform, true);

                        GetComponent<Rigidbody>().useGravity = false;
                        GetComponent<Rigidbody>().isKinematic = true;

                        if (other.gameObject.GetComponent<PlayerHitbox>())
                        {
                            Debug.Log("STUCK!");
                            visualIndicator.transform.localScale *= 3;
                            _stuck = true;
                        }
                    }
                }
            }
        }
    }

    void Explosion()
    {
        GameObject ex = GameObjectPool.instance.SpawnExplosion();
        ex.SetActive(true); ex.transform.position = transform.position;
        ex.GetComponent<Explosion>().player = player;
        ex.GetComponent<Explosion>().damage = _damage;
        ex.GetComponent<Explosion>().stuck = _stuck;


        //Transform e = Instantiate(explosionPrefab, transform.position + new Vector3(0, 1, 0), transform.rotation);
        gameObject.SetActive(false);
    }
}

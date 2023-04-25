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

    [SerializeField] Player _player;
    [SerializeField] int _force;
    [SerializeField] bool _useConstantForce;
    [SerializeField] float _explosionDelayOnImpact;
    [SerializeField] Transform explosionPrefab;
    [SerializeField] AudioClip _collisionSound;
    [SerializeField] bool _sticky;
    [SerializeField] bool _stuck;
    [SerializeField] LayerMask _stickyLayerMask;
    [SerializeField] GameObject _model;
    [SerializeField] GameObject _visualIndicator;
    [SerializeField] GameObject _visualIndicatorDuplicate;

    bool _collided;

    // Start is called before the first frame update
    void Start()
    {
        foreach (PlayerHitbox ph in player.GetComponent<PlayerHitboxes>().playerHitboxes)
            Physics.IgnoreCollision(GetComponent<Collider>(), ph.GetComponent<Collider>());

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
        if (collision.gameObject.layer != 9)
        {
            _collided = true;
            try { GetComponent<AudioSource>().clip = _collisionSound; GetComponent<AudioSource>().Play(); } catch { }

            if (_sticky)
            {
                _sticky = false;
                {
                    if (_stickyLayerMask == (_stickyLayerMask | (1 << collision.gameObject.layer)))
                    {
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

    void Explosion()
    {
        Transform e = Instantiate(explosionPrefab, transform.position + new Vector3(0, 1, 0), transform.rotation);
        e.GetComponent<Explosion>().player = player;
        e.GetComponent<Explosion>().stuck = _stuck;
        gameObject.SetActive(false);
    }
}

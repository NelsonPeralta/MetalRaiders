using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Rewired.ComponentControls.Effects.RotateAroundAxis;

public class ActorAddonsPool : MonoBehaviour
{
    public static ActorAddonsPool instance;





    [SerializeField] GameObject _fireballPrefab, _grenadePrefab, _grenadeExplosionVfx;





    List<Fireball> _fireballPool = new List<Fireball>();
    List<AIGrenade> _aIGrenades = new List<AIGrenade>();
    List<GameObject> _explosionVfx = new List<GameObject>();






    void Awake()
    {
        instance = this;
    }


    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 100; i++)
        {
            GameObject obj = Instantiate(_fireballPrefab, transform.position, transform.rotation);
            obj.SetActive(false);
            _fireballPool.Add(obj.GetComponent<Fireball>());
            obj.transform.parent = transform;

        }

        for (int i = 0; i < 20; i++)
        {
            GameObject obj = Instantiate(_grenadePrefab, transform.position, transform.rotation);
            obj.SetActive(false);
            _aIGrenades.Add(obj.GetComponent<AIGrenade>());
            obj.transform.parent = transform;


            obj = Instantiate(_grenadeExplosionVfx, transform.position, transform.rotation);
            obj.SetActive(false);
            _explosionVfx.Add(obj);
            obj.transform.parent = transform;
        }
    }



    public void SpawnPooledFireball(Vector3 sp, Vector3 sd, GameObject sourceBiped, int _damage, int _speed, int _radius, int _power)
    {
        foreach (Fireball fb in _fireballPool)
            if (!fb.gameObject.activeSelf)
            {
                fb.damage = _damage;
                fb.speed = _speed;
                fb.radius = _radius;
                fb.power = _power;
                fb.deSpawnTime = 10;
                fb.sourceBiped = sourceBiped;


                fb.transform.position = sp;
                fb.transform.rotation = Quaternion.LookRotation((Vector3)sd);
                fb.gameObject.SetActive(true);


                break;
            }
    }


    public void SpawnPooledGrenade(Vector3 sp, Vector3 sd, GameObject sourceBiped, List<ActorHitbox> _ignoreHb, int _damage, int _radius, int _throwForce)
    {
        foreach (AIGrenade fb in _aIGrenades)
            if (!fb.gameObject.activeSelf)
            {
                foreach (ActorHitbox c in _ignoreHb)
                    Physics.IgnoreCollision(fb.GetComponent<Collider>(), c.GetComponent<Collider>());

                fb.hasHitObject = false;

                fb.damage = _damage;
                fb.radius = _radius;
                fb.throwForce = _throwForce;
                fb.sourceBiped = sourceBiped;


                fb.transform.position = sp;
                fb.transform.rotation = Quaternion.LookRotation((Vector3)sd);
                fb.gameObject.SetActive(true);


                fb.GetComponent<Rigidbody>().AddForce(fb.transform.forward * _throwForce);

                break;
            }
    }


    public void SpawnGrenadeVfx(Vector3 _pos)
    {
        foreach (GameObject fb in _explosionVfx)
            if (!fb.gameObject.activeSelf)
            {
                fb.transform.position = _pos;
                fb.SetActive(true);
                StartCoroutine(GameObjectPool.instance.DisableObjectAfterTime(fb, 5));

                break;
            }
    }
}

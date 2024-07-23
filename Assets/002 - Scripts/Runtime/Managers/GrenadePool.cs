using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadePool : MonoBehaviour
{
    public static GrenadePool instance { get { return _instance; } }

    public List<GameObject> stickyGrenadePool { get { return _stickyGrenadePool; } }

    [SerializeField] GameObject _fragGrenadePrefab, _stickyGrenadePrefab, _explosionPrefab;
    [SerializeField] List<GameObject> _fragGrenadePool = new List<GameObject>();
    [SerializeField] List<GameObject> _stickyGrenadePool = new List<GameObject>();
    [SerializeField] List<Explosion> _explosions = new List<Explosion>();

    public AudioClip fragClip, plasmaClip, barrelClip, ultraBindClip;


    static GrenadePool _instance;

    private void Awake()
    {
        _instance = this;




        for (int i = 0; i < 200; i++)
        {
            _fragGrenadePool.Add(Instantiate(_fragGrenadePrefab, transform));
            _stickyGrenadePool.Add(Instantiate(_stickyGrenadePrefab, transform));
            _explosions.Add(Instantiate(_explosionPrefab, transform).GetComponent<Explosion>()); // Prefab must be inactive

            _fragGrenadePool[i].SetActive(false); _stickyGrenadePool[i].SetActive(false);
            _fragGrenadePool[i].transform.SetParent(this.transform); _stickyGrenadePool[i].transform.SetParent(this.transform);
            _explosions[i].transform.SetParent(this.transform);

            _explosions[i].name += $"{Random.Range(1, 99999)}";
        }
    }


    public static int GetAvailableGrenadeIndex(bool isFrag, int photonRoomIndex)
    {
        print($"GetAvailableGrenadeIndex {photonRoomIndex}");

        for (int i = (photonRoomIndex - 1) * 10; i < (photonRoomIndex * 10) - 1; i++)
        {
            print(i);
            print(instance.name);
            print(_instance._fragGrenadePool[i].name);
            print("stop");


            if (!_instance._fragGrenadePool[i].activeInHierarchy) _instance._fragGrenadePool[i].transform.SetParent(instance.transform);
            if (!_instance._stickyGrenadePool[i].activeInHierarchy)
            {
                _instance._stickyGrenadePool[i].GetComponent<Rigidbody>().isKinematic = false;
                _instance._stickyGrenadePool[i].GetComponent<Rigidbody>().useGravity = true;
                _instance._stickyGrenadePool[i].GetComponent<ExplosiveProjectile>().stuck = false;
                _instance._stickyGrenadePool[i].transform.SetParent(instance.transform);

                _instance._stickyGrenadePool[i].transform.localScale = Vector3.one;

            }

            if (isFrag)
            {
                if (!_instance._fragGrenadePool[i].activeInHierarchy) return i;
            }
            else
                if (!_instance._stickyGrenadePool[i].activeInHierarchy) return i;
        }

        return -1;
    }

    public static GameObject GetGrenade(bool isFrag, int index)
    {
        if (isFrag) return _instance._fragGrenadePool[index];
        else return _instance._stickyGrenadePool[index];
    }


    public static void SpawnExplosion(Player source, int damage, int radius, int expPower, string damageCleanNameSource,
        Vector3 pos, Explosion.Color col, Explosion.Type t, AudioClip ac, WeaponProperties.KillFeedOutput kfo, bool stuck = false)
    {
        foreach (Explosion obj in instance._explosions)
            if (!obj.gameObject.activeInHierarchy)
            {
                obj.GetComponent<AudioSource>().clip = ac;
                obj.player = source;
                obj.killFeedOutput = kfo;
                obj.damage = damage;
                obj.radius = radius;
                obj.explosionPower = expPower;
                obj.transform.position = pos;
                obj.stuck = stuck;
                obj.color = col;
                obj.damageSource = damageCleanNameSource;
                obj.type = t;
                obj.gameObject.SetActive(true);
                obj.Explode();
                obj.DisableIn3Seconds();
                break;
            }
    }
}

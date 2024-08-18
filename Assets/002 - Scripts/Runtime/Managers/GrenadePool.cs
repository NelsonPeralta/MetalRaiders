using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadePool : MonoBehaviour
{
    public static GrenadePool instance { get { return _instance; } }

    public List<GameObject> stickyGrenadePool { get { return _stickyGrenadePool; } }

    [SerializeField] GameObject _fragGrenadePrefab, _stickyGrenadePrefab, _rocketPrefab, _glProjectilePrefab, _explosionPrefab;
    [SerializeField] List<GameObject> _fragGrenadePool = new List<GameObject>();
    [SerializeField] List<GameObject> _stickyGrenadePool = new List<GameObject>();
    [SerializeField] List<ExplosiveProjectile> _rocketPool = new List<ExplosiveProjectile>();
    [SerializeField] List<ExplosiveProjectile> _glProjectilePool = new List<ExplosiveProjectile>();
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

            //_explosions[i].name += $"{Random.Range(1, 99999)}";
        }


        for (int i = 0; i < CurrentRoomManager.instance.expectedNbPlayers * 5; i++)
        {
            _rocketPool.Add(Instantiate(_rocketPrefab, transform).GetComponent<ExplosiveProjectile>());

            _rocketPool[i].gameObject.SetActive(false);
            _rocketPool[i].transform.SetParent(this.transform);
        }

        for (int i = 0; i < CurrentRoomManager.instance.expectedNbPlayers * 12; i++)
        {
            _glProjectilePool.Add(Instantiate(_glProjectilePrefab, transform).GetComponent<ExplosiveProjectile>());

            _glProjectilePool[i].gameObject.SetActive(false);
            _glProjectilePool[i].transform.SetParent(this.transform);
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



    public static int GetAvailableRocketAtIndex(int photonRoomIndex)
    {
        for (int i = (photonRoomIndex - 1) * 5; i < (photonRoomIndex * 5) - 1; i++)
        {

            if (!_instance._rocketPool[i].gameObject.activeInHierarchy) _instance._rocketPool[i].transform.SetParent(instance.transform);
            if (!_instance._rocketPool[i].gameObject.activeInHierarchy) return i;
        }

        return -1;
    }

    public static int GetAvailableGrenadeLauncherProjectileAtIndex(int photonRoomIndex)
    {
        for (int i = (photonRoomIndex - 1) * 12; i < (photonRoomIndex * 12) - 1; i++)
        {

            if (!_instance._glProjectilePool[i].gameObject.activeInHierarchy) _instance._glProjectilePool[i].transform.SetParent(instance.transform);
            if (!_instance._glProjectilePool[i].gameObject.activeInHierarchy) { print($"GetAvailableGrenadeLauncherProjectileAtIndex: {i}"); return i; }
        }

        return -1;
    }









    public static GameObject GetGrenade(bool isFrag, int index)
    {
        if (isFrag) return _instance._fragGrenadePool[index];
        else return _instance._stickyGrenadePool[index];
    }

    public static void SpawnRocket(Player p, int index, Vector3 pos, Vector3 rot)
    {
        _instance._rocketPool[index].player = p;

        Physics.IgnoreCollision(_instance._rocketPool[index].GetComponent<Collider>(), p.playerCapsule.GetComponent<Collider>());
        foreach (PlayerHitbox hb in p.hitboxes)
            Physics.IgnoreCollision(_instance._rocketPool[index].GetComponent<Collider>(), hb.GetComponent<Collider>()); // Prevents the grenade from colliding with the player who threw it


        if (p.PV.IsMine)
            _instance._rocketPool[index].gameObject.layer = 8;
        else
            _instance._rocketPool[index].gameObject.layer = 0;

        _instance._rocketPool[index].transform.position = pos;
        _instance._rocketPool[index].transform.rotation = Quaternion.Euler(rot);

        _instance._rocketPool[index].GetComponent<Rigidbody>().velocity = Vector3.zero; _instance._rocketPool[index].GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        _instance._rocketPool[index].gameObject.SetActive(true);

        if (!_instance._rocketPool[index].useConstantForce)
            _instance._rocketPool[index].GetComponent<Rigidbody>().AddForce(_instance._rocketPool[index].gameObject.transform.forward * _instance._rocketPool[index].throwForce);
    }

    public static void SpawnGrenadeLauncherProjectile(Player p, int index, Vector3 pos, Vector3 rot)
    {
        _instance._glProjectilePool[index].player = p;

        Physics.IgnoreCollision(_instance._glProjectilePool[index].GetComponent<Collider>(), p.playerCapsule.GetComponent<Collider>());
        foreach (PlayerHitbox hb in p.hitboxes)
            Physics.IgnoreCollision(_instance._glProjectilePool[index].GetComponent<Collider>(), hb.GetComponent<Collider>()); // Prevents the grenade from colliding with the player who threw it


        if (p.PV.IsMine)
            _instance._glProjectilePool[index].gameObject.layer = 8;
        else
            _instance._glProjectilePool[index].gameObject.layer = 0;

        _instance._glProjectilePool[index].transform.position = pos;
        _instance._glProjectilePool[index].transform.rotation = Quaternion.Euler(rot);
        _instance._rocketPool[index].GetComponent<Rigidbody>().velocity = Vector3.zero; _instance._glProjectilePool[index].GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        _instance._glProjectilePool[index].gameObject.SetActive(true);
        if (!_instance._glProjectilePool[index].useConstantForce)
            _instance._glProjectilePool[index].GetComponent<Rigidbody>().AddForce(_instance._glProjectilePool[index].gameObject.transform.forward * _instance._glProjectilePool[index].throwForce);
    }







    public int GetIndexOfExplosive(WeaponProperties.KillFeedOutput kfo, GameObject go)
    {
        if (kfo == WeaponProperties.KillFeedOutput.Frag_Grenade)
        {
            return _fragGrenadePool.IndexOf(go);
        }
        else if (kfo == WeaponProperties.KillFeedOutput.Plasma_Grenade)
        {
            return _stickyGrenadePool.IndexOf(go);
        }
        else if (kfo == WeaponProperties.KillFeedOutput.RPG)
        {
            return _rocketPool.IndexOf(go.GetComponent<ExplosiveProjectile>());
        }
        else if (kfo == WeaponProperties.KillFeedOutput.Grenade_Launcher)
        {
            return _glProjectilePool.IndexOf(go.GetComponent<ExplosiveProjectile>());
        }



        return -1;
    }



    public void DisableExplosive(WeaponProperties.KillFeedOutput kfo, int ind, Vector3 pos)
    {
        if (kfo == WeaponProperties.KillFeedOutput.Frag_Grenade)
        {
            _fragGrenadePool[ind].GetComponent<ExplosiveProjectile>().TriggerExplosion(pos);
        }
        else if (kfo == WeaponProperties.KillFeedOutput.Plasma_Grenade)
        {
            _stickyGrenadePool[ind].GetComponent<ExplosiveProjectile>().TriggerExplosion(pos);
        }
        else if (kfo == WeaponProperties.KillFeedOutput.RPG)
        {
            _rocketPool[ind].GetComponent<ExplosiveProjectile>().TriggerExplosion(pos);
        }
        else if (kfo == WeaponProperties.KillFeedOutput.Grenade_Launcher)
        {
            _glProjectilePool[ind].GetComponent<ExplosiveProjectile>().TriggerExplosion(pos);
        }
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

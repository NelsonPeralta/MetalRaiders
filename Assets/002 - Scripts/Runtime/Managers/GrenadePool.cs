using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;

public class GrenadePool : MonoBehaviour
{
    public static GrenadePool instance { get { return _instance; } }

    static int GRENADE_INC = 15;
    static int ROCKET_INC = 10;
    static int GL_INC = 24;

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




        for (int i = 0; i < CurrentRoomManager.instance.expectedNbPlayers * GRENADE_INC; i++)
        {
            _fragGrenadePool.Add(Instantiate(_fragGrenadePrefab, transform));
            _stickyGrenadePool.Add(Instantiate(_stickyGrenadePrefab, transform));
            _explosions.Add(Instantiate(_explosionPrefab, transform).GetComponent<Explosion>()); // Prefab must be inactive

            _fragGrenadePool[i].SetActive(false); _stickyGrenadePool[i].SetActive(false);
            _fragGrenadePool[i].transform.SetParent(this.transform); _stickyGrenadePool[i].transform.SetParent(this.transform);
            _explosions[i].transform.SetParent(this.transform);

            _fragGrenadePool[i].name += $" ({i})";
            _stickyGrenadePool[i].name += $" ({i})";
            _explosions[i].name += $" ({i})";
        }


        for (int i = 0; i < CurrentRoomManager.instance.expectedNbPlayers * ROCKET_INC; i++)
        {
            _rocketPool.Add(Instantiate(_rocketPrefab, transform).GetComponent<ExplosiveProjectile>());

            _rocketPool[i].gameObject.SetActive(false);
            _rocketPool[i].transform.SetParent(this.transform);
            _rocketPool[i].name += $" ({i})";
        }

        for (int i = 0; i < CurrentRoomManager.instance.expectedNbPlayers * GL_INC; i++)
        {
            _glProjectilePool.Add(Instantiate(_glProjectilePrefab, transform).GetComponent<ExplosiveProjectile>());

            _glProjectilePool[i].gameObject.SetActive(false);
            _glProjectilePool[i].transform.SetParent(this.transform);
            _glProjectilePool[i].name += $" ({i})";
        }
    }












    public static int GetAvailableGrenadeIndex(bool isFrag, int photonRoomIndex) // this is called localy only
    {
        Log.Print($"GetAvailableGrenadeIndex {photonRoomIndex}");

        for (int i = (photonRoomIndex - 1) * GRENADE_INC; i < (photonRoomIndex * GRENADE_INC) - 1; i++)
        {
            Log.Print($"GetAvailableGrenadeIndex {i}");
            Log.Print($"GetAvailableGrenadeIndex {photonRoomIndex}   {i}   {_instance._fragGrenadePool[i].name}");

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
        for (int i = (photonRoomIndex - 1) * ROCKET_INC; i < (photonRoomIndex * ROCKET_INC) - 1; i++)
        {

            if (!_instance._rocketPool[i].gameObject.activeInHierarchy) _instance._rocketPool[i].transform.SetParent(instance.transform);
            if (!_instance._rocketPool[i].gameObject.activeInHierarchy) return i;
        }

        return -1;
    }

    public static int GetAvailableGrenadeLauncherProjectileAtIndex(int photonRoomIndex)
    {
        for (int i = (photonRoomIndex - 1) * GL_INC; i < (photonRoomIndex * GL_INC) - 1; i++)
        {

            if (!_instance._glProjectilePool[i].gameObject.activeInHierarchy) _instance._glProjectilePool[i].transform.SetParent(instance.transform);
            if (!_instance._glProjectilePool[i].gameObject.activeInHierarchy) { Log.Print($"GetAvailableGrenadeLauncherProjectileAtIndex: {i}"); return i; }
        }

        return -1;
    }









    public static GameObject GetGrenadeAtIndex(bool isFrag, int i)
    {
        if (isFrag)
        {
            _instance._fragGrenadePool[i].GetComponent<Rigidbody>().mass = 1;
            return _instance._fragGrenadePool[i];
        }
        else
        {
            return _instance._stickyGrenadePool[i];
        }
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

        _instance._rocketPool[index].GetComponent<Rigidbody>().linearVelocity = Vector3.zero; _instance._rocketPool[index].GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        _instance._rocketPool[index].gameObject.SetActive(true);

        if (!_instance._rocketPool[index].useConstantForce)
            _instance._rocketPool[index].GetComponent<Rigidbody>().AddForce(_instance._rocketPool[index].gameObject.transform.forward * WeaponProperties.ROCKET_LAUNCHER_CONTINUOUS_FORCE);
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
        _instance._glProjectilePool[index].GetComponent<Rigidbody>().linearVelocity = Vector3.zero; _instance._glProjectilePool[index].GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        _instance._glProjectilePool[index].gameObject.SetActive(true);
        if (!_instance._glProjectilePool[index].useConstantForce)
            _instance._glProjectilePool[index].GetComponent<Rigidbody>().AddForce(_instance._glProjectilePool[index].gameObject.transform.forward * WeaponProperties.ROCKET_LAUNCHER_CONTINUOUS_FORCE);
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
        Log.Print($"DisableExplosive {PhotonNetwork.IsMasterClient} {kfo} {ind}");
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
        {
            Debug.Log($"SpawnExplosion. KFO: {obj.gameObject.activeInHierarchy} {obj.gameObject.activeSelf}");

            if (!obj.gameObject.activeInHierarchy)
            {
                Debug.Log($"SpawnExplosion. KFO: {(WeaponProperties.KillFeedOutput)kfo} damage: {damage} stuck: {stuck}");
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


    public void ResetAllEnabledObjects()
    {
        foreach (GameObject a in _fragGrenadePool.Where(item => item.activeInHierarchy)) a.SetActive(false);
        foreach (GameObject b in _stickyGrenadePool.Where(item => item.activeInHierarchy)) b.SetActive(false);
        foreach (ExplosiveProjectile c in _rocketPool.Where(item => item.gameObject.activeInHierarchy)) c.gameObject.SetActive(false);
        foreach (ExplosiveProjectile d in _glProjectilePool.Where(item => item.gameObject.activeInHierarchy)) d.gameObject.SetActive(false);
        foreach (Explosion d in _explosions.Where(item => item.gameObject.activeInHierarchy)) d.gameObject.SetActive(false);
    }
}

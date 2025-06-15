using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Runtime.CompilerServices;
using Steamworks;

public class ExplosiveBarrel : MonoBehaviour, IDamageable
{
    public delegate void ExplosiveBarrelEvent(ExplosiveBarrel explosiveBarrel);
    ExplosiveBarrelEvent OnExploded;

    public int index { get { return _index; } }
    public int _networkHitPoints { get { return _hitPoints; } set { _hitPoints = value; if (_hitPoints <= 0) OnExploded?.Invoke(this); } }
    public int hitPoints { get { return _networkHitPoints; } set { NetworkGameManager.instance.DamageExplosiveBarrel(spawnPointPosition, value, _lastPID); } }
    public Vector3 spawnPointPosition { get { return _spawnPointPosition; } }
    public Quaternion spawnPointRotation { get { return _spawnPointRotation; } }
    public int lastPid { get { return _lastPID; } }


    [SerializeField] int _index;
    [SerializeField] GameObject explosionPrefab;
    [SerializeField] int _defaultHitPoints;
    [SerializeField] int _hitPoints;
    [SerializeField] WeaponProperties.KillFeedOutput _killFeedOutput;
    [SerializeField] AudioClip _collisionAudioClip;
    [SerializeField] Vector3 _spawnPointPosition;
    [SerializeField] Quaternion _spawnPointRotation;
    [SerializeField] int _lastPID;

    private void Awake()
    {
        if(!GameManager.instance)Destroy(gameObject);
    }
    private void OnEnable()
    {
        GetComponent<Rigidbody>().linearVelocity *= 0;
        _hitPoints = _defaultHitPoints;
    }

    private void Start()
    {
        _spawnPointPosition = new Vector3((float)System.Math.Round(transform.position.x, 1), (float)System.Math.Round(transform.position.y, 1), (float)System.Math.Round(transform.position.z, 1));
        _spawnPointRotation = transform.rotation;
        OnExploded += OnExplode_Delegate;

        int i = 0;
        foreach (ExplosiveBarrel eb in FindObjectsOfType<ExplosiveBarrel>())
        {
            if (eb == this)
                _index = i;
            i++;
        }

        CurrentRoomManager.instance.AddSpawnedMappAddOn(transform);
    }

    private void OnCollisionEnter(Collision collision)
    {
        try
        {
            GetComponent<AudioSource>().clip = _collisionAudioClip;
            GetComponent<AudioSource>().Play();
        }
        catch { }
    }

    // Damage
    #region
    public void Damage(int damage)
    {
        print("ExplosiveBarrel Damage");
        hitPoints -= damage;
    }

    public void Damage(int damage, bool headshot, int playerWhoShotThisPlayerPhotonId)
    {
        print($"ExplosiveBarrel Damage {playerWhoShotThisPlayerPhotonId}");
        _lastPID = playerWhoShotThisPlayerPhotonId;
        hitPoints -= damage;
    }

    public void Damage(int damage, bool headshot, int playerWhoShotThisPlayerPhotonId,
        Vector3? impactPos = null, Vector3? impactDir = null, string damageSource = null, 
        bool isGroin = false, int weaponIndx = -1, WeaponProperties.KillFeedOutput kfo = WeaponProperties.KillFeedOutput.Unassigned,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        _lastPID = playerWhoShotThisPlayerPhotonId;
        print($"ExplosiveBarrel Damage {playerWhoShotThisPlayerPhotonId}");
        hitPoints -= damage;
    }
    #endregion

    // Delegates
    #region
    void OnExplode_Delegate(ExplosiveBarrel explosiveBarrel)
    {
        Debug.Log("EXPLOSIVE BARREL OnExplode_Delegate");

        transform.parent.GetComponent<ExplosiveBarrelSpawnPoint>().TriggerExplosionCoroutine();

        //GameObject e = Instantiate(explosionPrefab, transform.position + new Vector3(0, 1, 0), transform.rotation);
        //{
        //    e.GetComponent<Explosion>().damageSource = "Barrel";
        //    e.GetComponent<Explosion>().player = GameManager.GetPlayerWithPhotonViewId(_lastPID);
        //}
        //gameObject.SetActive(false);
    }
    #endregion

    // RPCs
    #region
    [PunRPC]
    void UpdateHitPoints(int h)
    {
        _networkHitPoints = h;
    }
    #endregion

    public void UpdateLastPlayerWhoDamaged(int playerPid)
    {
        _lastPID = playerPid;
    }
}

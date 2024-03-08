using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Runtime.CompilerServices;

public class IceChunk : Hazard, IDamageable
{
    public delegate void IceChunkBarrelEvent(IceChunk explosiveBarrel);
    IceChunkBarrelEvent OnExploded;

    [SerializeField] GameObject _model, _explosionModel;

    [SerializeField] int _defaultHitPoints;

    [SerializeField] int _hitPoints;
    public int networkHitPoints
    {
        get { return _hitPoints; }
        set
        {
            Debug.Log($"IceChunk {transform.position} networkHitPoints change to : {value}");
            _hitPoints = value; if (_hitPoints <= 0) OnExploded?.Invoke(this);
        }
    }
    public int hitPoints
    {
        get { return networkHitPoints; }
        set
        {
            Debug.Log($"IceChunk {transform.position} hp change to : {value}");
            NetworkGameManager.instance.DamageIceChunk(transform.position, value);
        }
    }


    private void OnEnable()
    {
        _hitPoints = _defaultHitPoints;
    }

    private void Start()
    {
        OnExploded += OnExplode_Delegate;
        CurrentRoomManager.instance.spawnedMapAddOns++;
        GameManager.instance.hazards.Add(this);
    }

    // Damage
    #region
    public void Damage(int damage)
    {
        hitPoints -= damage;
    }

    public void Damage(int healthDamage, bool headshot, int playerWhoShotThisPlayerPhotonId)
    {
        Damage(healthDamage, headshot, playerWhoShotThisPlayerPhotonId, damageSource: "");
    }

    public void Damage(int damage, bool headshot, int playerWhoShotThisPlayerPhotonId,
        Vector3? impactPos = null, Vector3? impactDir = null, string damageSource = null, bool isGroin = false,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        Debug.Log("ice chunk " + damage);
        hitPoints -= damage;
    }
    #endregion

    // Delegates
    #region
    void OnExplode_Delegate(IceChunk explosiveBarrel)
    {

        GetComponent<MeshRenderer>().enabled = false; GetComponent<Collider>().enabled = false;
        //_model.gameObject.SetActive(false);
        _explosionModel.gameObject.SetActive(true);

        StartCoroutine(Reset_Coroutine());

        //GameObject e = Instantiate(explosionPrefab, transform.position + new Vector3(0, 1, 0), transform.rotation);
        //Destroy(gameObject);
    }
    #endregion



    // Coroutines
    #region

    IEnumerator Reset_Coroutine()
    {
        yield return new WaitForSeconds(30);

        //_model.gameObject.SetActive(true);
        GetComponent<MeshRenderer>().enabled = true; GetComponent<Collider>().enabled = true;
        _explosionModel.gameObject.SetActive(false);
        _hitPoints = _defaultHitPoints;
    }

    #endregion

    // RPCs
    #region
    [PunRPC]
    void UpdateHitPoints(int h)
    {
        networkHitPoints = h;
    }
    #endregion
}

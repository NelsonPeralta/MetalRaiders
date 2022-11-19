using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class IceChunk : MonoBehaviour, IDamageable
{
    public delegate void IceChunkBarrelEvent(IceChunk explosiveBarrel);
    IceChunkBarrelEvent OnExploded;

    [SerializeField] int _defaultHitPoints;

    [SerializeField] int _hitPoints;
    public int _networkHitPoints { get { return _hitPoints; } set { _hitPoints = value; if (_hitPoints <= 0) OnExploded?.Invoke(this); } }
    public int hitPoints
    {
        get { return _networkHitPoints; }
        set
        {
            NetworkGameManager.instance.DamageIceChunk(transform.position, value);
        }
    }

    public GameObject explosionPrefab;

    private void OnEnable()
    {
        _hitPoints = _defaultHitPoints;
    }

    private void Start()
    {
        OnExploded += OnExplode_Delegate;
    }

    // Damage
    #region
    public void Damage(int damage)
    {
        hitPoints -= damage;
    }

    public void Damage(int healthDamage, bool headshot, int playerWhoShotThisPlayerPhotonId)
    {
        throw new System.NotImplementedException();
    }

    public void Damage(int damage, bool headshot, int playerWhoShotThisPlayerPhotonId, Vector3? impactPos = null, Vector3? impactDir = null, string damageSource = null, bool isGroin = false)
    {
        hitPoints -= damage;
    }
    #endregion

    // Delegates
    #region
    void OnExplode_Delegate(IceChunk explosiveBarrel)
    {
        GameObject e = Instantiate(explosionPrefab, transform.position + new Vector3(0, 1, 0), transform.rotation);

        Destroy(gameObject);
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
}

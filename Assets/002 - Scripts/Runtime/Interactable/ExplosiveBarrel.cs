using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ExplosiveBarrel : MonoBehaviour, IDamageable
{
    public delegate void ExplosiveBarrelEvent(ExplosiveBarrel explosiveBarrel);
    ExplosiveBarrelEvent OnExploded;

    [SerializeField] int _defaultHitPoints;

    [SerializeField]  int _hitPoints;
    int _networkHitPoints { get { return _hitPoints; } set { _hitPoints = value; if (_hitPoints <= 0) OnExploded?.Invoke(this); } }
    public int hitPoints
    {
        get { return _networkHitPoints; }
        set
        {
            GetComponent<PhotonView>().RPC("UpdateHitPoints", RpcTarget.All, value);
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

    public void Damage(int damage, bool headshot, int playerWhoShotThisPlayerPhotonId, Vector3? impactPos = null, string damageSource = null, bool isGroin = false)
    {
        hitPoints -= damage;
    }
    #endregion

    // Delegates
    #region
    void OnExplode_Delegate(ExplosiveBarrel explosiveBarrel)
    {
        GameObject e = Instantiate(explosionPrefab, transform.position + new Vector3(0, 1, 0), transform.rotation);

        Destroy(e, 10);
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

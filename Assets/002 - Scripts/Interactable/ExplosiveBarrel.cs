using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveBarrel : MonoBehaviour, IDamageable
{
    [SerializeField] int _hitPoints;
    public int hitPoints
    {
        get { return _hitPoints; }
        set
        {
            _hitPoints = value;

            if (hitPoints <= 0)
                gameObject.SetActive(false);
        }
    }
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
}

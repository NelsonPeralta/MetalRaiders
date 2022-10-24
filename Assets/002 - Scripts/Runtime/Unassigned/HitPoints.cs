using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitPoints : MonoBehaviour
{
    public delegate void HitPointsDelegate(HitPoints hitPoints);
    public HitPointsDelegate OnDeath, OnHitPointsChanged, OnHitPointsDamaged, OnHealthDamaged, OnHealthRechargeStarted, OnShieldRechargeStarted, OnShieldDamaged, OnShieldBroken;

    float _hitPoints;
    int _maxHitPoints, _maxHealthPoints, _maxShieldPoints;
    bool _isDead;

    public float hitPoints
    {
        get { return _hitPoints; }

        set
        {
            float previousValue = _hitPoints;
            _hitPoints = Mathf.Clamp(value, 0, _maxHitPoints);

            if (previousValue > value)
                OnHitPointsDamaged?.Invoke(this);

            if (previousValue != value)
                OnHitPointsChanged?.Invoke(this);

            if (maxHitPoints == 250)
            {
                if (value >= maxHealthPoints && value < previousValue)
                {
                    Debug.Log("OnPlayerShieldDamaged");
                    OnShieldDamaged?.Invoke(this);
                }

                if (value <= maxHealthPoints && previousValue > maxHealthPoints)
                    OnShieldBroken?.Invoke(this);
            }

            if (value < maxHealthPoints && previousValue <= maxHealthPoints)
                OnHealthDamaged?.Invoke(this);



            if (_hitPoints <= 0)
                isDead = true;

            //impactPos = null;
        }
    }

    public bool isDead
    {
        get { return _isDead; }
        set
        {
            bool previousValue = _isDead;
            _isDead = value;

            if (value && !previousValue)
                OnDeath?.Invoke(this);
        }
    }

    public int maxHitPoints { get { return _maxHitPoints; } set { _maxHitPoints = value; } }

    public int maxHealthPoints
    {
        get { return _maxHealthPoints; }
        private set { _maxHealthPoints = value; }
    }
    public int maxShieldPoints
    {
        get { return _maxShieldPoints; }
        private set { _maxShieldPoints = value; }
    }
}

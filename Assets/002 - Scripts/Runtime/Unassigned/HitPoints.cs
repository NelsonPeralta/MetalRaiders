using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using static Player;

public class HitPoints : MonoBehaviour
{
    public delegate void HitPointsDelegate(HitPoints hitPoints);
    public HitPointsDelegate OnDeath, OnDeathLate, OnHitPointsChanged, OnHitPointsDamaged, OnHealthDamaged, OnHealthRechargeStarted,
        OnShieldRechargeStarted, OnShieldDamaged, OnShieldBroken, OnOvershieldPointsChanged;

    public Biped biped;
    public List<Hitbox> hitboxes = new List<Hitbox>();
    public bool needsHealthPack;


    [SerializeField] float _hitPoints, _healthPoints, _shieldPoints, _overshieldPoints;
    [SerializeField] int _maxHitPoints, _maxHealthPoints, _maxShieldPoints, _maxOvershieldPoints;
    [SerializeField] bool _isDead;

    [SerializeField] float _healingCountdown, _shieldRechargeCountdown;

    int _defaultHealingCountdown = 4;
    float _healthHealingIncrement = (100 * 2), _shieldHealingIncrement = (150 * 0.5f);
    bool _isHealing, _overshieldRecharge, _isInvincible;
    Vector3 _impactPos;


    public float hitPoints
    {
        get { return _hitPoints + _overshieldPoints; }

        set
        {
            float _previousValue = hitPoints;
            float _damage = _previousValue - value;

            if (_damage > 0 && (_isInvincible || hitPoints <= 0))
                return;

            if (overshieldPoints > 0)
            {
                float _originalOsPoints = overshieldPoints;
                overshieldPoints -= _damage;
                if (_damage > _originalOsPoints)
                {
                    _damage -= _originalOsPoints;
                }
                else
                    return;
            }

            float newValue = hitPoints - _damage;

            if (overshieldPoints <= 0)
                _hitPoints = Mathf.Clamp(newValue, 0, (_maxHealthPoints + _maxShieldPoints));

            if (_previousValue > newValue)
            {
                _isHealing = false;
                if (!needsHealthPack)
                {
                    healingCountdown = _defaultHealingCountdown;
                    shieldRechargeCountdown = _defaultHealingCountdown;
                    if (hitPoints <= maxHealthPoints)
                        shieldRechargeCountdown = _defaultHealingCountdown + ((maxHealthPoints - hitPoints) / _healthHealingIncrement);
                }
                OnHitPointsDamaged?.Invoke(this);
            }

            if (_previousValue != newValue)
                OnHitPointsChanged?.Invoke(this);

            if (_maxShieldPoints > 0)
            {
                if (newValue >= maxHealthPoints && newValue < _previousValue)
                {
                    OnShieldDamaged?.Invoke(this);
                }

                if (newValue <= maxHealthPoints && _previousValue > maxHealthPoints)
                    OnShieldBroken?.Invoke(this);
            }

            if (newValue < maxHealthPoints && _previousValue <= maxHealthPoints && _previousValue > newValue)
                OnHealthDamaged?.Invoke(this);

            isDead = _hitPoints <= 0;

            impactPos = null;

            _shieldPoints = shieldPoints;
            _healthPoints = healthPoints;
        }
    }
    public float shieldPoints
    {
        get { return Mathf.Clamp((hitPoints - maxHealthPoints), 0, maxShieldPoints); }
    }

    public float healthPoints
    {
        get { return (hitPoints - shieldPoints); }
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

    public float shieldRechargeCountdown
    {
        get { return _shieldRechargeCountdown; }
        private set
        {
            _shieldRechargeCountdown = Mathf.Clamp(value, 0, _defaultHealingCountdown + (maxHealthPoints / _healthHealingIncrement));
        }
    }
    public float healingCountdown
    {
        get { return _healingCountdown; }
        private set
        {
            _healingCountdown = Mathf.Clamp(value, 0, _defaultHealingCountdown);
        }
    }
    public float overshieldPoints
    {
        get { return _overshieldPoints; }
        private set
        {
            _overshieldPoints = Mathf.Clamp(value, 0, _maxOvershieldPoints);

            if (_overshieldPoints >= _maxOvershieldPoints)
            {
                _isInvincible = false;
                _overshieldRecharge = false;
            }

            if (_overshieldPoints <= 0)
            {

                //_overshieldFx.SetActive(false);
            }

            OnOvershieldPointsChanged?.Invoke(this);
        }
    }
    public Vector3? impactPos
    {
        protected set
        {
            try
            {
                _impactPos = (Vector3)value;
            }
            catch { }
        }
        get { return _impactPos; }
    }







    private void Awake()
    {
        try { biped = GetComponent<Biped>(); } catch { }
        hitboxes = GetComponentsInChildren<Hitbox>().ToList();
        foreach (Hitbox hitbox in hitboxes) { hitbox.hitPoints = this; }
    }

    private void Start()
    {

    }

    private void Update()
    {
        HitPointsRecharge();
        OvershieldPointsRecharge();
    }

    void HitPointsRecharge()
    {

        if (healingCountdown > 0)
        {
            healingCountdown -= Time.deltaTime;
        }

        if (healingCountdown <= 0 && hitPoints < maxHitPoints && !needsHealthPack)
        {
            if (!_isHealing)
                OnShieldRechargeStarted?.Invoke(this);

            _isHealing = true;
            if (hitPoints < maxHealthPoints)
                hitPoints += (Time.deltaTime * _healthHealingIncrement);
            else
                hitPoints += (Time.deltaTime * _shieldHealingIncrement);

            if (hitPoints == maxHitPoints)
                _isHealing = false;
        }

        if (shieldRechargeCountdown > 0)
        {
            shieldRechargeCountdown -= Time.deltaTime;
        }
    }

    void OvershieldPointsRecharge()
    {
        if (_overshieldRecharge && overshieldPoints < _maxOvershieldPoints)
        {
            hitPoints = _maxHitPoints;
            _isInvincible = true;
            overshieldPoints += (Time.deltaTime * _shieldHealingIncrement);
        }
    }

}

using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class HitPoints : MonoBehaviour
{
    public delegate void HitPointsDelegate(HitPoints hitPoints);
    public HitPointsDelegate OnDeath, OnDeathLate, OnHitPointsChanged, OnHitPointsDamaged, OnHealthDamaged, OnHealthRechargeStarted,
        OnShieldRechargeStarted, OnShieldDamaged, OnShieldBroken, OnOvershieldPointsChanged;

    public Biped biped;
    public List<Hitbox> hitboxes = new List<Hitbox>();
    [SerializeField] int _maxHitPoints, _maxHealthPoints, _maxShieldPoints;
    public bool meleeMagnetism;
    public bool healthRegenaration;


    [SerializeField] bool _isDead;

    [SerializeField] float _hitPoints, _healingCountdown, _shieldRechargeCountdown;

    int _defaultHealingCountdown = 4, _maxOvershieldPoints;
    float _healthHealingIncrement = (100 * 2), _shieldHealingIncrement = (150 * 0.5f), _healthPoints, _shieldPoints, _overshieldPoints;
    bool _isHealing, _overshieldRecharge, _isInvincible;
    Vector3 _impactPos;


    public float hitPoints
    {
        get { return _hitPoints + _overshieldPoints; }

        private set
        {
            float _previousValue = hitPoints;
            float _damage = _previousValue - value;

            Log.Print(() =>$"Hitpoints damage:{_damage}");

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
            {
                Log.Print(() =>newValue);
                Log.Print(() =>Mathf.Clamp(newValue, 0, (maxHealthPoints + maxShieldPoints)));
                _hitPoints = Mathf.Clamp(newValue, 0, (maxHealthPoints + maxShieldPoints));
            }

            if (_previousValue > newValue)
            {
                _isHealing = false;
                if (healthRegenaration)
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
            {
                Log.Print(() =>"OnDeath");
                OnDeath?.Invoke(this);
            }
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

        _hitPoints = maxHitPoints; _maxHealthPoints = maxHitPoints - maxShieldPoints;
    }

    private void Start()
    {
    }

    private void Update()
    {
        HitPointsRegeneration();
        OvershieldPointsRecharge();
    }

    void HitPointsRegeneration()
    {

        if (healingCountdown > 0)
        {
            healingCountdown -= Time.deltaTime;
        }

        if (healingCountdown <= 0 && hitPoints < maxHitPoints && healthRegenaration)
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




















    // -999 = Guardians
    public void Damage(int dam, bool head, int pid, Vector3? impactPos = null, Vector3? impactDir = null, string damageSource = null, bool groin = false)
    {
        hitPoints -= dam;
    }
}

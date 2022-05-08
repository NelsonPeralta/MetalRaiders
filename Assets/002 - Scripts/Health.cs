using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    // delegate signature de fonction
    public delegate void HealthEvent(Health health);

    // Listeners
    public HealthEvent OnChanged;
    public HealthEvent OnDamaged;
    public HealthEvent OnDeath;

    public int max;
    public int initialHealth;

    [SerializeField] int _value; // Current Life
    public int value
    {
        get { return _value; }
        set
        {
            var previous = _value;

            _value = Mathf.Clamp(value, 0, max);

            if (_value != previous)
            {
                OnChanged?.Invoke(this);

                if (_value < previous)
                {
                    OnDamaged?.Invoke(this);
                }

                if (_value <= 0)
                    OnDeath?.Invoke(this);
            }
        }
    }

    private void Start()
    {
        //if (initialHealth > 0)
        //    _value = initialHealth;
        //else
        //    _value = max;
        //OnChanged?.Invoke(this);
    }
}
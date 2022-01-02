using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class AiAbstractClass : MonoBehaviour
{
    public delegate void AiEvent(AiAbstractClass aiAbstractClass);
    public AiEvent OnHealthChange, OnDeath;

    // private variables
    int _health;

    // public variables
    public int health
    {
        get
        {
            return _health;
        }

        set
        {
            health = value;
            OnHealthChange?.Invoke(this);
        }
    }
    void Start()
    {
        OnDeath += Death;
    }

    public abstract bool IsDead();
    public abstract void Damage(int damage, int playerWhoShotPDI);
    public abstract int GetHealth();

    void Death(AiAbstractClass aiAbstractClass)
    {
        Debug.Log("OnDeath from abstract class");
        SwarmManager.instance.OnAiDeath();
    }
}

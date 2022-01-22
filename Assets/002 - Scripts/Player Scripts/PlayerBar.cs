using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class PlayerBar : MonoBehaviour
{
    public Player player;

    private void Awake()
    {
        player.OnPlayerDamaged += OnPlayerDamaged_Delegate;
    }

    public abstract void OnPlayerDamaged_Delegate(Player player);
}

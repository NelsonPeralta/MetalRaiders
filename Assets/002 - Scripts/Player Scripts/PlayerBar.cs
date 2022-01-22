using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

abstract public class PlayerBar : MonoBehaviour
{
    public Player player;
    public GameObject holder;
    private void Awake()
    {
        player.OnPlayerHitPointsChanged += OnPlayerHitPointsChanged_Delegate;
    }

    private void Start()
    {
        OnPlayerHitPointsChanged_Delegate(player);
    }
    public abstract void OnPlayerHitPointsChanged_Delegate(Player player);
}

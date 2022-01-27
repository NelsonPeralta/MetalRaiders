using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerShieldBar : PlayerBar
{
    public Slider healingSlider;
    private void Start()
    {
        healingSlider.maxValue = player.defaultHealingCountdown + ((player.maxHealthPoints - player.hitPoints) / player.healthHealingIncrement);
        GetComponent<Slider>().maxValue = player.maxShieldPoints;

        player.OnPlayerShieldDamaged += OnShieldDamaged_Delegate;
        player.OnPlayerShieldBroken += OnShieldBroken_Delegate;
    }

    private void Update()
    {
        healingSlider.value = player.shieldRechargeCountdown;
    }
    public override void OnPlayerHitPointsChanged_Delegate(Player player)
    {
        if (player.hitPoints >= 100)
            GetComponent<Slider>().value = player.hitPoints - 100;
    }
    void OnShieldDamaged_Delegate(Player player)
    {
        healingSlider.maxValue = player.defaultHealingCountdown;
    }

    void OnShieldBroken_Delegate(Player player)
    {
        healingSlider.maxValue = player.defaultHealingCountdown + ((player.maxHealthPoints - player.hitPoints)/ player.healthHealingIncrement);
    }
}

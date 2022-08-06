using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerShieldBar : PlayerBar
{
    public Slider healingSlider;
    [SerializeField] GameObject redAlertBar;

    float redAlertBarCountdown = 1;
    private void Start()
    {
        healingSlider.maxValue = player.defaultHealingCountdown + ((player.maxHealthPoints - player.hitPoints) / player.healthHealingIncrement);
        GetComponent<Slider>().maxValue = player.maxShieldPoints;

        player.OnPlayerShieldDamaged += OnShieldDamaged_Delegate;
        player.OnPlayerShieldBroken += OnShieldBroken_Delegate;

        redAlertBar.SetActive(false);
    }

    private void Update()
    {
        healingSlider.value = player.shieldRechargeCountdown;

        if (redAlertBarCountdown <= 0.25f)
        {
            redAlertBarCountdown -= Time.deltaTime;

            if (redAlertBarCountdown < 0.25f && redAlertBarCountdown >= 0 && !redAlertBar.activeSelf)
                redAlertBar.SetActive(true);
            else if (redAlertBarCountdown < 0 && redAlertBar.activeSelf)
                redAlertBar.SetActive(false);
            else if (redAlertBarCountdown < -0.25f && player.hitPoints <= 100)
                redAlertBarCountdown = 0.25f;
            else if (player.hitPoints > 100)
            {
                redAlertBar.SetActive(false);
                redAlertBarCountdown = 1;
            }

        }
    }
    public override void OnPlayerHitPointsChanged_Delegate(Player player)
    {
        if (player.hitPoints >= 100)
        {
            GetComponent<Slider>().value = player.hitPoints - 100;
            redAlertBarCountdown = 1;
        }
        else
            GetComponent<Slider>().value = 0;
    }
    void OnShieldDamaged_Delegate(Player player)
    {
        healingSlider.maxValue = player.defaultHealingCountdown;
    }

    void OnShieldBroken_Delegate(Player player)
    {
        Debug.Log("OnShieldBroken_Delegate");
        healingSlider.maxValue = player.defaultHealingCountdown + ((player.maxHealthPoints - player.hitPoints) / player.healthHealingIncrement);
        redAlertBarCountdown = 0.25f;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerShieldBar : PlayerBar
{
    public Slider healingSlider;
    [SerializeField] GameObject redAlertBar;

    float redAlertInterval = 0.25f;
    float _redAlertBarCountdown = 1;
    float redAlertBarCountdown
    {
        get { return _redAlertBarCountdown; }
        set { _redAlertBarCountdown = value; if (value > redAlertInterval) { redAlertBar.SetActive(false); } }
    }
    private void Start()
    {
        if (GameManager.instance.gameMode == GameManager.GameMode.Swarm)
            healingSlider.gameObject.SetActive(false);
        healingSlider.maxValue = player.defaultHealingCountdown + ((player.maxHealthPoints - player.hitPoints) / player.healthHealingIncrement);
        GetComponent<Slider>().maxValue = player.maxShieldPoints;

        player.OnPlayerShieldDamaged += OnShieldDamaged_Delegate;
        player.OnPlayerShieldBroken += OnShieldBroken_Delegate;

        redAlertBar.SetActive(false);
    }

    private void Update()
    {
        healingSlider.value = player.shieldRechargeCountdown;

        if (redAlertBarCountdown <= redAlertInterval)
        {
            redAlertBarCountdown -= Time.deltaTime;

            if (redAlertBarCountdown < redAlertInterval && redAlertBarCountdown >= 0 && !redAlertBar.activeSelf)
                redAlertBar.SetActive(true);
            else if (redAlertBarCountdown < 0 && redAlertBar.activeSelf)
                redAlertBar.SetActive(false);
            else if (redAlertBarCountdown < -redAlertInterval && player.hitPoints <= 100)
                redAlertBarCountdown = redAlertInterval;
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
        redAlertBarCountdown = redAlertInterval;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerShieldBar : PlayerBar
{
    public Slider healingSlider;
    [SerializeField] GameObject redAlertBar;
    [SerializeField] bool isOvershield;

    float redAlertInterval = 0.25f;
    float _redAlertBarCountdown = 1;
    float redAlertBarCountdown
    {
        get { return _redAlertBarCountdown; }
        set { _redAlertBarCountdown = value; if (value > redAlertInterval) { redAlertBar.SetActive(false); } }
    }
    private void Start()
    {
        if (GameManager.instance.gameMode == GameManager.GameMode.Swarm
            || GameManager.instance.gameType == GameManager.GameType.Swat
            || GameManager.instance.gameType ==  GameManager.GameType.Retro)
            healingSlider.gameObject.SetActive(false);
        healingSlider.maxValue = player.defaultHealingCountdown + ((player.maxHealthPoints - player.hitPoints) / player.healthHealingIncrement);
        GetComponent<Slider>().maxValue = player.maxShieldPoints;

        player.OnPlayerShieldDamaged += OnShieldDamaged_Delegate;
        player.OnPlayerShieldBroken += OnShieldBroken_Delegate;
        player.OnPlayerOvershieldPointsChanged += OnPlayerOvershieldPointsChanged_Delegate;

        redAlertBar.SetActive(false);
    }

    private void Update()
    {
        if (isOvershield)
            return;

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
        if (isOvershield)
        {
            GetComponent<Slider>().value = player.overshieldPoints;
            return;
        }

        if (player.hitPoints >= player.maxHealthPoints)
        {
            GetComponent<Slider>().value = player.hitPoints - player.maxHealthPoints;
            redAlertBarCountdown = 1;
        }
        else
            GetComponent<Slider>().value = 0;
    }

    public override void OnPlayerOvershieldPointsChanged_Delegate(Player player)
    {
        if (isOvershield)
        {
            GetComponent<Slider>().value = player.overshieldPoints;
            return;
        }

        if (player.hitPoints >= player.maxHealthPoints)
        {
            GetComponent<Slider>().value = player.hitPoints - player.maxHealthPoints;
            redAlertBarCountdown = 1;
        }
        else
            GetComponent<Slider>().value = 0;
    }
    void OnShieldDamaged_Delegate(Player player)
    {
        if (isOvershield)
            return;

        healingSlider.maxValue = player.defaultHealingCountdown;
    }

    void OnShieldBroken_Delegate(Player player)
    {
        if (isOvershield)
            return;

        healingSlider.maxValue = player.defaultHealingCountdown + ((player.maxHealthPoints - player.hitPoints) / player.healthHealingIncrement);
        redAlertBarCountdown = redAlertInterval;
    }
}

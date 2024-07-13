using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : PlayerBar
{
    public GameObject healthSliderGO;

    private void Start()
    {
        GetComponent<Slider>().maxValue = player.maxHitPoints;
        GetComponent<Slider>().value = player.maxHitPoints;

        if (GameManager.instance.gameMode == GameManager.GameMode.Coop || GameManager.instance.gameType == GameManager.GameType.Swat || GameManager.instance.gameType == GameManager.GameType.Retro)
            holder.SetActive(true);
        else
            holder.SetActive(false);
    }
    public override void OnPlayerHitPointsChanged_Delegate(Player player)
    {
        GetComponent<Slider>().value = Mathf.Clamp(player.hitPoints, 0, player.maxHitPoints);
    }
    void Update()
    {
        if (GetComponent<Slider>().value >= (GetComponent<Slider>().maxValue * 0.6f))
            healthSliderGO.gameObject.GetComponent<Image>().color = Color.green; // Green
        else if (GetComponent<Slider>().value <= (GetComponent<Slider>().maxValue * 0.25f))
            healthSliderGO.gameObject.GetComponent<Image>().color = Color.red; // Red
        else
            healthSliderGO.gameObject.GetComponent<Image>().color = Color.yellow; // Yellow
    }

    public override void OnPlayerOvershieldPointsChanged_Delegate(Player player)
    {
    }
}

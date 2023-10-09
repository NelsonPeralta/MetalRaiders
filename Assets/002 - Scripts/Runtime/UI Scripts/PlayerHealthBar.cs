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

        if (GameManager.instance.gameMode == GameManager.GameMode.Swarm || GameManager.instance.gameType == GameManager.GameType.Swat || GameManager.instance.gameType == GameManager.GameType.Retro)
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
            healthSliderGO.gameObject.GetComponent<Image>().color = new Color32(0, 255, 0, 255); // Green
        else if (GetComponent<Slider>().value <= (GetComponent<Slider>().maxValue * 0.25f))
            healthSliderGO.gameObject.GetComponent<Image>().color = new Color32(255, 0, 0, 255); // Red
        else
            healthSliderGO.gameObject.GetComponent<Image>().color = new Color32(255, 255, 0, 255); // Yellow
    }

    public override void OnPlayerOvershieldPointsChanged_Delegate(Player player)
    {
    }
}

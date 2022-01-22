using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerShieldBar : PlayerBar
{
    private void Start()
    {
        GetComponent<Slider>().maxValue = 150;

        if (GameManager.instance.gameMode == GameManager.GameMode.Multiplayer)
            holder.SetActive(true);
        else
            holder.SetActive(false);
    }
    public override void OnPlayerHitPointsChanged_Delegate(Player player)
    {
        if (player.hitPoints >= 100)
            GetComponent<Slider>().value = player.hitPoints - 100;
    }
}

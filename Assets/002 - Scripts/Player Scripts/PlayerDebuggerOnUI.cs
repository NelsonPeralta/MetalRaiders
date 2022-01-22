using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PlayerDebuggerOnUI : MonoBehaviour
{
    public Player player;
    public Text hitPointsText;
    public Text hitPointsRechargeCountdownText;

    private void Update()
    {
        hitPointsText.text = $"Hit Points: {player.hitPoints.ToString()}";
        hitPointsRechargeCountdownText.text = $"Healing Countdown: {player.healingCountdown.ToString()}";
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class PlayerDebuggerOnUI : MonoBehaviour
{
    public Player player;
    public TMPro.TextMeshProUGUI hitPointsText;
    public TMPro.TextMeshProUGUI hitPointsRechargeCountdownText;

    private void Update()
    {
        hitPointsText.text = $"HP: {player.hitPoints.ToString()}";
        hitPointsRechargeCountdownText.text = $"{player.shieldRechargeCountdown.ToString()}";
    }
}

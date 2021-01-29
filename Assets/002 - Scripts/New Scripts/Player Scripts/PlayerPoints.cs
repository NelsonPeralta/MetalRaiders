using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPoints : MonoBehaviour
{
    [Header("Multiplayer")]
    public int multiplayerKills;

    [Header("Swarm")]
    public int swarmPoints;
    bool isInSellerRadius;

    [Header("UI Components")]
    public Text multiplayerPointsText;
    public Text swarmPointsText;

    private void Start()
    {
        if(swarmPointsText != null)
        {
            swarmPointsText.text = "0";
        }
    }
}

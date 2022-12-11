using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreboardRow : MonoBehaviour
{
    public PlayerMultiplayerMatchStats.Team team
    {
        get { return _team; }
        set
        {
            _team = value;

            if(team == PlayerMultiplayerMatchStats.Team.Red)
            {
                redBg.SetActive(true);
            }else if(team == PlayerMultiplayerMatchStats.Team.Blue)
            {
                blueBg.SetActive(true);
            }
        }
    }

    [SerializeField] GameObject redBg;
    [SerializeField] GameObject blueBg;

    PlayerMultiplayerMatchStats.Team _team;


    [Header("UI Texts")]
    public TextMeshProUGUI playerNameText;
    public TextMeshProUGUI playerKillsText;
    public TextMeshProUGUI playerDeathsText;
    public TextMeshProUGUI playerHeadshotsText;
    public TextMeshProUGUI playerCurrentPointsText;
    public TextMeshProUGUI playerTotalDamageText;
    public TextMeshProUGUI playerTotalPointsText;
}

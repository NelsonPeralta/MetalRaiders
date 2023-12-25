using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreboardRow : MonoBehaviour
{
    public GameManager.Team team
    {
        get { return _team; }
        set
        {
            _team = value;

            if(team == GameManager.Team.Red)
            {
                redBg.SetActive(true);
            }else if(team == GameManager.Team.Blue)
            {
                blueBg.SetActive(true);
            }
        }
    }

    [SerializeField] GameObject redBg;
    [SerializeField] GameObject blueBg;

    GameManager.Team _team;


    [Header("UI Texts")]
    public TextMeshProUGUI playerNameText;
    public TextMeshProUGUI playerKillsText;
    public TextMeshProUGUI playerDeathsText;
    public TextMeshProUGUI playerHeadshotsText;
    public TextMeshProUGUI playerCurrentPointsText;
    public TextMeshProUGUI playerTotalDamageText;
    public TextMeshProUGUI playerTotalPointsText;
}

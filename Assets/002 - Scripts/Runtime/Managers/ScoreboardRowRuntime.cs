using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;

public class ScoreboardRowRuntime : MonoBehaviour
{
    public ScriptObjPlayerData playerScoreStruct
    {
        get { return pss; }
        set
        {
            pss = value;


            ColorUtility.TryParseHtmlString(GameManager.colorDict[playerScoreStruct.playerExtendedPublicData.armor_color_palette.ToString().ToLower()], out _tCol);
            _dynamicBg.color = new Color(_tCol.r, _tCol.g, _tCol.b, 1);

            if (GameManager.instance.teamMode == GameManager.TeamMode.Classic)
            {
                ColorUtility.TryParseHtmlString(GameManager.colorDict[playerScoreStruct.team.ToString().ToLower()], out _tCol);
                _dynamicBg.color = new Color(_tCol.r, _tCol.g, _tCol.b, 1);
            }



            _playerNameText.text = pss.playerExtendedPublicData.username;
            _playerTagText.text = pss.playerCurrentGameScore.damage.ToString();
            _playerScoreText.text = pss.playerCurrentGameScore.score.ToString();


            if (GameManager.instance.gameMode == GameManager.GameMode.Coop)
            {
                _playerTagText.text = pss.playerCurrentGameScore.points.ToString();
                _playerScoreText.text = pss.playerCurrentGameScore.totalPoints.ToString();
            }

            GetComponentInChildren<KickPlayerBtn>().SetPlayerDataCell(value);
        }
    }







    [SerializeField] Image _dynamicBg;
    [SerializeField] TextMeshProUGUI _playerNameText, _playerTagText, _playerScoreText;
    [SerializeField] ScriptObjPlayerData pss;
    //[SerializeField] Player _player;
    [SerializeField] Image _deadWitness;

    Color _tCol;

    private void Update()
    {
        if (pss != null)
        {
            _playerTagText.text = pss.playerCurrentGameScore.damage.ToString();
            _playerScoreText.text = pss.playerCurrentGameScore.score.ToString();


            if (GameManager.instance.gameMode == GameManager.GameMode.Coop)
            {
                _playerTagText.text = pss.playerCurrentGameScore.points.ToString();
                _playerScoreText.text = pss.playerCurrentGameScore.totalPoints.ToString();
            }

            if (GameManager.GetPlayerWithUsername(pss.playerExtendedPublicData.username)) _deadWitness.gameObject.SetActive(!GameManager.GetPlayerWithUsername(pss.playerExtendedPublicData.username).isAlive);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

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

            if(GameManager.instance.teamMode == GameManager.TeamMode.Classic)
            {
                ColorUtility.TryParseHtmlString(GameManager.colorDict[playerScoreStruct.team.ToString().ToLower()], out _tCol);
                _dynamicBg.color = new Color(_tCol.r, _tCol.g, _tCol.b, 1);
            }



            _playerNameText.text = pss.playerExtendedPublicData.username;
            _playerTagText.text = pss.playerCurrentGameScore.damage.ToString();
            _playerScoreText.text = pss.playerCurrentGameScore.score.ToString();
        }
    }







    [SerializeField] Image _dynamicBg;
    [SerializeField] TextMeshProUGUI _playerNameText, _playerTagText, _playerScoreText;




    ScriptObjPlayerData pss;
    Color _tCol;
}

using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerListItem : MonoBehaviourPunCallbacks
{
    public TMP_Text playerText;
    public TMP_Text levelText;
    Photon.Realtime.Player player;


    public PlayerDatabaseAdaptor pda
    {
        get { return _pda; }
        set
        {
            _pda = value;
        }
    }

    public Image mainBg { get { return _mainBg; } }
    public Image secBg { get { return _secBg; } }
    public Image rankIm
    {
        get { return _rankIm; }
        set { _rankIm = value; }
    }

    [SerializeField] Image _mainBg, _secBg, _rankIm;

    PlayerDatabaseAdaptor _pda;
    Color _tCol;

    public void SetUp(Photon.Realtime.Player _player) // MAIN
    {
        player = _player;
        WebManager.webManagerInstance.SetPlayerListItemInRoom(_player.NickName, this);
    }

    public void SetUp(string s)
    {
        Debug.Log($"Setup {s}");
        //text.text = s;
    }

    public void UpdateColorPalette()
    {
        Debug.Log("SetupWithTeam");

        if (GameManager.instance.teamMode == GameManager.TeamMode.Classic)
        {

            Debug.Log(((PlayerMultiplayerMatchStats.Team)GameManager.instance.teamDict[playerText.text]).ToString().ToLower());
            Debug.Log(GameManager.colorDict[GameManager.instance.roomPlayerData[playerText.text].playerBasicOnlineStats.armor_color_palette]);

            ColorUtility.TryParseHtmlString(GameManager.colorDict[((PlayerMultiplayerMatchStats.Team)GameManager.instance.teamDict[playerText.text]).ToString().ToLower()], out _tCol);
            mainBg.color = _tCol;
            secBg.color = new Color(_tCol.r, _tCol.g, _tCol.b, 0.4f);
            //WebManager.webManagerInstance.SetPlayerListItemInRoom(, this);
        }
        else
        {
            if (GameManager.instance.roomPlayerData.ContainsKey(playerText.text))
            {
                ColorUtility.TryParseHtmlString(GameManager.colorDict[GameManager.instance.roomPlayerData[playerText.text].playerBasicOnlineStats.armor_color_palette], out _tCol);
                mainBg.color = _tCol;

                _tCol = new Color(_tCol.r, _tCol.g, _tCol.b, (float)100);
                secBg.color = new Color(_tCol.r, _tCol.g, _tCol.b, 0.4f);
            }
        }
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        if (player == otherPlayer)
        {
            Destroy(gameObject);
        }
    }

    public override void OnLeftRoom()
    {
        Destroy(gameObject);
    }
}
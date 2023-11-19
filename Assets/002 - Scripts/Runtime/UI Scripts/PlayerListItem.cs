using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerListItem : MonoBehaviourPunCallbacks
{
    public string playerName
    {
        get { return _playerName; }
        set
        {
            _playerName = value;
            playerText.text = _playerName;
            UpdateColorPalette();
        }
    }
    public int playerLevel
    {
        get { return _playerLevel; }
        set
        {
            _playerLevel = value;
            levelText.text = _playerLevel.ToString();
        }
    }


    Photon.Realtime.Player player;


    public PlayerDatabaseAdaptor.PlayerExtendedPublicData playerExtendedPublicData
    {
        get { return _playerExtendedPublicData; }
        set
        {
            Debug.Log($"PlayerExtendedPublicData {_playerExtendedPublicData}");
            _playerExtendedPublicData = value;



            playerName = $"{_playerExtendedPublicData.username}";

            playerLevel = _playerExtendedPublicData.level;
            UpdateColorPalette();


            PlayerProgressionManager.Rank rank = PlayerProgressionManager.GetClosestAndNextRank(playerExtendedPublicData.honor)[0];


            if (GameManager.colorDict.ContainsKey(rank.color))
            {
                Debug.Log(playerExtendedPublicData.honor);
                rankIm.enabled = true;

                Debug.Log(PlayerProgressionManager.instance.rankSprites.Where(obj => obj.name == rank.spriteName).SingleOrDefault().name);

                rankIm.sprite = PlayerProgressionManager.instance.rankSprites.Where(obj => obj.name == rank.spriteName).SingleOrDefault();

                ColorUtility.TryParseHtmlString(GameManager.colorDict[rank.color], out _tCol);
                rankIm.color = _tCol;
            }
        }
    }

    public Image mainBg { get { return _mainBg; } }
    public Image secBg { get { return _secBg; } }
    public Image rankIm
    {
        get { return _rankIm; }
        set { _rankIm = value; }
    }

    [SerializeField] string _playerName;
    [SerializeField] int _playerLevel;

    [SerializeField] TMP_Text playerText, levelText;

    [SerializeField] Image _mainBg, _secBg, _rankIm;

    Color _tCol;
    [SerializeField] PlayerDatabaseAdaptor.PlayerExtendedPublicData _playerExtendedPublicData;











    private void Start()
    {

    }



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
        Debug.Log($"UpdateColorPalette of: {playerText.text}. TeamMode: {GameManager.instance.teamMode}");

        if (GameManager.instance.teamMode == GameManager.TeamMode.Classic)
        {
            Debug.Log($"SetupWithTeam: {playerText.text}");

            foreach (KeyValuePair<string, int> items in GameManager.instance.teamDict)
            {
                print("You have " + items.Value + " " + items.Key);
            }

            try
            {
                Debug.Log(((PlayerMultiplayerMatchStats.Team)GameManager.instance.teamDict[playerText.text]).ToString().ToLower());
                Debug.Log(GameManager.colorDict[((PlayerMultiplayerMatchStats.Team)GameManager.instance.teamDict[playerText.text]).ToString().ToLower()]);
            }
            catch { }

            Debug.Log((PlayerMultiplayerMatchStats.Team)GameManager.instance.teamDict[playerText.text]);

            try
            {
                ColorUtility.TryParseHtmlString(GameManager.colorDict[((PlayerMultiplayerMatchStats.Team)GameManager.instance.teamDict[playerText.text]).ToString().ToLower()], out _tCol);

                Debug.Log(_tCol);
                //mainBg.color = _tCol;
                mainBg.color = new Color(_tCol.r, _tCol.g, _tCol.b, 1);
                secBg.color = new Color(_tCol.r, _tCol.g, _tCol.b, 0.4f);
            }
            catch (System.Exception e) { Debug.LogWarning(e); }
            //WebManager.webManagerInstance.SetPlayerListItemInRoom(, this);
        }
        else
        {
            //Debug.Log($"Setup Solo Color: {playerText.text}");
            //Debug.Log($"Setup Solo Color: {_playerName}");
            Debug.Log($"Setup Solo Color: {playerName}");

            //foreach (KeyValuePair<string, KeyCode> kvp in GameManager.instance.roomPlayerData)
            //    Debug.Log(Debug.Log("Key = {0},Value = {1}" + kvp.Key + kvp.Value);)
            if (CurrentRoomManager.instance.PlayerExtendedDataContainsPlayerName(playerName))
            {
                try
                {
                    ColorUtility.TryParseHtmlString(GameManager.colorDict[CurrentRoomManager.instance.GetPLayerExtendedData(_playerName).armor_color_palette], out _tCol);
                    Debug.Log($"Setup Solo Color: {GameManager.colorDict[CurrentRoomManager.instance.GetPLayerExtendedData(_playerName).armor_color_palette]}");
                    mainBg.color = _tCol;

                    _tCol = new Color(_tCol.r, _tCol.g, _tCol.b, (float)100);
                    secBg.color = new Color(_tCol.r, _tCol.g, _tCol.b, 0.4f);
                }
                catch { }
            }
            else
            {
                Debug.LogWarning($"roomPlayerData does NOT contain {playerText.text}");
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
using Photon.Pun;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerNamePlate : MonoBehaviour
{
    public ScriptObjPlayerData playerDataCell
    {
        get { return _playerData; }
        set
        {
            Debug.Log($"ScriptObjPlayerData {_playerData}");
            _playerData = value;

            playerText.text = _playerData.playerExtendedPublicData.username;
            levelText.text = _playerData.playerExtendedPublicData.level.ToString();
            UpdateColorPalette();


            PlayerProgressionManager.Rank rank = PlayerProgressionManager.GetClosestAndNextRank(_playerData.playerExtendedPublicData.honor)[0];


            if (GameManager.colorDict.ContainsKey(rank.color))
            {
                Debug.Log(_playerData.playerExtendedPublicData.honor);
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


    [SerializeField] ScriptObjPlayerData _playerData;
    [SerializeField] TMP_Text playerText, levelText;
    [SerializeField] Image _mainBg, _secBg, _rankIm;
    [SerializeField] GameObject _pointerEnterIndicator, _roomLeaderIcon;

    Color _tCol;











    private void Start()
    {

    }



    public void SetUp(Photon.Realtime.Player _player, bool masterClient = false) // MAIN
    {
        Debug.Log($"SetUp PlayerListItem {_player.NickName}");
        //Debug.Log($"{_player.NickName.Split(char.Parse("-"))[0]}");

        if (!GameManager.instance.devMode)
            WebManager.webManagerInstance.SetPlayerListItemInRoom(int.Parse(_player.NickName), this);
        else
        {

            PlayerDatabaseAdaptor.PlayerExtendedPublicData pepd = new PlayerDatabaseAdaptor.PlayerExtendedPublicData();
            pepd.username = _player.NickName;
            pepd.player_id = int.Parse(_player.NickName);
            pepd.armor_color_palette = "grey";
            pepd.armor_data_string = "helmet1";



            CurrentRoomManager.instance.AddExtendedPlayerData(pepd);
            this.playerDataCell = CurrentRoomManager.GetDataCellWithDatabaseId(pepd.player_id, 0);



            foreach (Photon.Realtime.Player p in PhotonNetwork.CurrentRoom.Players.Values)
            {
                if (int.Parse(p.NickName) == pepd.player_id)
                    this.playerDataCell.photonRoomIndex = PhotonNetwork.CurrentRoom.Players.FirstOrDefault(x => x.Value == p).Key;
            }
        }
    }

    public void SetUp(string s)
    {
        Debug.Log($"Setup {s}");
        //text.text = s;
    }

    public void Setup(string name, int playerDataCell) // Only used for LOCAL play
    {
        playerText.text = name;
        _playerData = CurrentRoomManager.GetLocalPlayerData(playerDataCell);
        UpdateColorPalette();
    }

    public void UpdateColorPalette()
    {
        Debug.Log($"UpdateColorPalette of: {_playerData.playerExtendedPublicData.username}. TeamMode: {GameManager.instance.teamMode}");

        if (GameManager.instance.teamMode == GameManager.TeamMode.Classic)
        {

            ScriptObjPlayerData spd = CurrentRoomManager.GetDataCellWithDatabaseId(_playerData.playerExtendedPublicData.player_id, _playerData.rewiredId);

            if ((spd.team != GameManager.Team.None))
            {
                ColorUtility.TryParseHtmlString(GameManager.colorDict[spd.team.ToString().ToLower()], out _tCol);

                Debug.Log(_tCol);
                mainBg.color = new Color(_tCol.r, _tCol.g, _tCol.b, 1);
                secBg.color = new Color(_tCol.r, _tCol.g, _tCol.b, 0.4f);
                Debug.Log($"Setup TEAM Color: {playerDataCell.team} {_tCol} {_playerData.playerExtendedPublicData.player_id}");
            }
            else
                print("PLAYER TEAM IS NONE");
        }
        else
        {
            try
            {
                Debug.Log($"Setup Solo Color: {playerDataCell.playerExtendedPublicData.armor_color_palette}");
                ColorUtility.TryParseHtmlString(playerDataCell.playerExtendedPublicData.armor_color_palette, out _tCol);
                mainBg.color = _tCol;

                _tCol = new Color(_tCol.r, _tCol.g, _tCol.b, (float)100);
                secBg.color = new Color(_tCol.r, _tCol.g, _tCol.b, 0.4f);
            }
            catch { }
        }
    }

    public void OnClick()
    {
        GameManager.PlayClickSound();
        MenuManager.Instance.OpenMenu("service_record", false);
        ServiceRecordMenu s = MenuManager.Instance.GetMenu("service_record").GetComponent<ServiceRecordMenu>();

        s.playerData = _playerData;
        Launcher.instance.playerModel.GetComponent<PlayerArmorManager>().playerDataCell = playerDataCell;
        Launcher.TogglePlayerModel(true);
    }

    public void OnPointerEnter()
    {
        _pointerEnterIndicator.SetActive(true);
    }

    public void OnPointerExit()
    {
        _pointerEnterIndicator.SetActive(false);
    }

    public void ToggleLeaderIcon(bool t)
    {
        _roomLeaderIcon.SetActive(t);
    }
}
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
            //Log.Print(() =>$"ScriptObjPlayerData {value != null}");
            _playerData = value;

            playerText.text = _playerData.steamName;
            levelText.text = _playerData.playerExtendedPublicData.level.ToString();
            UpdateColorPalette();


            PlayerProgressionManager.Rank rank = PlayerProgressionManager.GetClosestAndNextRank(_playerData.playerExtendedPublicData.honor)[0];


            if (GameManager.colorDict.ContainsKey(rank.color))
            {
                Log.Print(() => _playerData.playerExtendedPublicData.honor);
                rankIm.enabled = true;

                Log.Print(() => PlayerProgressionManager.instance.rankSprites.Where(obj => obj.name == rank.codename).SingleOrDefault().name);

                rankIm.sprite = PlayerProgressionManager.instance.rankSprites.Where(obj => obj.name == rank.codename).SingleOrDefault();

                ColorUtility.TryParseHtmlString(GameManager.colorDict[rank.color], out _tCol);
                rankIm.color = _tCol;
            }




            GetComponentInChildren<KickPlayerBtn>(includeInactive: true).SetPlayerDataCell(value);
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
    [SerializeField] KickPlayerBtn _kickPlayerBtn; // bugs when not stored, dont touch

    Color _tCol;
    float _checkForChanges;





    private void Awake()
    {
        _checkForChanges = 0.6f;
    }



    private void Start()
    {

    }

    private void Update()
    {
        if (_checkForChanges > 0)
        {
            _checkForChanges -= Time.deltaTime;

            if (_checkForChanges <= 0)
            {
                playerDataCell = playerDataCell;
                _checkForChanges = 0.6f;
            }
        }
    }



    public void SetUp(Photon.Realtime.Player _player, bool masterClient = false) // MAIN
    {
        Log.Print(() =>$"SetUp PlayerListItem {_player.NickName}");


        playerText.text = _player.CustomProperties["username"].ToString();

        ColorUtility.TryParseHtmlString("grey", out _tCol);
        mainBg.color = _tCol;
        _tCol = new Color(_tCol.r, _tCol.g, _tCol.b, (float)100);
        secBg.color = new Color(_tCol.r, _tCol.g, _tCol.b, 0.4f);

        //try
        //{
        //    WebManager.webManagerInstance.SetPlayerListItemInRoom(long.Parse(_player.NickName), this);
        //}
        //catch { }
    }

    public void SetUp(string s)
    {
        Log.Print(() =>$"Setup {s}");
        //text.text = s;
    }

    public void Setup(string name, int playerDataCellInd, bool fetchPlayerStats = false) // Only used for LOCAL play
    {
        playerText.text = name;
        this.playerDataCell = CurrentRoomManager.GetLocalPlayerData(playerDataCellInd);
        _kickPlayerBtn.SetPlayerDataCell(_playerData);
    }

    public void UpdateColorPalette()
    {
        //Log.Print(() =>$"UpdateColorPalette of: {_playerData.steamName}. TeamMode: {GameManager.instance.teamMode}");

        if (GameManager.instance.teamMode == GameManager.TeamMode.Classic)
        {

            ScriptObjPlayerData spd = CurrentRoomManager.GetDataCellWithSteamIdAndRewiredId(_playerData.steamId, _playerData.rewiredId);

            if ((spd.team != GameManager.Team.None))
            {
                ColorUtility.TryParseHtmlString(GameManager.colorDict[spd.team.ToString().ToLower()], out _tCol);

                Log.Print(() =>_tCol);
                mainBg.color = new Color(_tCol.r, _tCol.g, _tCol.b, 1);
                secBg.color = new Color(_tCol.r, _tCol.g, _tCol.b, 0.4f);
            }
            else
                Log.Print(() => "PLAYER TEAM IS NONE");
        }
        else
        {
            try
            {
                //Log.Print(() =>$"Setup Solo Color: {playerDataCell.playerExtendedPublicData.armorColorPalette}");
                ColorUtility.TryParseHtmlString(playerDataCell.playerExtendedPublicData.armorColorPalette, out _tCol);
                mainBg.color = _tCol;

                _tCol = new Color(_tCol.r, _tCol.g, _tCol.b, (float)100);
                secBg.color = new Color(_tCol.r, _tCol.g, _tCol.b, 0.4f);
            }
            catch { }
        }
    }

    public void OnClick()
    {
        if (!MenuManager.Instance.APopUpMenuisOpen())
        {
            GameManager.PlayClickSound();

            Log.Print(() => $"PlayerNamePLate {_playerData.playerExtendedPublicData.player_id} {_playerData.playerExtendedPublicData.player_id != -999}");
            if (_playerData.playerExtendedPublicData.player_id > 0)
            {
                ServiceRecordMenu s = MenuManager.Instance.GetMenu("service_record").GetComponent<ServiceRecordMenu>();
                s.playerDataCell = _playerData;
                MenuManager.Instance.OpenMenu("service_record", false);
                Launcher.instance.playerModel.GetComponent<PlayerArmorManager>().playerDataCell = playerDataCell;
                Launcher.TogglePlayerModel(true);
            }
            else
            {
                MenuManager.Instance.OpenErrorMenu($"Could not fetch player data.");
            }
        }
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
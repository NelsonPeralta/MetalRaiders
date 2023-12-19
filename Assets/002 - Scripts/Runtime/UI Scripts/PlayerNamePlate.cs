using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerNamePlate : MonoBehaviour
{
    public ScriptObjPlayerData playerData
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
    [SerializeField] GameObject _pointerEnterIndicator;

    Color _tCol;











    private void Start()
    {

    }



    public void SetUp(Photon.Realtime.Player _player) // MAIN
    {
        Debug.Log($"SetUp PlayerListItem {_player.NickName}");
        //Debug.Log($"{_player.NickName.Split(char.Parse("-"))[0]}");
        WebManager.webManagerInstance.SetPlayerListItemInRoom(int.Parse(_player.NickName), this);
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
            //Debug.Log($"SetupWithTeam: {playerText.text}");

            //foreach (KeyValuePair<string, int> items in GameManager.instance.teamDict)
            //{
            //    print("You have " + items.Value + " " + items.Key);
            //}

            //try
            //{
            //    Debug.Log(((PlayerMultiplayerMatchStats.Team)GameManager.instance.teamDict[playerText.text]).ToString().ToLower());
            //    Debug.Log(GameManager.colorDict[((PlayerMultiplayerMatchStats.Team)GameManager.instance.teamDict[playerText.text]).ToString().ToLower()]);
            //}
            //catch { }

            //Debug.Log((PlayerMultiplayerMatchStats.Team)GameManager.instance.teamDict[playerText.text]);

            try
            {
                ScriptObjPlayerData spd = CurrentRoomManager.instance.GetPlayerDataWithId(_playerData.playerExtendedPublicData.player_id);
                Debug.Log(spd);
                //ColorUtility.TryParseHtmlString(GameManager.colorDict[((PlayerMultiplayerMatchStats.Team)GameManager.instance.teamDict[playerText.text]).ToString().ToLower()], out _tCol);
                ColorUtility.TryParseHtmlString(GameManager.colorDict[spd.team.ToString().ToLower()], out _tCol);

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
            try
            {
                Debug.Log($"Setup Solo Color: {playerData.playerExtendedPublicData.armor_color_palette}");
                ColorUtility.TryParseHtmlString(playerData.playerExtendedPublicData.armor_color_palette, out _tCol);
                mainBg.color = _tCol;

                _tCol = new Color(_tCol.r, _tCol.g, _tCol.b, (float)100);
                secBg.color = new Color(_tCol.r, _tCol.g, _tCol.b, 0.4f);
            }
            catch { }
        }
    }

    public void OnClick()
    {
        MenuManager.Instance.OpenMenu("service_record", false);
        ServiceRecordMenu s = MenuManager.Instance.GetMenu("service_record").GetComponent<ServiceRecordMenu>();

        s.playerData = _playerData;
        s.playerModel.GetComponent<PlayerArmorManager>().colorPalette = _playerData.playerExtendedPublicData.armor_color_palette;
        s.playerModel.GetComponent<PlayerArmorManager>().armorDataString = _playerData.playerExtendedPublicData.armor_data_string;
        s.playerModel.GetComponent<PlayerArmorManager>().PreventReloadArmor = true;

    }

    public void OnPointerEnter()
    {
        _pointerEnterIndicator.SetActive(true);
    }

    public void OnPointerExit()
    {
        _pointerEnterIndicator.SetActive(false);
    }
}
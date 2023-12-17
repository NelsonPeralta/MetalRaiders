using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ScriptObjPlayerData", menuName = "ScriptableObjects/PlayerData", order = 1)]
public class ScriptObjPlayerData : ScriptableObject
{
    [SerializeField] bool _occupied;
    [SerializeField] GameManager.Team _team;
    [SerializeField] PlayerDatabaseAdaptor.PlayerExtendedPublicData _playerExtendedPublicData;




    public PlayerDatabaseAdaptor.PlayerExtendedPublicData playerExtendedPublicData
    {
        get { return _playerExtendedPublicData; }
        set
        {
            _playerExtendedPublicData = value;
            _occupied = (value != null);
            if (value == null)
            {
                _playerExtendedPublicData = new PlayerDatabaseAdaptor.PlayerExtendedPublicData();
            }
        }
    }

    public bool occupied { get { return _occupied; } set { _occupied = value; } }
    public GameManager.Team team { get { return _team; } set { _team = value; } }

    public void Reset()
    {
        _occupied = false;
        _team = GameManager.Team.None;
        _playerExtendedPublicData = null;
    }




    //ran = Random.Range(1, 2147483646);
    //        sod.playerExtendedPublicData.username = ran.ToString();
    //public ScriptObjPlayerData(PlayerDatabaseAdaptor.PlayerExtendedPublicData pepd = null)
    //{
    //    if(pepd == null)
    //    {

    //        _playerExtendedPublicData = null;

    //        _username = _armor_data_string = _unlocked_armor_data_string = _armor_color_palette = "";
    //        _level = _xp = _rank = _honor = _credits = 0;

    //        return;
    //    }
    //    _playerExtendedPublicData = pepd;

    //    _username = _playerExtendedPublicData.username;

    //    _level = _playerExtendedPublicData.level;
    //    _xp = _playerExtendedPublicData.xp;
    //    _rank = _playerExtendedPublicData.rank;
    //    _honor = _playerExtendedPublicData.honor;
    //    _credits = _playerExtendedPublicData.credits;

    //    _armor_data_string = _playerExtendedPublicData.armor_data_string;
    //    _unlocked_armor_data_string = _playerExtendedPublicData.unlocked_armor_data_string;
    //    _armor_color_palette = _playerExtendedPublicData.armor_color_palette;
    //}
}

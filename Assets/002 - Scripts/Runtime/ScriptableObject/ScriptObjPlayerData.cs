using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Steamworks;
using UnityEngine;

[CreateAssetMenu(fileName = "ScriptObjPlayerData", menuName = "ScriptableObjects/PlayerData", order = 1)]
public class ScriptObjPlayerData : ScriptableObject
{
    [SerializeField] bool _occupied;
    [SerializeField] int _photonRoomIndex;
    [SerializeField] GameManager.Team _team;
    [SerializeField] PlayerDatabaseAdaptor.PlayerExtendedPublicData _playerExtendedPublicData;
    [SerializeField] string _cardsFound;
    [SerializeField] int _armorPiecesPurchased;


    bool _achUnl = false;

    public int photonRoomIndex { set { _photonRoomIndex = value; } get { return _photonRoomIndex; } }
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
            else
            {
                StringBuilder sb = new StringBuilder($"-{value.unlocked_armor_data_string}-");

                sb.Replace("helmet1", "");
                sb.Replace("--", "-");
                sb.Replace("--", "-");
                sb.Replace("--", "-");
                sb.Replace("--", "-");

                if (GameManager.instance.connection == GameManager.Connection.Online)
                    UpdateArmorPiecesPurchasedCount(sb.ToString().Split(char.Parse("-")).Count() - 2);
            }
        }
    }

    public bool occupied { get { return _occupied; } set { _occupied = value; } }
    public GameManager.Team team { get { return _team; } set { _team = value; } }

    public void InitialReset()
    {
        _photonRoomIndex = -999;
        _occupied = false;
        _team = GameManager.Team.None;
        _playerExtendedPublicData = null;
        _cardsFound = "";
        _armorPiecesPurchased = 0;

        if (this == CurrentRoomManager.instance.extendedPlayerData[0])
            LoadPrefs();
    }

    public void AddFoundCard(string _n)
    {
        if (!_cardsFound.Contains(_n))
        {
            _cardsFound += $"{_n}-";
            if ((_cardsFound.ToString().Split(char.Parse("-")).Count() - 1) == 7)
            {
                Steamworks.SteamUserStats.GetAchievement("COLLECTOR", out _achUnl);
                if (!_achUnl)
                {
                    WebManager.webManagerInstance.StartCoroutine(WebManager.UnlockArmorPiece_Coroutine("-geiger-lfa-"));
                    Debug.Log($"Unlocked Achivement COLLECTOR");
                    AchievementManager.UnlockAchievement("COLLECTOR");
                }
            }

            SavePrefs();
        }
    }

    public void UpdateArmorPiecesPurchasedCount(int n)
    {
        _armorPiecesPurchased = n;

        _achUnl = false;
        Steamworks.SteamUserStats.GetAchievement("WAYT", out _achUnl);
        if (_armorPiecesPurchased > 0 && !_achUnl)
        {
            Debug.Log($"Unlocked Achivement WAYT");
            AchievementManager.UnlockAchievement("WAYT");
        }
    }

    public void SavePrefs()
    {
        PlayerPrefs.SetString("cardsUnlocked", _cardsFound);
        PlayerPrefs.Save();
    }

    public void LoadPrefs()
    {
        if (this != CurrentRoomManager.instance.extendedPlayerData[0]) return;

        _cardsFound = PlayerPrefs.GetString("cardsUnlocked", "");
    }
}

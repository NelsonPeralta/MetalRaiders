using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Steamworks;
using UnityEngine;

[CreateAssetMenu(fileName = "ScriptObjPlayerData", menuName = "ScriptableObjects/PlayerData", order = 10)]
public class ScriptObjPlayerData : ScriptableObject
{
    [SerializeField] bool _occupied, _local;
    [SerializeField] long _steamId;
    [SerializeField] string _steamName;
    [SerializeField] int _photonRoomIndex, _rewiredIndex, _startingSpawnPosInd;
    [SerializeField] int _nbLocalPlayers;
    [SerializeField] GameManager.Team _team;
    [SerializeField] PlayerDatabaseAdaptor.PlayerExtendedPublicData _playerExtendedPublicData;
    [SerializeField] PlayerCurrentGameScore _playerCurrentGameScore;
    [SerializeField] string _cardsFound, _toysFound;
    [SerializeField] int _armorPiecesPurchased;
    public float sens;


    bool _achUnl = false;

    public long steamId { get { return _steamId; } set { _steamId = value; } }
    public string steamName { get { return _steamName; } set { _steamName = value; } }
    public int photonRoomIndex { set { _photonRoomIndex = value; Debug.Log($"photonRoomIndex {value}"); } get { return _photonRoomIndex; } }
    public PlayerDatabaseAdaptor.PlayerExtendedPublicData playerExtendedPublicData
    {
        get { return _playerExtendedPublicData; }
        set
        {
            _playerExtendedPublicData = value;
            occupied = (value.username != null && value.username.Length > 0);


            if (occupied /*&& !GameManager.instance.devMode*/)
            {
                StringBuilder sb = new StringBuilder($"-{value.unlocked_armor_data_string}-");

                sb.Replace("helmet1", "");
                sb.Replace("--", "-");
                sb.Replace("--", "-");
                sb.Replace("--", "-");
                sb.Replace("--", "-");

                if (GameManager.instance.connection == GameManager.NetworkType.Internet)
                    UpdateArmorPiecesPurchasedCount(sb.ToString().Split(char.Parse("-")).Count() - 2);
            }
        }
    }

    public string cardsFound { get { return _cardsFound; } }
    public string toysFound { get { return _toysFound; } }

    public bool occupied { get { return _occupied; } set { Debug.Log($"occupied {value}"); _occupied = value; } }
    public bool local { get { return _local; } set { _local = value; } }
    public int rewiredId { get { return _rewiredIndex; } set { _rewiredIndex = value; } }
    public int startingSpawnPosInd { get { return _startingSpawnPosInd; } set { _startingSpawnPosInd = value; } }
    public int nbLocalPlayers { get { return _nbLocalPlayers; } set { _nbLocalPlayers = value; } }
    public GameManager.Team team { get { return _team; } set { _team = value; } }
    public PlayerCurrentGameScore playerCurrentGameScore
    {
        get { return _playerCurrentGameScore; }
        set { _playerCurrentGameScore = value; }
    }

    public void InitialReset()
    {
        _steamId = -999; _steamName = "";
        _playerCurrentGameScore = new PlayerCurrentGameScore();
        _playerExtendedPublicData = new PlayerDatabaseAdaptor.PlayerExtendedPublicData();
        _playerCurrentGameScore = new PlayerCurrentGameScore();
        _photonRoomIndex = -999;
        _occupied = _local = false;
        _rewiredIndex = _startingSpawnPosInd = 0;
        _team = GameManager.Team.None;
        _cardsFound = "";
        _armorPiecesPurchased = 0;
        _nbLocalPlayers = 0;

        if (this == CurrentRoomManager.instance.playerDataCells[0])
            LoadPrefs();
    }


    public void AddFoundCard(string _n)
    {
        if (!_cardsFound.Contains(_n))
        {
            _cardsFound += $"{_n}-";
            if (AllCardsFound())
            {
                CheckIfAllItemsHaveBeenFound();
            }

            SavePrefs();
        }
    }


    public void AddFoundToy(string _n)
    {
        if (!_toysFound.Contains(_n))
        {
            _toysFound += $"{_n}";

            if (AllToysFound())
            {
                CheckIfAllItemsHaveBeenFound();
            }
            SavePrefs();
        }
    }

    bool AllCardsFound()
    {
        if (_cardsFound.Contains("red") &&
                _cardsFound.Contains("blue") &&
                _cardsFound.Contains("yellow") &&
                _cardsFound.Contains("green") &&
                _cardsFound.Contains("orange") &&
                _cardsFound.Contains("white") &&
                _cardsFound.Contains("black"))
        {
            return true;
        }
        return false;
    }

    bool AllToysFound()
    {
        if (_toysFound.Contains("one") &&
                 _toysFound.Contains("two") &&
                 _toysFound.Contains("three") &&
                 _toysFound.Contains("four") &&
                 _toysFound.Contains("five") &&
                 _toysFound.Contains("six") &&
                 _toysFound.Contains("seven"))
        {
            return true;
        }
        return false;
    }

    public void CheckIfAllItemsHaveBeenFound()
    {
        if (AllCardsFound() && AllToysFound())
        {
            Steamworks.SteamUserStats.GetAchievement("COLLECTOR", out _achUnl);
            if (rewiredId == 0 && !_achUnl)
                AchievementManager.UnlockAchievement("COLLECTOR");

            if (rewiredId == 0)
                if (!CurrentRoomManager.instance.playerDataCells[0].playerExtendedPublicData.unlocked_armor_data_string.Contains("burning_helmet"))
                    WebManager.webManagerInstance.StartCoroutine(WebManager.UnlockArmorPiece_Coroutine(CurrentRoomManager.instance.playerDataCells[0], "-burning_helmet-"));
        }
    }


    public void UpdateArmorPiecesPurchasedCount(int n)
    {
        _armorPiecesPurchased = n;

        _achUnl = false;
        Steamworks.SteamUserStats.GetAchievement("WAYT", out _achUnl);
        if (_armorPiecesPurchased > 0 && !_achUnl && this == CurrentRoomManager.instance.playerDataCells[0])
        {
            Debug.Log($"Unlocked Achivement WAYT");
            AchievementManager.UnlockAchievement("WAYT");
        }
    }

    public void SavePrefs()
    {
        PlayerPrefs.SetString("cardsUnlocked", _cardsFound);
        PlayerPrefs.SetString("toysUnlocked", _toysFound);
        PlayerPrefs.Save();
    }

    public void LoadPrefs()
    {
        if (this != CurrentRoomManager.instance.playerDataCells[0]) return;

        _cardsFound = PlayerPrefs.GetString("cardsUnlocked", "");
        _toysFound = PlayerPrefs.GetString("toysUnlocked", "");
    }
}

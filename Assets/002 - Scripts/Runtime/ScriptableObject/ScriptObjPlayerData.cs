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
    [SerializeField] int _photonRoomIndex, _rewiredIndex, _startingSpawnPosInd;
    [SerializeField] GameManager.Team _team;
    [SerializeField] PlayerDatabaseAdaptor.PlayerExtendedPublicData _playerExtendedPublicData;
    [SerializeField] PlayerCurrentGameScore _playerCurrentGameScore;
    [SerializeField] string _cardsFound;
    [SerializeField] int _armorPiecesPurchased;
    public float sens;


    bool _achUnl = false;

    public int photonRoomIndex { set { _photonRoomIndex = value; } get { return _photonRoomIndex; } }
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

                if (GameManager.instance.connection == GameManager.Connection.Online)
                    UpdateArmorPiecesPurchasedCount(sb.ToString().Split(char.Parse("-")).Count() - 2);
            }
        }
    }

    public string cardsFound { get { return _cardsFound; } }

    public bool occupied { get { return _occupied; } set { _occupied = value;  } }
    public bool local { get { return _local; } set { _local = value; } }
    public int rewiredId { get { return _rewiredIndex; } set { _rewiredIndex = value;  } }
    public int startingSpawnPosInd { get { return _startingSpawnPosInd; } set { _startingSpawnPosInd = value; } }
    public GameManager.Team team { get { return _team; } set { _team = value; } }
    public PlayerCurrentGameScore playerCurrentGameScore
    {
        get { return _playerCurrentGameScore; }
        set { _playerCurrentGameScore = value; }
    }

    public void InitialReset()
    {
        _playerCurrentGameScore = new PlayerCurrentGameScore();
        _photonRoomIndex = -999;
        _occupied = _local = false;
        _rewiredIndex = _startingSpawnPosInd = 0;
        _team = GameManager.Team.None;
        _playerExtendedPublicData = null;
        _cardsFound = "";
        _armorPiecesPurchased = 0;

        if (this == CurrentRoomManager.instance.playerDataCells[0])
            LoadPrefs();
    }

    public void AddFoundCard(string _n)
    {
        if (!_cardsFound.Contains(_n))
        {
            _cardsFound += $"{_n}-";
            if (_cardsFound.Contains("red") &&
                _cardsFound.Contains("blue") &&
                _cardsFound.Contains("yellow") &&
                _cardsFound.Contains("green") &&
                _cardsFound.Contains("orange") &&
                _cardsFound.Contains("white") &&
                _cardsFound.Contains("black"))
            {
                Steamworks.SteamUserStats.GetAchievement("COLLECTOR", out _achUnl);
                if (!_achUnl && !CurrentRoomManager.instance.playerDataCells[0].playerExtendedPublicData.unlocked_armor_data_string.Contains("geiger-lfa"))
                    WebManager.webManagerInstance.StartCoroutine(WebManager.UnlockArmorPiece_Coroutine("-geiger-lfa-"));

                if (!CurrentRoomManager.instance.playerDataCells[0].playerExtendedPublicData.unlocked_armor_data_string.Contains("geiger-lfa"))
                    AchievementManager.UnlockAchievement("COLLECTOR");
            }

            SavePrefs();
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
        PlayerPrefs.Save();
    }

    public void LoadPrefs()
    {
        if (this != CurrentRoomManager.instance.playerDataCells[0]) return;

        _cardsFound = PlayerPrefs.GetString("cardsUnlocked", "");
    }
}

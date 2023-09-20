using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerDatabaseAdaptor
{
    public delegate void OnAnyPlayerDataChanged(string log);

    public PlayerListItem playerListItem;

    PlayerLoginData _playerLoginData;
    PlayerCommonData _playerCommonData;
    PlayerBasicPvPStats _playerBasicPvPStats;
    PlayerBasicPvEStats _playerBasicPvEStats;

    public PlayerLoginData playerLoginData
    {
        get { return this._playerLoginData; }
        set
        {
            if (this._playerLoginData == null)
                this._playerLoginData = value;
            else
                Debug.LogError("PlayerData already set");
        }
    }


    public void SetPlayerBasicPvPStats(PlayerBasicPvPStats playerData)
    {
        this._playerBasicPvPStats = playerData;
        Debug.Log($"Player basic pvp stats are set. Kills: {this._playerBasicPvPStats.kills}. Deaths: {this._playerBasicPvPStats.deaths}. Headshots: {this._playerBasicPvPStats.headshots}. Melee Kills: {this._playerBasicPvPStats.melee_kills}. Grenade Kills: {this._playerBasicPvPStats.grenade_kills}");
    }

    public void SetPlayerBasicPvEStats(PlayerBasicPvEStats playerData)
    {
        this._playerBasicPvEStats = playerData;
        Debug.Log($"Player basic pve stats are set. Kills: {this._playerBasicPvEStats.kills}. Deaths: {this._playerBasicPvEStats.deaths}. Headshots: {this._playerBasicPvEStats.headshots}");
    }

    //public int id
    //{
    //    get { return playerData.id; }
    //    set
    //    {
    //        if(playerData.id != value)
    //        {

    //        }
    //        playerData.id = value;
    //    }
    //}

    // ********** GETTERS **********
    public int id
    {
        get
        {
            try { return _playerLoginData.id; }
            catch { return 0; }
        }
    }
    public string username
    {
        get
        {
            try { return _playerLoginData.username; }
            catch { return ""; }
        }
    }
    // Multiplayer
    public int GetPvPKills() { return _playerBasicPvPStats.kills; }
    public int GetPvPDeaths() { return _playerBasicPvPStats.deaths; }
    public int GetPvPHeadshots() { return _playerBasicPvPStats.headshots; }
    // PvE
    public int GetPvEKills() { return _playerBasicPvEStats.kills; }
    public int GetPvEDeaths() { return _playerBasicPvEStats.deaths; }
    public int GetPvEHeadshots() { return _playerBasicPvEStats.headshots; }
    public int GetPvEHighestPoints() { return _playerBasicPvEStats.highest_points; }
    public bool PlayerDataIsSet() { return _playerLoginData != null; }
    public int PvPMeleeKills { get { return _playerBasicPvPStats.melee_kills; } }
    public int PvPGrenadeKills { get { return _playerBasicPvPStats.grenade_kills; } }


    // **************** Properties **************** //
    public int level
    {
        get { return _playerCommonData.level; }
    }

    public string rank
    {
        get
        {
            PlayerProgressionManager.Rank rank = PlayerProgressionManager.GetClosestRank(playerBasicOnlineStats.level, playerBasicOnlineStats.honor);
            return rank.spriteName;
        }
    }

    public string cleanRank
    {
        get
        {
            PlayerProgressionManager.Rank rank = PlayerProgressionManager.GetClosestRank(playerBasicOnlineStats.level, playerBasicOnlineStats.honor);
            return rank.cleanName;
        }
    }

    public int xp
    {
        get { return _playerCommonData.xp; }
    }

    public int credits
    {
        set { _playerCommonData.credits = value; }
        get { return playerBasicOnlineStats.credits; }
    }

    public int honor
    {

        set { _playerCommonData.honor = value; }
        get { return playerBasicOnlineStats.honor; }
    }
    public string armorDataString
    {
        set
        {
            playerBasicOnlineStats.armor_data_string = value;
            GameObject.FindObjectOfType<ArmoryManager>().armorDataString.text = playerBasicOnlineStats.armor_data_string.ToString();
        }
        get { return playerBasicOnlineStats.armor_data_string; }
    }

    public string unlockedArmorDataString
    {
        set { playerBasicOnlineStats.unlocked_armor_data_string = value; }
        get { return playerBasicOnlineStats.unlocked_armor_data_string; }
    }

    Color _tCol;
    public PlayerCommonData playerBasicOnlineStats
    {
        get { return _playerCommonData; }
        set
        {
            Debug.Log("PlayerCommonData");
            _playerCommonData = value;

            if (playerListItem)
            {
                playerListItem.playerText.text = $"{username}";

                playerListItem.levelText.text = $"{level}";


                ColorUtility.TryParseHtmlString(GameManager.colorDict[playerBasicOnlineStats.armor_color_palette], out _tCol);
                playerListItem.mainBg.color = _tCol;


                _tCol = new Color(_tCol.r, _tCol.g, _tCol.b, (float)100);
                playerListItem.secBg.color = new Color(_tCol.r, _tCol.g, _tCol.b, 0.4f);

                PlayerProgressionManager.Rank rank = PlayerProgressionManager.GetClosestRank(playerBasicOnlineStats.level, playerBasicOnlineStats.honor);


                Debug.Log(playerBasicOnlineStats.honor);
                if (GameManager.colorDict.ContainsKey(rank.color))
                {
                    Debug.Log($"{rank.spriteName}");
                    Debug.Log(playerBasicOnlineStats.honor);
                    playerListItem.rankIm.enabled = true;

                    Debug.Log(PlayerProgressionManager.instance.rankSprites.Where(obj => obj.name == rank.spriteName).SingleOrDefault().name);

                    playerListItem.rankIm.sprite = PlayerProgressionManager.instance.rankSprites.Where(obj => obj.name == rank.spriteName).SingleOrDefault();

                    ColorUtility.TryParseHtmlString(GameManager.colorDict[rank.color], out _tCol);
                    playerListItem.rankIm.color = _tCol;
                }
            }

            //if(_playerBasicOnlineStats == null)
            //    _playerBasicOnlineStats = value;
            //else
            //    Debug.LogError("PlayerBasicOnlineStats already set");
        }
    }

    // ********* INNER CLASSES *********
    [System.Serializable]
    public class PlayerLoginData
    {

        public int id;
        public string username;

        public static PlayerLoginData CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<PlayerLoginData>(jsonString);
        }

    }

    [System.Serializable]
    public class PlayerCommonData
    {

        int _id;
        public int level, xp, credits, honor;
        public string armor_data_string, unlocked_armor_data_string, armor_color_palette;

        public static PlayerCommonData CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<PlayerCommonData>(jsonString);
        }

    }

    [System.Serializable]
    public class PlayerBasicPvPStats
    {
        int player_id;
        public int kills, deaths, headshots, melee_kills, grenade_kills;

        public static PlayerBasicPvPStats CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<PlayerBasicPvPStats>(jsonString);
        }
    }

    [System.Serializable]
    public class PlayerBasicPvEStats
    {
        int player_id;
        public int kills, deaths, headshots, highest_points;

        public static PlayerBasicPvEStats CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<PlayerBasicPvEStats>(jsonString);
        }
    }
}

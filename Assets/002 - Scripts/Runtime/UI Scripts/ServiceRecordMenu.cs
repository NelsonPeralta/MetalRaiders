using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class ServiceRecordMenu : MonoBehaviour
{
    public ScriptObjPlayerData playerData
    {
        get { return _playerData; }
        set
        {
            _playerData = value;
            print($"ServiceRecordMenu setting player data: {_playerData}");


            Launcher.instance.playerModel.SetActive(true);

            float kd = 0;

            if (_playerData.playerExtendedPublicData.deaths > 0)
                kd = _playerData.playerExtendedPublicData.kills / (float)_playerData.playerExtendedPublicData.deaths;

            multiplayerStatsText.text = $"MULTIPLAYER\n----------\n\nKills: {_playerData.playerExtendedPublicData.kills}\nDeaths: {_playerData.playerExtendedPublicData.deaths}\nK/D: {kd}\nHeadshots: {_playerData.playerExtendedPublicData.headshots}\nMelee Kills: {_playerData.playerExtendedPublicData.melee_kills}\nGrenade Kills: {_playerData.playerExtendedPublicData.grenade_kills}";
            swarmStatsText.text = $"SWARM\n-----\n\nKills: {_playerData.playerExtendedPublicData.pve_kills}\nDeaths: {_playerData.playerExtendedPublicData.pve_deaths}\nHeadshots: {_playerData.playerExtendedPublicData.pve_headshots}\nHighest Score: {_playerData.playerExtendedPublicData.highest_points}";

            levelText.text = $"Level: {_playerData.playerExtendedPublicData.level}";
            xpText.text = $"Xp: {_playerData.playerExtendedPublicData.xp}";
            creditsText.text = $"Cuckbucks: {_playerData.playerExtendedPublicData.credits}";

            int xpNeeded = 0;
            if (PlayerProgressionManager.playerLevelToXpDic.ContainsKey(_playerData.playerExtendedPublicData.level + 1)) xpNeeded = PlayerProgressionManager.playerLevelToXpDic[_playerData.playerExtendedPublicData.level + 1];

            if (xpNeeded > 0) xpText.text += $" / {xpNeeded}";



            PlayerProgressionManager.Rank[] rank = PlayerProgressionManager.GetClosestAndNextRank(_playerData.playerExtendedPublicData.honor);
            _rankCleanNameText.text = $"Rank: {rank[0].cleanName}";
            _honorText.text = $"Honor: {_playerData.playerExtendedPublicData.honor.ToString()}\n\nNext rank: {rank[1].cleanName} at {rank[1].honorRequired} honor";

            if (GameManager.colorDict.ContainsKey(rank[0].color))
            {
                _rankImage.enabled = true;

                Debug.Log(PlayerProgressionManager.instance.rankSprites.Where(obj => obj.name == rank[0].spriteName).SingleOrDefault().name);

                _rankImage.sprite = PlayerProgressionManager.instance.rankSprites.Where(obj => obj.name == rank[0].spriteName).SingleOrDefault();

                ColorUtility.TryParseHtmlString(GameManager.colorDict[rank[0].color], out _tCol);
                _rankImage.color = _tCol;
            }


        }
    }

    [SerializeField] ScriptObjPlayerData _playerData;


    public TMP_Text levelText;
    public TMP_Text xpText;
    public TMP_Text creditsText;
    [SerializeField] TMP_Text _honorText;
    [SerializeField] TMP_Text _rankCleanNameText;

    public TMP_Text multiplayerStatsText;
    public TMP_Text swarmStatsText;

    [SerializeField] Image _rankImage;
    [SerializeField] GameObject _redWitness, _blueWitness, _yellowWitness, _greenWitness, _orangeWitness, _whiteWitness, _blackWitness;



    ServiceRecordMenu _srm;
    Color _tCol;


    private void OnEnable()
    {
        print($"ServiceRecordMenu OnEnable {playerData}");




        Launcher.instance.playerModel.GetComponent<PlayerArmorManager>().playerDataCell = CurrentRoomManager.GetLocalPlayerData(0);
        Launcher.TogglePlayerModel(true);
        Launcher.instance.playerModel.GetComponent<Animator>().SetBool("hold rifle", true);
        Launcher.instance.playerModel.GetComponent<PlayerArmorManager>().weaponForServiceRecord.SetActive(true);








        PlayerDatabaseAdaptor pda = WebManager.webManagerInstance.pda;

        float kd = 0;

        if (pda.GetPvPDeaths() > 0)
        {
            kd = pda.GetPvPKills() / (float)pda.GetPvPDeaths();
        }
        Debug.Log($"Initializing Service Record Menu. PvE Kills {pda.GetPvEKills()}");
        multiplayerStatsText.text = $"MULTIPLAYER\n----------\n\nKills: {pda.GetPvPKills()}\nDeaths: {pda.GetPvPDeaths()}\nK/D: {kd}\nHeadshots: {pda.GetPvPHeadshots()}\nMelee Kills: {pda.PvPMeleeKills}\nGrenade Kills: {pda.PvPGrenadeKills}";
        swarmStatsText.text = $"SWARM\n-----\n\nKills: {pda.GetPvEKills()}\nDeaths: {pda.GetPvEDeaths()}\nHeadshots: {pda.GetPvEHeadshots()}\nHighest Score: {pda.GetPvEHighestPoints()}";

        levelText.text = $"Level: {pda.level.ToString()}";
        xpText.text = $"Xp: {pda.xp.ToString()}";
        creditsText.text = $"Cuckbucks: {pda.credits.ToString()}";

        int xpNeeded = 0;
        if (PlayerProgressionManager.playerLevelToXpDic.ContainsKey(pda.level + 1)) xpNeeded = PlayerProgressionManager.playerLevelToXpDic[pda.level + 1];

        if (xpNeeded > 0) xpText.text += $" / {xpNeeded}";



        PlayerProgressionManager.Rank[] rank = PlayerProgressionManager.GetClosestAndNextRank(pda.honor);
        _rankCleanNameText.text = $"Rank: {rank[0].cleanName}";
        _honorText.text = $"Honor: {pda.honor.ToString()}\n\nNext rank: {rank[1].cleanName} at {rank[1].honorRequired} honor";

        if (GameManager.colorDict.ContainsKey(rank[0].color))
        {
            _rankImage.enabled = true;

            Debug.Log(PlayerProgressionManager.instance.rankSprites.Where(obj => obj.name == rank[0].spriteName).SingleOrDefault().name);

            _rankImage.sprite = PlayerProgressionManager.instance.rankSprites.Where(obj => obj.name == rank[0].spriteName).SingleOrDefault();

            ColorUtility.TryParseHtmlString(GameManager.colorDict[rank[0].color], out _tCol);
            _rankImage.color = _tCol;
        }


        // In Service Record, not nameplate
        {
            _redWitness.SetActive(!playerData && CurrentRoomManager.instance.playerDataCells[0].cardsFound.Contains("red"));
            _blueWitness.SetActive(!playerData && CurrentRoomManager.instance.playerDataCells[0].cardsFound.Contains("blue"));
            _yellowWitness.SetActive(!playerData && CurrentRoomManager.instance.playerDataCells[0].cardsFound.Contains("yellow"));
            _greenWitness.SetActive(!playerData && CurrentRoomManager.instance.playerDataCells[0].cardsFound.Contains("green"));
            _orangeWitness.SetActive(!playerData && CurrentRoomManager.instance.playerDataCells[0].cardsFound.Contains("orange"));
            _whiteWitness.SetActive(!playerData && CurrentRoomManager.instance.playerDataCells[0].cardsFound.Contains("white"));
            _blackWitness.SetActive(!playerData && CurrentRoomManager.instance.playerDataCells[0].cardsFound.Contains("black"));
        }
    }


    private void OnDisable()
    {
        _playerData = null;
        Launcher.instance.playerModel.SetActive(false);
        Launcher.instance.playerModel.GetComponent<Animator>().SetBool("hold rifle", false);
        Launcher.instance.playerModel.GetComponent<PlayerArmorManager>().weaponForServiceRecord.SetActive(false);
    }
}

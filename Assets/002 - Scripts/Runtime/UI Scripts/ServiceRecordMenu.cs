using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ServiceRecordMenu : MonoBehaviour
{
    public ScriptObjPlayerData playerDataCell // Used for nameplate
    {
        get { return _playerData; }
        set
        {
            _playerData = value;
            Log.Print(() => $"ServiceRecordMenu setting player data: {_playerData}");


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
            _honorText.text = $"Honor: {_playerData.playerExtendedPublicData.honor.ToString()}";
            if (rank[1].honorRequired > 0)
                _honorText.text += $"\n\nNext rank: {rank[1].cleanName} at {rank[1].honorRequired} honor";


            if (GameManager.colorDict.ContainsKey(rank[0].color))
            {
                _rankImage.enabled = true;

                Log.Print(() =>PlayerProgressionManager.instance.rankSprites.Where(obj => obj.name == rank[0].codename).SingleOrDefault().name);

                _rankImage.sprite = PlayerProgressionManager.instance.rankSprites.Where(obj => obj.name == rank[0].codename).SingleOrDefault();

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
    [SerializeField] GameObject _allCardsWitness, _redWitness, _blueWitness, _yellowWitness, _greenWitness, _orangeWitness, _whiteWitness, _blackWitness;
    [SerializeField] GameObject _allToysWitnessesHolder, _firstToyWitness, _secondToyWitness, _thirdToWitness, _fourthToyWitness, _fifthToyWitness, _sixthToyWitness, _seventhToyWitness;


    ServiceRecordMenu _srm;
    Color _tCol;


    private void OnEnable()
    {
        Log.Print(() => $"ServiceRecordMenu OnEnable {playerDataCell}");



        Launcher.instance.playerModel.SetActive(true);
        Launcher.instance.playerModel.GetComponent<PlayerArmorManager>().playerDataCell = CurrentRoomManager.GetLocalPlayerData(0);
        Launcher.TogglePlayerModel(true);
        Launcher.instance.playerModel.GetComponent<Animator>().SetBool("hold rifle", true);
        Launcher.instance.playerModel.GetComponent<PlayerArmorManager>().weaponForServiceRecord.SetActive(true);








        float kd = 0;

        if (playerDataCell.playerExtendedPublicData.deaths > 0)
            kd = playerDataCell.playerExtendedPublicData.kills / (float)playerDataCell.playerExtendedPublicData.deaths;
        else
            kd = 0;


        multiplayerStatsText.text = $"MULTIPLAYER\n----------\n\nKills: {playerDataCell.playerExtendedPublicData.kills}" +
            $"\nDeaths: {playerDataCell.playerExtendedPublicData.deaths}" +
            $"\nK/D: {kd}\nHeadshots: {playerDataCell.playerExtendedPublicData.headshots}" +
            $"\nMelee Kills: {playerDataCell.playerExtendedPublicData.melee_kills}" +
            $"\nGrenade Kills: {playerDataCell.playerExtendedPublicData.grenade_kills}";

        swarmStatsText.text = $"SWARM\n-----\n\nKills: {playerDataCell.playerExtendedPublicData.pve_kills}" +
            $"\nDeaths: {playerDataCell.playerExtendedPublicData.pve_deaths}" +
            $"\nHeadshots: {playerDataCell.playerExtendedPublicData.headshots}" +
            $"\nHighest Score: {playerDataCell.playerExtendedPublicData.highest_points}";

        levelText.text = $"Level: {playerDataCell.playerExtendedPublicData.level}";
        xpText.text = $"Xp: {playerDataCell.playerExtendedPublicData.xp}";
        creditsText.text = $"Cuckbucks: {playerDataCell.playerExtendedPublicData.credits}";

        int xpNeeded = 0;
        if (PlayerProgressionManager.playerLevelToXpDic.ContainsKey(playerDataCell.playerExtendedPublicData.level + 1)) xpNeeded
                = PlayerProgressionManager.playerLevelToXpDic[playerDataCell.playerExtendedPublicData.level + 1];

        if (xpNeeded > 0) xpText.text += $" / {xpNeeded}";



        PlayerProgressionManager.Rank[] rank = PlayerProgressionManager.GetClosestAndNextRank(playerDataCell.playerExtendedPublicData.honor);
        _rankCleanNameText.text = $"Rank: {rank[0].cleanName}";
        _honorText.text = $"Honor: {playerDataCell.playerExtendedPublicData.honor}";
        if (rank[1].honorRequired > 0)
            _honorText.text += $"\n\nNext rank: {rank[1].cleanName} at {rank[1].honorRequired} honor";

        if (GameManager.colorDict.ContainsKey(rank[0].color))
        {
            _rankImage.enabled = true;

            Log.Print(() =>PlayerProgressionManager.instance.rankSprites.Where(obj => obj.name == rank[0].codename).SingleOrDefault().name);

            _rankImage.sprite = PlayerProgressionManager.instance.rankSprites.Where(obj => obj.name == rank[0].codename).SingleOrDefault();

            ColorUtility.TryParseHtmlString(GameManager.colorDict[rank[0].color], out _tCol);
            _rankImage.color = _tCol;
        }


        // In Service Record, not nameplate
        {



            if (GameManager.instance.connection == GameManager.NetworkType.Internet && playerDataCell && !playerDataCell.local)
            {
                _allCardsWitness.SetActive(false);
                _allToysWitnessesHolder.SetActive(false);
            }
            else
            {
                _redWitness.SetActive(playerDataCell && playerDataCell.cardsFound.Contains("red"));
                _blueWitness.SetActive(playerDataCell && playerDataCell.cardsFound.Contains("blue"));
                _yellowWitness.SetActive(playerDataCell && playerDataCell.cardsFound.Contains("yellow"));
                _greenWitness.SetActive(playerDataCell && playerDataCell.cardsFound.Contains("green"));
                _orangeWitness.SetActive(playerDataCell && playerDataCell.cardsFound.Contains("orange"));
                _whiteWitness.SetActive(playerDataCell && playerDataCell.cardsFound.Contains("white"));
                _blackWitness.SetActive(playerDataCell && playerDataCell.cardsFound.Contains("black"));


                _firstToyWitness.SetActive(playerDataCell && playerDataCell.toysFound.Contains("one"));
                _secondToyWitness.SetActive(playerDataCell && playerDataCell.toysFound.Contains("two"));
                _thirdToWitness.SetActive(playerDataCell && playerDataCell.toysFound.Contains("three"));
                _fourthToyWitness.SetActive(playerDataCell && playerDataCell.toysFound.Contains("four"));
                _fifthToyWitness.SetActive(playerDataCell && playerDataCell.toysFound.Contains("five"));
                _sixthToyWitness.SetActive(playerDataCell && playerDataCell.toysFound.Contains("six"));
                _seventhToyWitness.SetActive(playerDataCell && playerDataCell.toysFound.Contains("seven"));


                _allCardsWitness.SetActive(true);
                _allToysWitnessesHolder.SetActive(true);
            }
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

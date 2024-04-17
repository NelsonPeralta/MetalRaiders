using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class CarnageReportMenu : MonoBehaviour
{
    float xpTimer
    {
        get { return _xpTimer; }
        set
        {
            _xpTimer = Mathf.Clamp(value, 0, 1);
            _xpSlider.value = _xpBase + (int)(_xpTimer * GameManager.instance.carnageReport.xpGained);
            _xpText.text = $"{_xpSlider.value} / {_xpToLevelUp}";

            if (_xpSlider.value >= _xpToLevelUp)
            {
                _xpBase = PlayerProgressionManager.playerLevelToXpDic[GameManager.instance.carnageReport.playerLevel + _lvlAddCount];

                _lvlAddCount++;
                _xpToLevelUp = PlayerProgressionManager.playerLevelToXpDic[GameManager.instance.carnageReport.playerLevel + _lvlAddCount];
                _xpSlider.maxValue = _xpToLevelUp;
            }
        }
    }

    float hnrTimer
    {
        get { return _hnrTimer; }
        set
        {
            _hnrTimer = Mathf.Clamp(value, 0, 1);
            _hnrSlider.value = GameManager.instance.carnageReport.currentHonor - GameManager.instance.carnageReport.honorGained + (int)(_hnrTimer * GameManager.instance.carnageReport.honorGained);
            _hnrText.text = $"{_hnrSlider.value} / {_hnrToLevelUp}";
        }
    }

    [SerializeField]
    Image _rankImage;
    [SerializeField]
    TMP_Text _lvlText, _xpText, _hnrText;
    [SerializeField] Slider _xpSlider, _hnrSlider;

    float _xpTimer, _hnrTimer;
    int _xpToLevelUp, _hnrToLevelUp;

    int _lvlAddCount = 1, _xpBase;



    [SerializeField] TMP_Text _thankYouExtraText;
    [SerializeField] GameObject _backBtn;

    private void OnEnable()
    {
        _backBtn.SetActive(false);
        StartCoroutine(EnableBackBtn_Coroutine());

        FindObjectOfType<ArmoryManager>(true).playerModel.GetComponent<PlayerArmorManager>().playerDataCell = CurrentRoomManager.GetLocalPlayerData(0);
        FindObjectOfType<ArmoryManager>(true).playerModel.SetActive(true);

        //_xpTimer = _hnrTimer = 0;

        //_xpToLevelUp = PlayerProgressionManager.playerLevelToXpDic[GameManager.instance.carnageReport.playerLevel + 1];
        //PlayerProgressionManager.Rank nextRank = PlayerProgressionManager.instance.ranks.ElementAt(PlayerProgressionManager.instance.ranks.IndexOf(PlayerProgressionManager.GetClosestRank(WebManager.webManagerInstance.pda.level, WebManager.webManagerInstance.pda.honor)) + 1);
        //_hnrToLevelUp = nextRank.honorRequired;

        //_xpSlider.maxValue = _xpToLevelUp; _hnrSlider.maxValue = _hnrToLevelUp;
        //_xpSlider.value = GameManager.instance.carnageReport.currentXp; _hnrSlider.value = GameManager.instance.carnageReport.currentHonor;
        //_xpBase = GameManager.instance.carnageReport.currentXp;

        //_hnrText.text = $"0 / {_hnrToLevelUp}";

        Debug.Log($"CarnageReportMenu {GameManager.instance.carnageReport.xpGained}");
        if (GameManager.instance.carnageReport.xpGained > 0)
        {
            _thankYouExtraText.text = $"PS: You gained {GameManager.instance.carnageReport.xpGained} xp and credits";
            if (GameManager.instance.carnageReport.leveledUp)
                _thankYouExtraText.text += $" AND leveled up ({GameManager.instance.carnageReport.newLevel}).";
            else
            {
                try
                {
                    _xpToLevelUp = PlayerProgressionManager.playerLevelToXpDic[GameManager.instance.carnageReport.playerLevel + _lvlAddCount];
                    _thankYouExtraText.text += $", only {_xpToLevelUp - (GameManager.instance.carnageReport.currentXp + GameManager.instance.carnageReport.xpGained)} xp to go! :)";
                }
                catch (System.Exception e) { Debug.LogException(e); }
            }
        }


        if (GameManager.instance.carnageReport.honorGained > 0)
        {
            _thankYouExtraText.text += $"\n\nAlso, you gained {GameManager.instance.carnageReport.honorGained} Honor";

            PlayerProgressionManager.Rank[] rs = PlayerProgressionManager.GetClosestAndNextRank(GameManager.instance.carnageReport.currentHonor + GameManager.instance.carnageReport.honorGained);

            if (!GameManager.instance.carnageReport.rankedUp)
                _thankYouExtraText.text += $". You will be promoted to {rs[1].cleanName} in {rs[1].honorRequired - (GameManager.instance.carnageReport.currentHonor + GameManager.instance.carnageReport.honorGained)} more points.";
            else
                _thankYouExtraText.text += $" and have been PROMOTED to {rs[0].cleanName}!!! Congrats :D";

            _thankYouExtraText.text += $" \n\nPlay more games to earn Honor points, win games to speed up your progress.";
        }
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (_xpToLevelUp > 0 && xpTimer < 1)
        {
            xpTimer += (float)(Time.deltaTime) * 0.5f;
        }

        if (_xpTimer == 1)
        {
            hnrTimer += Time.deltaTime;
        }
    }

    private void OnDisable()
    {
        //GameManager.carnageReport = null;
        if (GameManager.instance.previousScenePayloads.Contains(GameManager.PreviousScenePayload.OpenCarnageReport))
            GameManager.instance.previousScenePayloads.Remove(GameManager.PreviousScenePayload.OpenCarnageReport);


        FindObjectOfType<ArmoryManager>(true).playerModel.SetActive(false);
    }

    IEnumerator EnableBackBtn_Coroutine()
    {
        yield return new WaitForSeconds(1);
        _backBtn.SetActive(true);
    }
}

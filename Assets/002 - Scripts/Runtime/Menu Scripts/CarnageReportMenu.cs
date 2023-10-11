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
            Debug.Log((int)(_xpTimer * GameManager.instance.carnageReport.xpGained));
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

    private void OnEnable()
    {
        FindObjectOfType<ArmoryManager>(true).playerModel.SetActive(true);

        _xpTimer = _hnrTimer = 0;

        _xpToLevelUp = PlayerProgressionManager.playerLevelToXpDic[GameManager.instance.carnageReport.playerLevel + 1];
        PlayerProgressionManager.Rank nextRank = PlayerProgressionManager.instance.ranks.ElementAt(PlayerProgressionManager.instance.ranks.IndexOf(PlayerProgressionManager.GetClosestRank(WebManager.webManagerInstance.pda.level, WebManager.webManagerInstance.pda.honor)) + 1);
        _hnrToLevelUp = nextRank.honorRequired;

        _xpSlider.maxValue = _xpToLevelUp; _hnrSlider.maxValue = _hnrToLevelUp;
        _xpSlider.value = GameManager.instance.carnageReport.currentXp; _hnrSlider.value = GameManager.instance.carnageReport.currentHonor;
        _xpBase = GameManager.instance.carnageReport.currentXp;

        _hnrText.text = $"0 / {_hnrToLevelUp}";
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
        FindObjectOfType<ArmoryManager>(true).playerModel.SetActive(false);

    }
}

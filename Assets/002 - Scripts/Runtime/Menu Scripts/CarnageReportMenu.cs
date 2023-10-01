using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class CarnageReportMenu : MonoBehaviour
{
    float xpTimer
    {
        get { return _xpTimer; }
        set
        {
            _xpTimer = Mathf.Clamp(value, 0, 1);
            _xpSlider.value = GameManager.instance.carnageReport.finalXp - GameManager.instance.carnageReport.xpGained + (int)(_xpTimer * GameManager.instance.carnageReport.xpGained);
            _xpText.text = $"{_xpSlider.value} / {GameManager.instance.carnageReport.finalXp}";
        }
    }

    float hnrTimer
    {
        get { return _hnrTimer; }
        set
        {
            _hnrTimer = Mathf.Clamp(value, 0, 1);
            _hnrSlider.value = GameManager.instance.carnageReport.finalHonor - GameManager.instance.carnageReport.honorGained + (int)(_hnrTimer * GameManager.instance.carnageReport.honorGained);
            _hnrText.text = $"{_hnrSlider.value} / {GameManager.instance.carnageReport.finalHonor}";
        }
    }

    [SerializeField]
    Image _rankImage;
    [SerializeField]
    TMP_Text _lvlText, _xpText, _hnrText;
    [SerializeField] int _targetXp, _targetHonor;
    [SerializeField] Slider _xpSlider, _hnrSlider;

    float _xpTimer, _hnrTimer;


    private void OnEnable()
    {
        _xpTimer = _hnrTimer = 0;
        _xpSlider.maxValue = GameManager.instance.carnageReport.finalXp; _hnrSlider.maxValue = GameManager.instance.carnageReport.finalHonor;

        _targetXp = GameManager.instance.carnageReport.xpGained; _targetHonor = GameManager.instance.carnageReport.honorGained;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (_targetXp > 0)
        {
            xpTimer += (float)(Time.deltaTime);
        }

        if (_xpTimer == 1)
        {
            hnrTimer += Time.deltaTime;
        }
    }

    private void OnDisable()
    {
        //GameManager.carnageReport = null;
    }
}

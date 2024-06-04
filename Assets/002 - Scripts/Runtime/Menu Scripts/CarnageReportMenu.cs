using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarnageReportMenu : MonoBehaviour
{
    [SerializeField] CarnageReportRow[] carnageReportRowArray;
    [SerializeField] GameObject _backBtn;





    List<CarnageReportStruc> _carnageReportStrucs = new List<CarnageReportStruc>();




    Color _tCol;





    private void OnEnable()
    {
        _backBtn.SetActive(false);
        StartCoroutine(EnableBackBtn_Coroutine());
        SetupScoreboard();
    }

    public void AddStruct(CarnageReportStruc c)
    {
        _carnageReportStrucs.Add(c);
    }


    void SetupScoreboard()
    {
        //foreach (CarnageReportRow c in carnageReportRowArray) { c.gameObject.SetActive(false); }



        print($"SetupScoreboard {_carnageReportStrucs.Count}");
        for (int i = 0; i < _carnageReportStrucs.Count; i++)
        {
            print($"{_carnageReportStrucs[i].kills} {_carnageReportStrucs[i].deaths}");
            ColorUtility.TryParseHtmlString(_carnageReportStrucs[i].colorPalette.ToString().ToLower(), out _tCol);
            carnageReportRowArray[i].mainColor.color = new Color(_tCol.r, _tCol.g, _tCol.b, 1);






            carnageReportRowArray[i].playerName.text = _carnageReportStrucs[i].playerName.ToString();
            carnageReportRowArray[i].kills.text = _carnageReportStrucs[i].kills.ToString();
            carnageReportRowArray[i].deaths.text = _carnageReportStrucs[i].deaths.ToString();
            carnageReportRowArray[i].damage.text = _carnageReportStrucs[i].damage.ToString();
            carnageReportRowArray[i].score.text = _carnageReportStrucs[i].score.ToString();
            carnageReportRowArray[i].headshots.text = _carnageReportStrucs[i].headshots.ToString();


            if (_carnageReportStrucs[i].deaths > 0)
                carnageReportRowArray[i].kdr.text = $"{_carnageReportStrucs[i].kills / (float)_carnageReportStrucs[i].deaths}";
            //carnageReportRowArray[i].kdr.text = $"{Mathf.Round(((_carnageReportStrucs[i].kills / (float)_carnageReportStrucs[i].deaths) * 10) * 0.1f)}";
            else
                carnageReportRowArray[i].kdr.text = "0";


            carnageReportRowArray[i].gameObject.SetActive(true);
        }
    }


    public void ClearCarnageReportData()
    {
        _carnageReportStrucs.Clear();
    }

    IEnumerator EnableBackBtn_Coroutine()
    {
        yield return new WaitForSeconds(1);
        _backBtn.SetActive(true);
    }
}

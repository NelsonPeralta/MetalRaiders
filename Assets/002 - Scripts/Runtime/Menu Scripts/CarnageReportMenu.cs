using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;

public class CarnageReportMenu : MonoBehaviour
{
    public static GameManager.Team winningTeam = GameManager.Team.None;
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

        _carnageReportStrucs = _carnageReportStrucs.ToArray().OrderByDescending(x => x.score).ToList();

        print($"SetupScoreboard {_carnageReportStrucs.Count} {winningTeam}");
        if (winningTeam == GameManager.Team.None)
        {
            for (int i = 0; i < _carnageReportStrucs.Count; i++)
            {
                print($"{_carnageReportStrucs[i].kills} {_carnageReportStrucs[i].deaths}");
                ColorUtility.TryParseHtmlString(_carnageReportStrucs[i].colorPalette.ToString().ToLower(), out _tCol);

                print($"Changing carnage report row color to: {_tCol}");
                carnageReportRowArray[i].mainColor.color = new Color(_tCol.r, _tCol.g, _tCol.b, 1);


                carnageReportRowArray[i].playerName.text = _carnageReportStrucs[i].playerName.ToString();
                carnageReportRowArray[i].kills.text = _carnageReportStrucs[i].kills.ToString();
                carnageReportRowArray[i].deaths.text = _carnageReportStrucs[i].deaths.ToString();
                carnageReportRowArray[i].damage.text = _carnageReportStrucs[i].damage.ToString();
                carnageReportRowArray[i].score.text = _carnageReportStrucs[i].score.ToString();
                carnageReportRowArray[i].headshots.text = _carnageReportStrucs[i].headshots.ToString();


                if (_carnageReportStrucs[i].deaths > 0)
                    carnageReportRowArray[i].kdr.text = $"{_carnageReportStrucs[i].kills / (float)_carnageReportStrucs[i].deaths}";
                else
                    carnageReportRowArray[i].kdr.text = "0";


                carnageReportRowArray[i].gameObject.SetActive(true);
            }
        }
        else
        {
            int c = 0;

            for (int i = 0; i < _carnageReportStrucs.Count; i++)
            {
                if (_carnageReportStrucs[i].team == winningTeam)
                {
                    print($"{_carnageReportStrucs[i].playerName} {_carnageReportStrucs[i].team} {winningTeam}");
                    ColorUtility.TryParseHtmlString(_carnageReportStrucs[i].team.ToString().ToLower(), out _tCol);
                    print($"Changing carnage report row color to: {_tCol}");
                    carnageReportRowArray[c].mainColor.color = new Color(_tCol.r, _tCol.g, _tCol.b, 1);


                    carnageReportRowArray[c].playerName.text = _carnageReportStrucs[i].playerName.ToString();
                    carnageReportRowArray[c].kills.text = _carnageReportStrucs[i].kills.ToString();
                    carnageReportRowArray[c].deaths.text = _carnageReportStrucs[i].deaths.ToString();
                    carnageReportRowArray[c].damage.text = _carnageReportStrucs[i].damage.ToString();
                    carnageReportRowArray[c].score.text = _carnageReportStrucs[i].score.ToString();
                    carnageReportRowArray[c].headshots.text = _carnageReportStrucs[i].headshots.ToString();


                    if (_carnageReportStrucs[i].deaths > 0)
                        carnageReportRowArray[c].kdr.text = $"{_carnageReportStrucs[i].kills / (float)_carnageReportStrucs[i].deaths}";
                    else
                        carnageReportRowArray[c].kdr.text = "0";


                    carnageReportRowArray[c].gameObject.SetActive(true); c++;
                }
            }

            for (int i = 0; i < _carnageReportStrucs.Count; i++)
            {
                if (_carnageReportStrucs[i].team != winningTeam)
                {
                    print($"{_carnageReportStrucs[i].playerName} {_carnageReportStrucs[i].team} {winningTeam}");
                    ColorUtility.TryParseHtmlString(_carnageReportStrucs[i].team.ToString().ToLower(), out _tCol);
                    print($"Changing carnage report row color to: {_tCol}");
                    carnageReportRowArray[c].mainColor.color = new Color(_tCol.r, _tCol.g, _tCol.b, 1);


                    carnageReportRowArray[c].playerName.text = _carnageReportStrucs[i].playerName.ToString();
                    carnageReportRowArray[c].kills.text = _carnageReportStrucs[i].kills.ToString();
                    carnageReportRowArray[c].deaths.text = _carnageReportStrucs[i].deaths.ToString();
                    carnageReportRowArray[c].damage.text = _carnageReportStrucs[i].damage.ToString();
                    carnageReportRowArray[c].score.text = _carnageReportStrucs[i].score.ToString();
                    carnageReportRowArray[c].headshots.text = _carnageReportStrucs[i].headshots.ToString();


                    if (_carnageReportStrucs[i].deaths > 0)
                        carnageReportRowArray[c].kdr.text = $"{_carnageReportStrucs[i].kills / (float)_carnageReportStrucs[i].deaths}";
                    else
                        carnageReportRowArray[c].kdr.text = "0";


                    carnageReportRowArray[c].gameObject.SetActive(true); c++;
                }
            }
        }
    }


    public void ClearCarnageReportData()
    {
        _carnageReportStrucs.Clear();
    }

    IEnumerator EnableBackBtn_Coroutine()
    {
        yield return new WaitForSeconds(1);

        //if (PhotonNetwork.InRoom) Launcher.instance.TriggerOnJoinedRoomBehaviour();

        _backBtn.SetActive(true);
    }
}

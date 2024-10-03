using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreditsMenu : MonoBehaviour
{
    [SerializeField] TMP_Text _thankYouExtraText;
    [SerializeField] GameObject _backBtn;



    int _xpToLevelUp;
    int _lvlAddCount = 1;



    private void OnEnable()
    {
        _backBtn.SetActive(false);
        StartCoroutine(EnableBackBtn_Coroutine());

        Launcher.instance.playerModel.GetComponent<PlayerArmorManager>().playerDataCell = CurrentRoomManager.GetLocalPlayerData(0);
        Launcher.instance.playerModel.SetActive(true);


        if (GameManager.instance.carnageReport != null)
        {
            Debug.Log($"CarnageReportMenu {GameManager.instance.carnageReport.xpGained}");
            if (GameManager.instance.carnageReport.xpGained > 0)
            {
                _thankYouExtraText.text = $"PS: You gained {GameManager.instance.carnageReport.xpGained} Xp and Cuckbucks.";
                if (GameManager.instance.carnageReport.leveledUp)
                    _thankYouExtraText.text += $" LEVEL UP -> {GameManager.instance.carnageReport.newLevel}!";
                else
                {
                    try
                    {
                        _xpToLevelUp = PlayerProgressionManager.playerLevelToXpDic[GameManager.instance.carnageReport.playerLevel + _lvlAddCount];
                        _thankYouExtraText.text += $" Only {_xpToLevelUp - (GameManager.instance.carnageReport.currentXp + GameManager.instance.carnageReport.xpGained)} xp to go! :)";
                    }
                    catch (System.Exception e) { Debug.LogException(e); }
                }
            }


            if (GameManager.instance.carnageReport.honorGained > 0)
            {
                _thankYouExtraText.text += $"\n\nAlso, you gained {GameManager.instance.carnageReport.honorGained} Honor.";

                PlayerProgressionManager.Rank[] rs = PlayerProgressionManager.GetClosestAndNextRank(GameManager.instance.carnageReport.currentHonor + GameManager.instance.carnageReport.honorGained);


                if (GameManager.instance.carnageReport.rankedUp)
                {
                    _thankYouExtraText.text += $" You are hereby PROMOTED to {rs[0].cleanName}!!! Congrats :D";
                }
                else
                {
                    if (rs[1].honorRequired > 0)// Must be -1 or lesser to be ignored. Used for max rank
                    {
                        _thankYouExtraText.text += $" You will be promoted to {rs[1].cleanName} in {rs[1].honorRequired - (GameManager.instance.carnageReport.currentHonor + GameManager.instance.carnageReport.honorGained)} more points.";
                    }
                }


                //if (!GameManager.instance.carnageReport.rankedUp)
                //    _thankYouExtraText.text += $". You will be promoted to {rs[1].cleanName} in {rs[1].honorRequired - (GameManager.instance.carnageReport.currentHonor + GameManager.instance.carnageReport.honorGained)} more points.";
                //else
                //    _thankYouExtraText.text += $" and have been PROMOTED to {rs[0].cleanName}!!! Congrats :D";

                _thankYouExtraText.text += $" \n\nPlay more games to earn Honor points, win games to speed up your progress.";
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {

    }


    private void OnDisable()
    {
        //GameManager.carnageReport = null;
        if (GameManager.instance.previousScenePayloads.Contains(GameManager.PreviousScenePayload.OpenCarnageReportAndCredits))
            GameManager.instance.previousScenePayloads.Remove(GameManager.PreviousScenePayload.OpenCarnageReportAndCredits);


        Launcher.instance.playerModel.SetActive(false);
    }

    IEnumerator EnableBackBtn_Coroutine()
    {
        yield return new WaitForSeconds(1);
        _backBtn.SetActive(true);
    }
}

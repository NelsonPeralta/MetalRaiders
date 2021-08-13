using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KillFeedManager : MonoBehaviour
{
    [Header("Singletons")]
    public MultiplayerManager multiplayerManager;

    [Header("Components")]
    public GridLayoutGroup gridLayout;

    [Header("Prefabs")]
    public GameObject killFeedItemPrefab;

    private void Start()
    {
       multiplayerManager= MultiplayerManager.multiplayerManagerInstance;
    }

    public void EnterNewFeed(string playerWhoGotKillName, string playerWhoWasKilledName, bool wasHeadshot)
    {
        StartCoroutine(SpawnNewFeed_Coroutine(playerWhoGotKillName, playerWhoWasKilledName, wasHeadshot));
    }

    public void EnterNewFeed(string playerWhoCommitedSuicideName)
    {
        StartCoroutine(SpawnNewFeed_Coroutine(playerWhoCommitedSuicideName));
    }

    IEnumerator SpawnNewFeed_Coroutine(string playerWhoGotKillName, string playerWhoWasKilledName, bool wasHeadshot)
    {
        var nkf = Instantiate(killFeedItemPrefab, gridLayout.transform);
        var headshotFeed = $"";
        if(wasHeadshot)
            headshotFeed = $"with a headshot!";
        nkf.GetComponent<TMP_Text>().text = $"{playerWhoGotKillName} Killed {playerWhoWasKilledName} {headshotFeed}";
        yield return new WaitForSeconds(5);
        Destroy(nkf);
    }

    IEnumerator SpawnNewFeed_Coroutine(string playerWhoCommitedSuicideName)
    {
        var nkf = Instantiate(killFeedItemPrefab, gridLayout.transform);
        nkf.GetComponent<TMP_Text>().text = $"{playerWhoCommitedSuicideName} commited suicide";
        yield return new WaitForSeconds(5);
        Destroy(nkf);
    }
}

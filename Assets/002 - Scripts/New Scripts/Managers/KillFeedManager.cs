using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KillFeedManager : MonoBehaviour
{
    [Header("Singletons")]
    public OnlineMultiplayerManager multiplayerManager;

    [Header("Components")]
    public GridLayoutGroup gridLayout;

    [Header("Prefabs")]
    public GameObject killFeedItemPrefab;

    private void Start()
    {
       multiplayerManager= OnlineMultiplayerManager.multiplayerManagerInstance;
    }

    public void EnterNewFeed(string playerWhoGotKillName, string playerWhoWasKilledName, bool wasHeadshot)
    {
        StartCoroutine(SpawnNewFeed_Coroutine(playerWhoGotKillName, playerWhoWasKilledName, wasHeadshot));
    }

    public void EnterNewFeed(string feed)
    {
        StartCoroutine(SpawnNewFeed_Coroutine(feed));
    }

    IEnumerator SpawnNewFeed_Coroutine(string playerWhoGotKillName, string playerWhoWasKilledName, bool wasHeadshot)
    {
        var nkf = Instantiate(killFeedItemPrefab);
        var headshotFeed = $"";
        if(wasHeadshot)
            headshotFeed = $"with a headshot!";
        nkf.GetComponent<TMP_Text>().text = $"{playerWhoGotKillName} Killed {playerWhoWasKilledName} {headshotFeed}";
        nkf.transform.SetParent(gridLayout.transform);
        nkf.transform.SetAsFirstSibling();
        yield return new WaitForSeconds(5);
        Destroy(nkf);
    }

    IEnumerator SpawnNewFeed_Coroutine(string feed)
    {
        var nkf = Instantiate(killFeedItemPrefab);
        nkf.GetComponent<TMP_Text>().text = $"{feed}";
        nkf.transform.SetParent(gridLayout.transform);
        nkf.transform.SetAsFirstSibling();
        yield return new WaitForSeconds(5);
        Destroy(nkf);
    }
}

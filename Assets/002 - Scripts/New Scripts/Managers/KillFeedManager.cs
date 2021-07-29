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

    public void EnterNewFeed(string playerWhoGotKillName, string playerWhoWasKilledName)
    {
        StartCoroutine(SpawnNewFeed_Coroutine(playerWhoGotKillName, playerWhoWasKilledName));
    }

    IEnumerator SpawnNewFeed_Coroutine(string playerWhoGotKillName, string playerWhoWasKilledName)
    {
        var nkf = Instantiate(killFeedItemPrefab, gridLayout.transform);
        nkf.GetComponent<TMP_Text>().text = $"{playerWhoGotKillName} Killed {playerWhoWasKilledName}";
        yield return new WaitForSeconds(5);
        Destroy(nkf);
    }
}

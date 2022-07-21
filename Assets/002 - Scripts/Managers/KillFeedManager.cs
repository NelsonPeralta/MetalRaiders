using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KillFeedManager : MonoBehaviour
{
    public static Dictionary<string, string> killFeedColorCodeDict = new Dictionary<string, string>();
    public static Dictionary<string, int> killFeedWeaponCodeDict = new Dictionary<string, int>();
    public static Dictionary<string, int> killFeedSpecialCodeDict = new Dictionary<string, int>();

    [Header("Components")]
    public GridLayoutGroup gridLayout;

    [Header("Prefabs")]
    public GameObject killFeedItemPrefab;

    private void Start()
    {
        killFeedColorCodeDict.Add("green", "#06FF00");
        killFeedColorCodeDict.Add("blue", "#00B0FF");
        killFeedColorCodeDict.Add("purple", "#FF00B0");
        killFeedColorCodeDict.Add("orange", "#FF9000");
        killFeedColorCodeDict.Add("red", "#FF0000");
        killFeedColorCodeDict.Add("yellow", "#FFFB00");


        killFeedWeaponCodeDict.Add("m1911", 8);
        killFeedWeaponCodeDict.Add("colt", 20);
        killFeedWeaponCodeDict.Add("mp5", 13);

        killFeedWeaponCodeDict.Add("m4", 4);
        killFeedWeaponCodeDict.Add("ak47", 3);
        killFeedWeaponCodeDict.Add("m16", 21);
        killFeedWeaponCodeDict.Add("scar", 0);
        killFeedWeaponCodeDict.Add("patriot", 19);
        killFeedWeaponCodeDict.Add("m249", 15);

        killFeedWeaponCodeDict.Add("m1100", 6);
        killFeedWeaponCodeDict.Add("r700", 17);
        killFeedWeaponCodeDict.Add("barrett50cal", 18);
        killFeedWeaponCodeDict.Add("rpg", 5);

        killFeedWeaponCodeDict.Add("fraggrenade", 24);
        killFeedWeaponCodeDict.Add("stickygrenade", 28);
        killFeedWeaponCodeDict.Add("melee", 22);

        killFeedSpecialCodeDict.Add("headshot", 23);
        killFeedSpecialCodeDict.Add("nutshot", 34);



        GetComponent<PlayerController>().OnPlayerTestButton -= OnTestButton_Delegate;
        GetComponent<PlayerController>().OnPlayerTestButton += OnTestButton_Delegate;
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
        yield return new WaitForSeconds(8);
        Destroy(nkf);
    }

    IEnumerator SpawnNewFeed_Coroutine(string feed)
    {
        var nkf = Instantiate(killFeedItemPrefab);
        nkf.GetComponent<TMP_Text>().text = $"{feed}";
        nkf.transform.SetParent(gridLayout.transform, false);
        nkf.transform.SetAsFirstSibling();
        yield return new WaitForSeconds(5);
        Destroy(nkf);
    }

    void OnTestButton_Delegate(PlayerController playerController)
    {
        EnterNewFeed($"Player 123 Killed Player 123 <sprite=0 color=#FF00B0>");
    }
}

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
    public GameObject newKillFeedItemPrefab;

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



        //GetComponent<PlayerController>().OnPlayerTestButton -= OnTestButton_Delegate;
        //GetComponent<PlayerController>().OnPlayerTestButton += OnTestButton_Delegate;
    }

    public void EnterNewFeed(string feed)
    {
        try { StartCoroutine(SpawnNewFeed_Coroutine(feed)); } catch { }
    }

    public void EnterNewWeaponFeed(string part1, string part2, string weaponCodeName, bool headShot = false)
    {
        StartCoroutine(SpawnNewWeaponFeed_Coroutine(part1, part2, weaponCodeName));
    }

    public void EnterNewFeed(string part1, string part2, string weaponCodeName = "", bool headShot = false, bool melee = false)
    {

    }

    IEnumerator SpawnNewFeed_Coroutine(string playerWhoGotKillName, string playerWhoWasKilledName, bool wasHeadshot)
    {
        var nkf = Instantiate(killFeedItemPrefab);
        var headshotFeed = $"";
        if (wasHeadshot)
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

        LayoutRebuilder.ForceRebuildLayoutImmediate(gridLayout.GetComponent<RectTransform>());
        nkf.GetComponent<TMP_Text>().ForceMeshUpdate();

        // Calculates in pixels the width of text
        // TODO: Modular Kill Feed without Sprite Sheet
        Debug.Log(nkf.GetComponent<TMP_Text>().textBounds.size.x);

        yield return new WaitForSeconds(5);
        Destroy(nkf);
    }

    IEnumerator SpawnNewWeaponFeed_Coroutine(string part1, string part2, string weaponCodeName, bool headShot = false)
    {
        int adj = 500;

        var nkf = Instantiate(newKillFeedItemPrefab);
        nkf.transform.SetParent(gridLayout.transform, false);
        nkf.transform.SetAsFirstSibling();

        TMP_Text t1 = nkf.transform.Find("part 1").GetComponent<TMP_Text>();
        TMP_Text t2 = nkf.transform.Find("part 2").GetComponent<TMP_Text>();
        Image im = nkf.transform.Find("weapon").GetComponent<Image>();
        im.gameObject.SetActive(true);

        t1.text = part1;
        t2.text = part2;
        im.sprite = GameManager.GetMyPlayer().playerInventory.GetWeaponProperties(weaponCodeName).weaponIcon;

        LayoutRebuilder.ForceRebuildLayoutImmediate(gridLayout.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(im.GetComponent<RectTransform>());
        t1.ForceMeshUpdate();
        t2.ForceMeshUpdate();

        Debug.Log($"t1 bound size x: {t1.textBounds.size.x}");
        Debug.Log($"t2 bound size x: {t2.textBounds.size.x}");
        Debug.Log($"im bound size x: {im.rectTransform.rect.width}");

        var temp = im.rectTransform.anchoredPosition;
        temp.x = t1.textBounds.size.x;
        im.rectTransform.anchoredPosition = temp;

        temp = t2.rectTransform.anchoredPosition;
        temp.x = t1.textBounds.size.x + im.rectTransform.rect.width;
        t2.rectTransform.anchoredPosition = temp;

        yield return new WaitForSeconds(5);
        Destroy(nkf);
    }

    IEnumerator SpawnNewFeed_Coroutine(string part1, string part2, bool headShot = false, bool melee = false)
    {
        var nkf = Instantiate(killFeedItemPrefab);

        TMP_Text t1 = nkf.transform.Find("part 1").GetComponent<TMP_Text>();
        TMP_Text t2 = nkf.transform.Find("part 2").GetComponent<TMP_Text>();
        Image im;

        if (!headShot && !melee)
            im = nkf.transform.Find("weapon").GetComponent<Image>();



        yield return new WaitForSeconds(5);
        Destroy(nkf);
    }

    //void OnTestButton_Delegate(PlayerController playerController)
    //{
    //    //EnterNewFeed($"Player 123 Killed Player 123 <sprite=0 color=#FF00B0>");
    //    EnterNewWeaponFeed("player 1", "player 2", "m16");
    //}
}
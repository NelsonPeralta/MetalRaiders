using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using UnityEngine.UI;

public class ArmoryManager : MonoBehaviour
{
    public static ArmoryManager instance;

    public Transform scrollMenuContainer;

    public TMP_Text creditsText;
    public TMP_Text armorDataString;
    public TMP_Text newArmorDataString;
    public TMP_Text unlockedArmorDataString;

    public ArmorPieceListing armorPieceListingPrefab;
    public List<ArmorPieceListing> armorPieceListingList = new List<ArmorPieceListing>();

    [SerializeField] Slider _rotatePlayerModelScrollbar;
    Vector3 _defaultPlayerModelRotation;


    void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        instance = this;

        _defaultPlayerModelRotation = Launcher.instance.playerModel.transform.localRotation.eulerAngles;
        _rotatePlayerModelScrollbar.value = 180;
    }
    private void OnEnable()
    {
        Debug.Log($"OnEnable ArmoryManager {Launcher.instance.playerModel.name}");
        Launcher.instance.playerModel.GetComponent<PlayerArmorManager>().PreventReloadArmor = true;
        Launcher.instance.playerModel.SetActive(true);
        Launcher.instance.playerModel.GetComponent<PlayerArmorManager>().playerDataCell = CurrentRoomManager.GetLocalPlayerData(0);
        PlayerDatabaseAdaptor pda = WebManager.webManagerInstance.pda;

        creditsText.text = $"{pda.playerBasicOnlineStats.credits}cr";
        armorDataString.text = $"ADS: {pda.armorDataString.ToString()}";
        newArmorDataString.text = $"NADS: {pda.armorDataString.ToString()}";
        unlockedArmorDataString.text = $"UADS: {pda.unlockedArmorDataString.ToString()}";

        int i = 1;
        foreach (PlayerArmorPiece playerArmorPiece in Launcher.instance.playerModel.GetComponent<PlayerArmorManager>().playerArmorPieces)
        {
            if (!playerArmorPiece.hideFromArmory)
            {
                if (playerArmorPiece.cost < 0 && !pda.unlockedArmorDataString.Contains(playerArmorPiece.entity)) continue;

                GameObject pal = Instantiate(armorPieceListingPrefab.gameObject, scrollMenuContainer);
                armorPieceListingList.Add(pal.GetComponent<ArmorPieceListing>());
                pal.GetComponent<ArmorPieceListing>().playerArmorPiece = playerArmorPiece;
                pal.name += $" ({i})";
                i++;
            }
        }


        PlayerArmorPiece ppp = new PlayerArmorPiece() { entity = "filler" };
        GameObject ppppp = Instantiate(armorPieceListingPrefab.gameObject, scrollMenuContainer);
        armorPieceListingList.Add(ppppp.GetComponent<ArmorPieceListing>());
        ppppp.GetComponent<ArmorPieceListing>().playerArmorPiece = ppp;
        ppppp.name += $" (filler)";
    }

    private void OnDisable()
    {
        Launcher.TogglePlayerModel(false);
        foreach (ArmorPieceListing armorPieceListing in armorPieceListingList)
            Destroy(armorPieceListing.gameObject);
        armorPieceListingList.Clear();
    }

    public void OnArmorBuy_Delegate()
    {
        foreach (ArmorPieceListing armorPieceListing in armorPieceListingList)
            armorPieceListing.playerArmorPiece = armorPieceListing.playerArmorPiece;

    }


    float previousValue;
    public void RotateMyObject()
    {
        // REFERENCE: https://discussions.unity.com/t/rotate-gameobject-with-gui-slider/204758/2
        // How much we've changed
        float delta = (_rotatePlayerModelScrollbar.value - 180) - this.previousValue;
        Launcher.instance.playerModel.transform.Rotate(Vector3.up * delta);

        // Set our previous value for the next change
        this.previousValue = _rotatePlayerModelScrollbar.value - 180;
    }

    public void ResetPlayerModelRotation()
    {
        //Launcher.instance.playerModel.transform.rotation = Quaternion.Euler(Vector3.zero);
        //if (_defaultPlayerModelRotation.y > 180) Launcher.instance.playerModel.transform.Rotate(new Vector3(0, 180, 0), relativeTo: Space.Self);
        _rotatePlayerModelScrollbar.value = 180;
    }
}

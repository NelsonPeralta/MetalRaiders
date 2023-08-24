using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerArmorManager : MonoBehaviour
{
    public Player player;
    public List<PlayerArmorPiece> playerArmorPieces = new List<PlayerArmorPiece>();

    public string armorDataString
    {
        get { return _armorDataString; }
        set
        {
            _armorDataString = value;
            SoftReloadArmor();
        }
    }

    public string colorPalette
    {
        get { return _colorPalette; }
        private set
        {
            _colorPalette = value;
            UpdateColorPalette();
        }
    }

    [SerializeField] string _armorDataString, _colorPalette;

    int tries = 0;

    private void OnEnable()
    {
        colorPalette = "grey";

        HardReloadArmor();

        if (armorDataString == null || armorDataString == "")
        {
            tries = 0;
            StartCoroutine(ReloadArmor_Coroutine());
        }
    }

    private void Awake()
    {
        colorPalette = "grey";
        playerArmorPieces.Clear(); playerArmorPieces.AddRange(GetComponentsInChildren<PlayerArmorPiece>(true));
    }

    private void Start()
    {
        //ReloadArmor();
    }

    void HardReloadArmor()
    {
        try
        {
            if (player)
            {
                if (player.isMine)
                {
                    if (player.rid == 0)
                    {
                        armorDataString = WebManager.webManagerInstance.pda.playerBasicOnlineStats.armor_data_string;
                        colorPalette = WebManager.webManagerInstance.pda.playerBasicOnlineStats.armor_color_palette;
                    }
                    else
                    {
                        Debug.Log("PlayerArmorManager RID 0");
                        armorDataString = "helmet1";
                        colorPalette = "grey";
                    }
                }
                else
                {
                    Debug.Log("PlayerArmorManager NOT MINE");
                    Debug.Log(GameManager.instance.roomPlayerData[player.nickName].armorDataString);
                    armorDataString = GameManager.instance.roomPlayerData[player.nickName].armorDataString;
                }
            }
            else // You are in the menu
            {
                armorDataString = WebManager.webManagerInstance.pda.playerBasicOnlineStats.armor_data_string;
                colorPalette = WebManager.webManagerInstance.pda.playerBasicOnlineStats.armor_color_palette;
            }
        }
        catch { }

        DisableAllArmor();
        EnableAllArmorsInDataString();
        UpdateColorPalette();
    }

    void SoftReloadArmor()
    {
        DisableAllArmor();
        EnableAllArmorsInDataString();
    }

    void DisableAllArmor()
    {
        try
        {
            foreach (PlayerArmorPiece piece in playerArmorPieces)
                piece.gameObject.SetActive(false);
        }
        catch { }
    }

    void EnableAllArmorsInDataString()
    {
        try
        {
            foreach (PlayerArmorPiece piece in playerArmorPieces)
                piece.gameObject.SetActive(armorDataString.Contains(piece.entity));
        }
        catch { }
    }

    void UpdateColorPalette()
    {
        Texture _tex = GameManager.instance.colorPaletteTextures.Where(obj => obj.name.ToLower().Contains($"{colorPalette}")).SingleOrDefault();
        Debug.Log(_tex.name);
        //Debug.Log(player.rid);

        foreach (PlayerArmorPiece playerArmorPiece in playerArmorPieces)
            if (playerArmorPiece.canChangeColorPalette)
                try
                {
                    playerArmorPiece.GetComponent<Renderer>().material.SetTexture("_MainTex", _tex);
                }
                catch { }
    }

    IEnumerator ReloadArmor_Coroutine()
    {
        yield return new WaitForSeconds(0.1f);
        HardReloadArmor();
        tries++;

        if (tries < 10 && (armorDataString == null || armorDataString == ""))
            StartCoroutine(ReloadArmor_Coroutine());
    }
}

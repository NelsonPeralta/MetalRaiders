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
        }
    }


    [SerializeField] string _armorDataString;

    int tries = 0;

    private void OnEnable()
    {
        ReloadArmor();

        if (armorDataString == null || armorDataString == "")
        {
            tries = 0;
            StartCoroutine(ReloadArmor_Coroutine());
        }
    }

    private void Awake()
    {
        playerArmorPieces.Clear(); playerArmorPieces.AddRange(GetComponentsInChildren<PlayerArmorPiece>(true));
    }

    private void Start()
    {
        //ReloadArmor();
    }

    void ReloadArmor()
    {
        try
        {
            if (player)
            {
                if (player.isMine)
                {
                    if (player.rid == 0)
                        armorDataString = WebManager.webManagerInstance.pda.playerBasicOnlineStats.armor_data_string;
                    else
                        armorDataString = "helmet1";
                }
                else
                {
                    Debug.Log("PlayerArmorManager NOT MINE");
                    Debug.Log(GameManager.instance.roomPlayerData[player.nickName].armorDataString);
                    armorDataString = GameManager.instance.roomPlayerData[player.nickName].armorDataString;
                }
            }
            else
            {
                armorDataString = WebManager.webManagerInstance.pda.playerBasicOnlineStats.armor_data_string;
            }
        }
        catch { }

        DisableAllArmor();
        EnableAllArmorsInDataString();
        UpdateColorPalette();
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
        Texture _tex = GameManager.instance.colorPaletteTextures.Where(obj => obj.name.ToLower().Contains($"{WebManager.webManagerInstance.pda.playerBasicOnlineStats.armor_color_palette}")).SingleOrDefault();
        Debug.Log(_tex.name);

        foreach (PlayerArmorPiece playerArmorPiece in playerArmorPieces)
            if (playerArmorPiece.canChangeColorPalette)
                try
                {
                    if (GameManager.instance.armorTex)
                        playerArmorPiece.GetComponent<Renderer>().material.SetTexture("_MainTex", _tex);
                }
                catch { }
    }

    IEnumerator ReloadArmor_Coroutine()
    {
        yield return new WaitForSeconds(0.1f);
        ReloadArmor();
        tries++;

        if (tries < 10 && (armorDataString == null || armorDataString == ""))
            StartCoroutine(ReloadArmor_Coroutine());
    }
}

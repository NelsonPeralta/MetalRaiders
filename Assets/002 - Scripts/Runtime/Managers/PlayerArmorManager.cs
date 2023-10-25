using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerArmorManager : MonoBehaviour
{
    public Player player;
    public List<PlayerArmorPiece> playerArmorPieces = new List<PlayerArmorPiece>();
    public List<MarineArmorPiece> marineArmorPieces = new List<MarineArmorPiece>();

    public string armorDataString
    {
        get { return _armorDataString; }
        set
        {
            //if (_armorDataString != value)
            {
                Debug.Log($"Changing ArmorDataString from {_armorDataString} to {value}");

                _armorDataString = value;

                if (value == null || value == "")
                {
                    ToggleMarinePieces(true);
                    DisableAllArmor();
                }
                else
                {
                    SoftReloadArmor();
                }
            }
        }
    }

    public string colorPalette
    {
        get { return _colorPalette; }
        set
        {

            //if (_colorPalette != value)
            if (value != null && value != "")
            {
                Debug.Log($"Changing ColorPalette from {_colorPalette} to {value}");
                _colorPalette = value;
                UpdateColorPalette();
            }
        }
    }

    public bool isRagdoll { get { return GetComponent<PlayerRagdoll>(); } }

    [SerializeField] string _armorDataString, _colorPalette;

    int tries = 0;

    private void OnEnable()
    {

        if (marineArmorPieces.Count == 0)
            marineArmorPieces = GetComponentsInChildren<MarineArmorPiece>(true).ToList();
        ToggleMarinePieces(false);

        if (GetComponent<PlayerRagdoll>()) return;

        try { HardReloadArmor(); } catch { }

        {
            tries = 0;
            StartCoroutine(ReloadArmor_Coroutine());
        }


        //player.playerShield.ShowShieldRechargeEffect();
    }

    private void Awake()
    {
        marineArmorPieces = GetComponentsInChildren<MarineArmorPiece>(true).ToList();
        playerArmorPieces.Clear(); playerArmorPieces.AddRange(GetComponentsInChildren<PlayerArmorPiece>(true));
    }

    private void Start()
    {
        //ReloadArmor();
    }

    void ToggleMarinePieces(bool t)
    {

        try
        {
            foreach (MarineArmorPiece map in marineArmorPieces)
                map.gameObject.SetActive(t);
        }
        catch { }
    }

    public void HardReloadArmor(bool forceEnable = false)
    {
        Debug.Log("HardReloadArmor");

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
                }
                else
                {
                    if (CurrentRoomManager.instance.PlayerExtendedDataContainsPlayerName(player.nickName))
                    //if (GameManager.instance.roomPlayerData.ContainsKey(player.nickName))
                    {
                        //Debug.Log($"PlayerArmorManager NOT MINE");
                        //Debug.Log($"PlayerArmorManager NOT MINE + {GameManager.instance.roomPlayerData[player.nickName].armorDataString}");
                        //armorDataString = GameManager.instance.roomPlayerData[player.nickName].armorDataString;
                        //colorPalette = GameManager.instance.roomPlayerData[player.nickName].playerBasicOnlineStats.armor_color_palette;


                        Debug.Log($"PlayerArmorManager NOT MINE + {CurrentRoomManager.instance.GetPLayerExtendedData(player.nickName)}");
                        armorDataString = CurrentRoomManager.instance.GetPLayerExtendedData(player.nickName).armor_data_string;
                        colorPalette = CurrentRoomManager.instance.GetPLayerExtendedData(player.nickName).armor_color_palette;
                    }
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
        EnableAllArmorsInDataString(forceEnable);
        UpdateColorPalette();
    }

    void SoftReloadArmor()
    {

        DisableAllArmor();
        EnableAllArmorsInDataString();
    }

    public void DisableAllArmor()
    {
        Debug.Log("DisableAllArmor");
        try
        {
            foreach (PlayerArmorPiece piece in playerArmorPieces)
                piece.gameObject.SetActive(false);
        }
        catch { }

        if (GameManager.instance.gameMode == GameManager.GameMode.Swarm)
            try
            {
                foreach (MarineArmorPiece map in marineArmorPieces)
                    map.gameObject.SetActive(true);
            }
            catch { }
    }

    void EnableAllArmorsInDataString(bool force = false)
    {
        if (GameManager.instance.gameMode == GameManager.GameMode.Swarm && !force) return;

        Debug.Log("EnableAllArmorsInDataString");
        try
        {
            foreach (PlayerArmorPiece piece in playerArmorPieces)
                piece.gameObject.SetActive(armorDataString.Contains(piece.entity));
        }
        catch { }
    }

    void UpdateColorPalette()
    {
        Debug.Log(colorPalette);

        Texture _tex = GameManager.instance.colorPaletteTextures.Where(obj => obj.name.ToLower().Contains($"{colorPalette}")).SingleOrDefault();


        if (SceneManager.GetActiveScene().buildIndex > 0)
        {
            if (GameManager.instance.teamMode == GameManager.TeamMode.Classic)
            {
                string c = ((PlayerMultiplayerMatchStats.Team)GameManager.instance.teamDict[player.nickName]).ToString().ToLower();
                _tex = GameManager.instance.colorPaletteTextures.Where(obj => obj.name.ToLower().Contains($"{c}")).SingleOrDefault();
            }
        }

        Debug.Log(_tex.name);

        foreach (PlayerArmorPiece playerArmorPiece in playerArmorPieces)
            if (playerArmorPiece.canChangeColorPalette)
                try
                {
                    playerArmorPiece.GetComponent<Renderer>().material.SetTexture("_MainTex", _tex);
                }
                catch (Exception e) { Debug.Log(e); }
    }


    public void ReloadFpsArmor()
    {
        Debug.Log(player.playerArmorManager.armorDataString);
        foreach (PlayerArmorPiece pap in player.playerInventory.activeWeapon.GetComponentsInChildren<PlayerArmorPiece>(true))
            pap.gameObject.SetActive(player.playerArmorManager.armorDataString.Contains(pap.entity));
    }

    IEnumerator ReloadArmor_Coroutine()
    {
        yield return new WaitForSeconds(1f);

        if (!isRagdoll)
        {
            HardReloadArmor();
            tries++;

            if (tries < 3 /*&& (armorDataString == null || armorDataString == "")*/)
                StartCoroutine(ReloadArmor_Coroutine());
        }
    }
}

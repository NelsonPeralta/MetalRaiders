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

    public int ReloadArmorTries { get; set; }
    public bool PreventReloadArmor { get; set; }

    private void OnEnable()
    {

        if (marineArmorPieces.Count == 0)
            marineArmorPieces = GetComponentsInChildren<MarineArmorPiece>(true).ToList();
        ToggleMarinePieces(false);

        if (GetComponent<PlayerRagdoll>()) return;

        HardReloadArmor();

        {
            ReloadArmorTries = 0;
            StartCoroutine(ReloadArmor_Coroutine());
        }


        //player.playerShield.ShowShieldRechargeEffect();
    }

    private void Awake()
    {
        marineArmorPieces = GetComponentsInChildren<MarineArmorPiece>(true).ToList();
        playerArmorPieces.Clear();
        CreateArmorPiecesList();

    }

    private void Start()
    {
        //ReloadArmor();
    }

    void CreateArmorPiecesList()
    {
        playerArmorPieces.AddRange(GetComponentsInChildren<PlayerArmorPiece>(true));
        playerArmorPieces = playerArmorPieces.OrderByDescending(x => x.listingPriority).ToList();
    }

    void ToggleMarinePieces(bool t)
    {

        foreach (MarineArmorPiece map in marineArmorPieces)
            map.gameObject.SetActive(t);
    }

    public void HardReloadArmor(bool forceEnable = false)
    {
        Debug.Log("HardReloadArmor");

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
                    if (CurrentRoomManager.instance.PlayerExtendedDataContainsPlayerName(player.username))
                    //if (GameManager.instance.roomPlayerData.ContainsKey(player.nickName))
                    {
                        //Debug.Log($"PlayerArmorManager NOT MINE");
                        //Debug.Log($"PlayerArmorManager NOT MINE + {GameManager.instance.roomPlayerData[player.nickName].armorDataString}");
                        //armorDataString = GameManager.instance.roomPlayerData[player.nickName].armorDataString;
                        //colorPalette = GameManager.instance.roomPlayerData[player.nickName].playerBasicOnlineStats.armor_color_palette;


                        Debug.Log($"PlayerArmorManager NOT MINE + {CurrentRoomManager.instance.GetPLayerExtendedData(player.username)}");
                        armorDataString = CurrentRoomManager.instance.GetPLayerExtendedData(player.username).armor_data_string;
                        colorPalette = CurrentRoomManager.instance.GetPLayerExtendedData(player.username).armor_color_palette;
                    }
                }
            }
            else // You are in the menu
            {
                armorDataString = WebManager.webManagerInstance.pda.playerBasicOnlineStats.armor_data_string;
                colorPalette = WebManager.webManagerInstance.pda.playerBasicOnlineStats.armor_color_palette;
            }
        }

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
        {
            foreach (PlayerArmorPiece piece in playerArmorPieces)
                piece.gameObject.SetActive(false);
        }

        if (GameManager.instance.gameMode == GameManager.GameMode.Swarm)
        {
            foreach (MarineArmorPiece map in marineArmorPieces)
                map.gameObject.SetActive(true);
        }
    }

    void EnableAllArmorsInDataString(bool force = false)
    {
        if (GameManager.instance.gameMode == GameManager.GameMode.Swarm && !force && (SceneManager.GetActiveScene().buildIndex > 0)) return;

        Debug.Log("EnableAllArmorsInDataString");
        {
            foreach (PlayerArmorPiece piece in playerArmorPieces)
                piece.gameObject.SetActive(armorDataString.Contains(piece.entity));
        }
    }

    void UpdateColorPalette()
    {
        Debug.Log("UpdateColorPalette");
        Debug.Log(colorPalette);

        Texture _tex = GameManager.instance.colorPaletteTextures.Where(obj => obj.name.ToLower().Contains($"{colorPalette}")).SingleOrDefault();


        if (SceneManager.GetActiveScene().buildIndex > 0)
        {
            if (GameManager.instance.teamMode == GameManager.TeamMode.Classic)
            {
                Debug.Log($"{player.username}");

                string c = CurrentRoomManager.GetPlayerDataWithId(player.playerId).team.ToString().ToLower();
                _tex = GameManager.instance.colorPaletteTextures.Where(obj => obj.name.ToLower().Contains($"{c}")).SingleOrDefault();
            }
        }

        Debug.Log(_tex.name);

        if (playerArmorPieces.Count > 0)
            foreach (PlayerArmorPiece playerArmorPiece in playerArmorPieces)
            {
                if (playerArmorPiece.canChangeColorPalette)
                {
                    try
                    {
                        playerArmorPiece.GetComponent<Renderer>().material.SetTexture("_MainTex", _tex);
                    }
                    catch
                    {
                        Debug.LogWarning("Armor piece may not have a renderer");

                        foreach (Renderer r in playerArmorPiece.GetComponentsInChildren<Renderer>())
                            try
                            {
                                r.GetComponent<Renderer>().material.SetTexture("_MainTex", _tex);
                            }
                            catch { Debug.LogWarning("DID NOT FIND ANY RENDERER"); }
                    }
                }
            }
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

        if (!PreventReloadArmor)
            if (!isRagdoll)
            {
                HardReloadArmor();
                ReloadArmorTries++;

                if (ReloadArmorTries >= 0 && ReloadArmorTries < 3 /*&& (armorDataString == null || armorDataString == "")*/)
                    StartCoroutine(ReloadArmor_Coroutine());
            }
    }
}

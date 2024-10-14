using HarmonyLib;
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


    public GameObject weaponForServiceRecord;

    public ScriptObjPlayerData playerDataCell
    {
        get { return _playerDataCell; }
        set
        {
            Debug.Log("set playerDataCell");
            _playerDataCell = value;

            if (SceneManager.GetActiveScene().buildIndex == 0)
            {
                EnableAllArmorsInDataString();
                UpdateColorPalette();
            }
            else
            {
                if (GameManager.instance.gameMode == GameManager.GameMode.Versus)
                    EnableAllArmorsInDataString();
                else if (GameManager.instance.gameMode == GameManager.GameMode.Coop)
                {
                    if (!player.hasArmor)
                        ToggleMarinePieces(true);
                    else
                        EnableAllArmorsInDataString();
                }


                if (GameManager.instance.teamMode == GameManager.TeamMode.Classic)
                {
                    if (player && player.hasArmor)
                    {
                        ToggleMarinePieces(false);
                        EnableAllArmorsInDataString();
                    }
                    if (GameManager.instance.gameMode == GameManager.GameMode.Versus)
                        UpdateColorPalette(CurrentRoomManager.GetDataCellWithDatabaseIdAndRewiredId(player.playerId, playerDataCell.rewiredId).team.ToString().ToLower());
                    else
                        UpdateColorPalette();
                }
                else
                    UpdateColorPalette();
            }
        }
    }



    public bool isRagdoll { get { return GetComponent<PlayerRagdoll>(); } }
    public Material shieldMaterial { get { return _shieldMaterial; } }

    [SerializeField] ScriptObjPlayerData _playerDataCell;
    [SerializeField] Material _shieldMaterial;
    [SerializeField] GameObject _santaHat;

    public int ReloadArmorTries { get; set; }
    public bool PreventReloadArmor { get; set; }










    private void Awake()
    {
        Debug.Log("PlayerArmorManager Awake");
        marineArmorPieces.Clear();
        marineArmorPieces.AddRange(GetComponentsInChildren<MarineArmorPiece>(true));


        playerArmorPieces.Clear();
        playerArmorPieces.AddRange(GetComponentsInChildren<PlayerArmorPiece>(true));
        playerArmorPieces = playerArmorPieces.OrderByDescending(x => x.listingPriority).ToList();

        foreach (MarineArmorPiece map in marineArmorPieces)
            map.gameObject.SetActive(false);

        foreach (PlayerArmorPiece map in playerArmorPieces)
            map.gameObject.SetActive(false);

        if (SceneManager.GetActiveScene().buildIndex == 0) // If you disable the go in editor, Awake will trigger AFTER setting Data Cell
            gameObject.SetActive(false);


        if (_santaHat) _santaHat.SetActive(DateTime.Now >= new DateTime(DateTime.Now.Year, 10, 1) && DateTime.Now <= new DateTime(DateTime.Now.Year + 1, 3, 31));
    }

    private void Start()
    {
        if (player)
        {
            player.OnPlayerRespawnEarly -= OnPlayerRespawn;
            player.OnPlayerRespawnEarly += OnPlayerRespawn;
        }
    }






    void ToggleMarinePieces(bool t)
    {
        foreach (MarineArmorPiece map in marineArmorPieces)
            map.gameObject.SetActive(t);

        foreach (PlayerArmorPiece map in playerArmorPieces)
            map.gameObject.SetActive(false);
    }

    void EnableAllArmorsInDataString()
    {
        if (playerArmorPieces.Count == 0)
        {
            playerArmorPieces.AddRange(GetComponentsInChildren<PlayerArmorPiece>(true));
            playerArmorPieces = playerArmorPieces.OrderByDescending(x => x.listingPriority).ToList();
        }

        Debug.Log($"EnableAllArmorsInDataString: {playerDataCell.playerExtendedPublicData.armor_data_string}");
        {
            foreach (PlayerArmorPiece piece in playerArmorPieces)
            {
                try { piece.gameObject.SetActive(playerDataCell.playerExtendedPublicData.armor_data_string.Contains(piece.entity)); } catch (System.Exception e) { Debug.LogException(e); }
            }
        }
    }

    void UpdateColorPalette(string forcedColor = null)
    {
        Debug.Log("UpdateColorPalette");

        //Texture _tex = GameManager.instance.colorPaletteTextures.Where(obj => obj.name.ToLower().Contains($"{colorPalette}")).SingleOrDefault();
        Texture _tex = GameManager.instance.colorPaletteTextures.Where(obj => obj.name.ToLower().Contains(playerDataCell.playerExtendedPublicData.armor_color_palette)).SingleOrDefault();

        if (forcedColor != null)
            _tex = GameManager.instance.colorPaletteTextures.Where(obj => obj.name.ToLower().Contains(forcedColor)).SingleOrDefault();


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
                        //Debug.LogWarning("Armor piece may not have a renderer");

                        foreach (Renderer r in playerArmorPiece.GetComponentsInChildren<Renderer>())
                            try
                            {
                                r.GetComponent<Renderer>().material.SetTexture("_MainTex", _tex);
                            }
                            catch { /*Debug.LogWarning("DID NOT FIND ANY RENDERER");*/ }
                    }
                }
            }
    }


    public void ReloadFpsArmor()
    {
        // Only for active weapon. All disabled weapon will handle themselves

        Debug.Log("ReloadFpsArmor");
        foreach (PlayerArmorPiece pap in player.playerInventory.activeWeapon.GetComponentsInChildren<PlayerArmorPiece>(true))
            pap.gameObject.SetActive(player.playerArmorManager.playerDataCell.playerExtendedPublicData.armor_data_string.Contains(pap.entity));
    }



    public void ReloadArmorData()
    {
        playerDataCell = _playerDataCell;
    }

    void OnPlayerRespawn(Player p)
    {
        if (GameManager.instance.gameMode == GameManager.GameMode.Coop)
            ToggleMarinePieces(true);
    }
}

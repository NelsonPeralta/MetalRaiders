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
            if (!name.Contains("Ragdoll"))
            {
                _colorPalette = value;
            }
            UpdateColorPalette();
        }
    }

    [SerializeField] GameObject _helmetlessHead;
    [SerializeField] string _armorDataString, _colorPalette;

    int tries = 0;

    private void OnEnable()
    {
        _helmetlessHead.SetActive(false);
        HardReloadArmor();

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

    public void HardReloadArmor()
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
                    if (GameManager.instance.roomPlayerData.ContainsKey(player.nickName))
                    {
                        Debug.Log($"PlayerArmorManager NOT MINE");
                        Debug.Log($"PlayerArmorManager NOT MINE + {GameManager.instance.roomPlayerData[player.nickName].armorDataString}");
                        armorDataString = GameManager.instance.roomPlayerData[player.nickName].armorDataString;
                        colorPalette = GameManager.instance.roomPlayerData[player.nickName].playerBasicOnlineStats.armor_color_palette;
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
        EnableAllArmorsInDataString();
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
            _helmetlessHead.SetActive(true);
    }

    void EnableAllArmorsInDataString()
    {
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
        if (name.Contains("Ragdoll"))
            return;

        Texture _tex = GameManager.instance.colorPaletteTextures.Where(obj => obj.name.ToLower().Contains($"{colorPalette}")).SingleOrDefault();


        if (SceneManager.GetActiveScene().buildIndex > 0)
        {
            if (GameManager.instance.teamMode == GameManager.TeamMode.Classic)
            {
                string c = ((PlayerMultiplayerMatchStats.Team)GameManager.instance.teamDict[player.nickName]).ToString().ToLower();
                _tex = GameManager.instance.colorPaletteTextures.Where(obj => obj.name.ToLower().Contains($"{c}")).SingleOrDefault();
            }
        }

        //Debug.Log(_tex.name);

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
        yield return new WaitForSeconds(0.3f);
        HardReloadArmor();
        tries++;

        if (tries < 10 /*&& (armorDataString == null || armorDataString == "")*/)
            StartCoroutine(ReloadArmor_Coroutine());
    }
}

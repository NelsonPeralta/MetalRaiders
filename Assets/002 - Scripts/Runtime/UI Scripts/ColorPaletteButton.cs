using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class ColorPaletteButton : MonoBehaviour
{
    public void SelectThisColorPalette(string colorName)
    {
        Texture _tex = GameManager.instance.colorPaletteTextures.Where(obj => obj.name.ToLower().Contains($"{colorName}")).SingleOrDefault();

        print($"ColorPaletteButton {Launcher.instance.playerModel.GetComponent<PlayerArmorManager>().playerArmorPieces.Count}");
        foreach (PlayerArmorPiece playerArmorPiece in Launcher.instance.playerModel.GetComponent<PlayerArmorManager>().playerArmorPieces)
            if (playerArmorPiece.canChangeColorPalette)
            {
                try
                {
                    playerArmorPiece.GetComponent<Renderer>().material.SetTexture("_MainTex", _tex);
                }
                catch
                {
                    foreach (Renderer r in playerArmorPiece.GetComponentsInChildren<Renderer>())
                        try
                        {
                            r.GetComponent<Renderer>().material.SetTexture("_MainTex", _tex);
                        }
                        catch { /*Debug.LogWarning("DID NOT FIND ANY RENDERER");*/ }
                }
            }

        SaveColorPalette(colorName);
    }

    void SaveColorPalette(string name)
    {
        Debug.Log($"SaveColorPalette: {name}");
        MenuManager.Instance.GetMenu("armory").GetComponent<ArmoryManager>().playerDataCell.playerExtendedPublicData.armorColorPalette = name;

        StartCoroutine(WebManager.webManagerInstance.SaveArmorColorPalette_Coroutine(
            MenuManager.Instance.GetMenu("armory").GetComponent<ArmoryManager>().playerDataCell.playerExtendedPublicData.player_id,
            name));
    }
}

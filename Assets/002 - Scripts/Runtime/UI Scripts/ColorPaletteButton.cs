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

        foreach (PlayerArmorPiece playerArmorPiece in Launcher.instance.playerModel.GetComponent<PlayerArmorManager>().playerArmorPieces)
            if (playerArmorPiece.canChangeColorPalette)
                try
                {
                    playerArmorPiece.GetComponent<Renderer>().material.SetTexture("_MainTex", _tex);
                }
                catch { }

        SaveColorPalette(colorName);
    }

    void SaveColorPalette(string name)
    {
        Debug.Log($"SaveColorPalette: {name}");
        WebManager.webManagerInstance.pda.playerBasicOnlineStats.armor_color_palette = name;

        StartCoroutine(WebManager.webManagerInstance.SaveArmorColorPalette_Coroutine(name));

    }
}

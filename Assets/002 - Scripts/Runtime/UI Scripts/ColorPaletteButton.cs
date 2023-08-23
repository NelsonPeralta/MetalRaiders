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
        GameManager.instance.armorTex = _tex;

        foreach (PlayerArmorPiece playerArmorPiece in ArmoryManager.instance.playerModel.GetComponent<PlayerArmorManager>().playerArmorPieces)
            if (playerArmorPiece.canChangeColorPalette)
                try
                {
                    playerArmorPiece.GetComponent<Renderer>().material.SetTexture("_MainTex", _tex);
                }
                catch { }
    }
}

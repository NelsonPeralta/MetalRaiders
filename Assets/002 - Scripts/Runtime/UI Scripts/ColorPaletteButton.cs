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

        try
        {
            foreach (PlayerArmorPiece playerArmorPiece in ArmoryManager.instance.playerModel.GetComponent<PlayerArmorManager>().playerArmorPieces)
                if (playerArmorPiece.pieceType == PlayerArmorPiece.PieceType.Core)
                {
                    var materialsCopy = GetComponent<MeshRenderer>().materials;
                    materialsCopy[0] = GameManager.instance.armorMaterial;
                    GetComponent<MeshRenderer>().materials = materialsCopy;

                    playerArmorPiece.GetComponent<Material>().mainTexture = _tex;
                }
        }
        catch
        {

        }
    }
}

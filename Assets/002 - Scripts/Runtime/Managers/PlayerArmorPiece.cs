using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerArmorPiece : MonoBehaviour
{
    public enum BodyPart
    {
        Helmet,
        Chest,

        LeftShoulder,
        RightShoulder,

        LeftBicep,
        RightBicep,

        LeftForearm,
        RightForearm,

        LeftPalm,
        RightPalm,

        COD,

        LeftThigh,
        RightThigh,

        LeftKnee,
        RightKnee,

        LeftShin,
        RightShin,

        Feet,

        Effect
    }
    public enum PieceType
    {
        Core,
        Attachment
    }

    public PieceType pieceType;
    public BodyPart bodyPart;

    public string entity;
    public string cleanName;

    public int cost;



    public int minimumLevel { get { return _minimumLevel; } }
    public bool hideFromArmory { get { return _hideFromArmory; } }
    public bool canChangeColorPalette { get { return _canChangeColorPalette; } }

    [SerializeField] int _minimumLevel;
    [SerializeField] bool _hideFromArmory, _canChangeColorPalette;
}

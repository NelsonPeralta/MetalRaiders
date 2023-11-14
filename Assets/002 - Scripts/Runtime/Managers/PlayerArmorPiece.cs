using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
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



    public int minLvl { get { return _minimumLevel; } }
    public int minHonor { get { return _minHonor; } }
    public bool hideFromArmory { get { return _hideFromArmory; } }
    public bool canChangeColorPalette { get { return _canChangeColorPalette; } }

    [SerializeField] int _minimumLevel, _minHonor;
    [SerializeField] bool _hideFromArmory, _canChangeColorPalette;

    public int listingPriority
    {
        get
        {
            int p = 0;

            if (bodyPart == BodyPart.Helmet) p = 100;
            else if (bodyPart == BodyPart.Chest) p = 90;
            else if (bodyPart == BodyPart.LeftShoulder) p = 80;
            else if (bodyPart == BodyPart.LeftBicep) p = 70;
            else if (bodyPart == BodyPart.LeftForearm) p = 65;
            else if (bodyPart == BodyPart.RightShoulder) p = 60;
            else if (bodyPart == BodyPart.RightBicep) p = 50;
            else if (bodyPart == BodyPart.COD) p = 40;
            else if (bodyPart == BodyPart.LeftThigh) p = 30;
            else if (bodyPart == BodyPart.Effect) p = 20;

            return p;
        }
    }
}

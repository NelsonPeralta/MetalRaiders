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

        Feet
    }
    public enum PieceType
    {
        Core,
        Attachment
    }

    public PieceType pieceType;
    public BodyPart bodyPart;

    public string entity;
    public string uiName;

    public int cost;
}
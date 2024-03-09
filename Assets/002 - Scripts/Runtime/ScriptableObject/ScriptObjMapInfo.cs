using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "ScriptObjPlayerData", menuName = "ScriptableObjects/MapData", order = 2)]
public class ScriptObjMapInfo : ScriptableObject
{
    public string mapName;
    public int sceneBuildIndex;
    public Sprite image;
}

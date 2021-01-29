using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tags : MonoBehaviour
{
    public string[] tags = new string[5];

    public bool isPlayer = false;
    public bool isAI = false;

    public bool isHitbox = false;
    public bool isAlive =false;

    public bool redTeam = false;
    public bool blueTeam = false;
    public bool yellowTeam = false;
    public bool greenTeam = false;

    public bool hasTag(string param1)
    {
        foreach (string tag in tags)
        {
            if(param1 == tag)
            {
                return true;
            }
        }

        return false;
    }
}

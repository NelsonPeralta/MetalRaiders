using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyLibrary : MonoBehaviour
{
    public Player GetMyOnlinePlayer()
    {
        foreach(Player pp in FindObjectsOfType<Player>())
        {
            if (pp.PV.IsMine)
                return pp;
        }
        return null;
    }
}

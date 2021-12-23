using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyLibrary : MonoBehaviour
{
    public PlayerProperties GetMyOnlinePlayer()
    {
        foreach(PlayerProperties pp in FindObjectsOfType<PlayerProperties>())
        {
            if (pp.PV.IsMine)
                return pp;
        }
        return null;
    }
}

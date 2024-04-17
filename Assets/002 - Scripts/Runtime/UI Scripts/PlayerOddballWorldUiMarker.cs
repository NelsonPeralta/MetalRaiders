using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerOddballWorldUiMarker : MonoBehaviour
{
    [SerializeField] Player player;
    [SerializeField] GameObject _skullTag, _shieldTag;



    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        try
        {
            _skullTag.SetActive(player.playerInventory.activeWeapon.codeName == "oddball");
        }
        catch { }
    }
}

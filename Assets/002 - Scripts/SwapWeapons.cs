using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwapWeapons : MonoBehaviour
{
    public PlayerInventory pInventory;
    
    public void printnameofgun()
    {
        Debug.Log(pInventory.weaponsEquiped[0].name);
    }

    private void Update()
    {
        if(Input.GetAxis("Mouse ScrollWheel") > 0 || Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            Debug.Log("mouse works ");
        }

    }
}

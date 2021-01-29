using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponList : MonoBehaviour
{
    public PlayerInventory pI;

    [Header("All Weapons")]
    public string Weapon1 = "M4";

    public GameObject M4;

    public void Start()
    {
        pI = GetComponent<PlayerInventory>();

        //M4 = FindComp
    }
}

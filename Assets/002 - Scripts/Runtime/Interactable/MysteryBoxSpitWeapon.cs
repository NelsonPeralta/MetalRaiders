using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MysteryBoxSpitWeapon : MonoBehaviour
{
    [SerializeField] MysteryBox _mysteryBox;


    private void OnEnable()
    {
        _mysteryBox.SpitWeapon();
    }
}

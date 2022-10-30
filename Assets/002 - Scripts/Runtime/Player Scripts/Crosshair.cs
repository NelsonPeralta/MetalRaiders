using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crosshair : MonoBehaviour
{
    public enum Color { Red, Green, Blue }

    public Color color
    {
        get { return color; }
        set
        {
            color = value;

            if (value == Color.Blue)
            {
                redReticuleVersion.SetActive(false);
                greenReticuleVersion.SetActive(false);
            }
            else if (value == Color.Green)
            {
                greenReticuleVersion.SetActive(true);
                redReticuleVersion.SetActive(false);
            }
            else if (value == Color.Red)
            {
                greenReticuleVersion.SetActive(false);
                redReticuleVersion.SetActive(true);
            }
        }
    }

    public WeaponProperties.ReticuleType weaponReticule;
    [SerializeField] GameObject redReticuleVersion;
    [SerializeField] GameObject greenReticuleVersion;
}

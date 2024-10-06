using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crosshair : MonoBehaviour
{
    public enum Color { Red, Green, Blue }

    [SerializeField] Color _color;
    [SerializeField] GameObject blueReticuleVersion;
    [SerializeField] GameObject redReticuleVersion;
    [SerializeField] GameObject greenReticuleVersion;


    public Color color
    {
        get { return _color; }
        set
        {
            _color = value;

            if (transform.root.GetComponent<Player>() == GameManager.GetRootPlayer() && value == Color.Blue) print($"CROSSHAIR BLUE");

            if (value == Color.Blue)
            {
                try
                {
                    blueReticuleVersion.SetActive(true);
                    redReticuleVersion.SetActive(false);
                    greenReticuleVersion.SetActive(false);
                }
                catch { }
            }
            else if (value == Color.Green)
            {
                try
                {
                    blueReticuleVersion.SetActive(false);
                    greenReticuleVersion.SetActive(true);
                    redReticuleVersion.SetActive(false);
                }
                catch { }
            }
            else if (value == Color.Red)
            {
                try
                {
                    blueReticuleVersion.SetActive(false);
                    greenReticuleVersion.SetActive(false);
                    redReticuleVersion.SetActive(true);
                }
                catch { }
            }
        }
    }


    private void Start()
    {
        color = Color.Blue;
    }
}

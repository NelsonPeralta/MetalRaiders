using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testscript : MonoBehaviour
{
    public Camera cam;
    public HandgunScriptLPFP pistolScript;

    public void Start()
    {
        pistolScript.gunCamera = GameObject.FindGameObjectWithTag("Player_1_Camera").GetComponent<Camera>();
        cam = GameObject.FindGameObjectWithTag("Player_1_Camera").GetComponent<Camera>();
    }
}

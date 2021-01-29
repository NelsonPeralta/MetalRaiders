using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Aiming : MonoBehaviour
{
    [Header("MANUAL LINKING")]
    public PlayerController pController;
    public GameObject aimingBG;

    private bool hasFoundComponents = false;
    

    public void Update()
    {
        if (pController.isAiming == true)
        {
            aimingBG.SetActive(true);
        }

        if (pController.isAiming == false)
        {
            aimingBG.SetActive(false);
        }
    }


}

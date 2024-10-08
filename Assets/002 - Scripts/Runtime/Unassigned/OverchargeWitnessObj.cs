using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverchargeWitnessObj : MonoBehaviour
{
    public PlayerShooting playerShooting;
    public GameObject witness;
    public bool leftWeapon;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (playerShooting && witness)
        {
            if (!leftWeapon) witness.SetActive(playerShooting.overchargeReady);
            else if (leftWeapon) witness.SetActive(playerShooting.overchargeReadyLeftWeapon);
        }
    }
}

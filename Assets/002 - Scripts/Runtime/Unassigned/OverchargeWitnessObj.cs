using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverchargeWitnessObj : MonoBehaviour
{
    public PlayerShooting playerShooting;
    public GameObject witness;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (playerShooting && witness)
        {
            witness.SetActive(playerShooting.overchargeReady);
        }
    }
}

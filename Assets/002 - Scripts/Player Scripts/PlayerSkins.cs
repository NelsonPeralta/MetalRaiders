using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkins : MonoBehaviour
{
    public GameObject playerDefaultSkin;
    public GameObject[] armors;

    public void EnableArmor(string armorName, GameObject player)
    {
        foreach (GameObject skin in armors)
        {
            if (skin.name == armorName)
            {
                //player.GetComponent<PlayerController>().tPersonController = skin.GetComponent<ThirdPersonScript>();
                //player.GetComponent<Movement>().tPersonScripts = skin.GetComponent<ThirdPersonScript>();
                playerDefaultSkin.SetActive(false);
                skin.SetActive(true);
            }
        }
    }
}

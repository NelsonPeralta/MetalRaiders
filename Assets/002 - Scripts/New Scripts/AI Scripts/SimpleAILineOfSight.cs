using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleAILineOfSight : MonoBehaviour
{
    public bool playerInLineOfSight;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "player")
        {
            playerInLineOfSight = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "player")
        {
            playerInLineOfSight = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "player")
        {
            playerInLineOfSight = false;
        }
    }
}

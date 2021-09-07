using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ladder : MonoBehaviour
{
    public int defaultPlayerSlope;
    public int defaultPlayerStepOffset;
    public int defaultPlayerMovementSpeed;
    private void OnTriggerEnter(Collider other)
    {
        if (!other.GetComponent<CharacterController>())
            return;

        other.GetComponent<CharacterController>().slopeLimit = 180;
        other.GetComponent<CharacterController>().stepOffset = 1;
        other.GetComponent<Movement>().speed = other.GetComponent<Movement>().GetDefaultSpeed() / 10;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.GetComponent<Movement>())
            return;

        other.GetComponent<Movement>().ResetCharacterControllerProperties();
    }
}
